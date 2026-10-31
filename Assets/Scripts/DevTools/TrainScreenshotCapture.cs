#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FrozenTrain.Interaction;
using FrozenTrain.Player;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.DevTools
{
    public sealed class TrainScreenshotCapture : MonoBehaviour
    {
        // New installations capture automatically on every Editor Play.
        // User opt-out persists via EditorPrefs.
        // V2 preference enables capture even if the prior opt-in toggle was saved as false.
        private const string CapturePreference = "FrozenTrain.CaptureScreenshotsOnPlay.V2";
        private const string CaptureMenu = "Tools/FrozenTrain/Capture Screenshots on Play";
        private static bool Enabled => UnityEditor.EditorPrefs.GetBool(CapturePreference, true);

        [UnityEditor.MenuItem(CaptureMenu)]
        private static void ToggleCapture()
        {
            bool enabled = !Enabled;
            UnityEditor.EditorPrefs.SetBool(CapturePreference, enabled);
            UnityEditor.Menu.SetChecked(CaptureMenu, enabled);
            Debug.Log("[Screenshots] Capture on Play " + (enabled ? "enabled" : "disabled"));
        }

        [UnityEditor.MenuItem(CaptureMenu, true)]
        private static bool ValidateCaptureMenu()
        {
            UnityEditor.Menu.SetChecked(CaptureMenu, Enabled);
            return true;
        }

        private const string MainSceneName = "FPSPrototype";
        private const string TrainRootName = "TRAIN_INTERIOR_PROTOTYPE";
        private const int Width = 1600;
        private const int Height = 900;
        private const int JpgQuality = 92;
        private const int MaxShots = 60;
        private const float EyeHeight = 1.55f;
        private const float StartDelaySeconds = 2f;

        private struct View
        {
            public string Name, Label;
            public Vector3 Position, Target;
            public float Fov;
            public View(string name, string label, Vector3 pos, Vector3 target, float fov)
            {
                Name = name;
                Label = label;
                Position = pos;
                Target = target;
                Fov = fov;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Enabled || SceneManager.GetActiveScene().name != MainSceneName) return;
            GameObject host = new GameObject("__TrainScreenshotCapture") { hideFlags = HideFlags.DontSave };
            host.AddComponent<TrainScreenshotCapture>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(StartDelaySeconds);

            Camera original = FindPlayerCamera();
            Transform train = GameObject.Find(TrainRootName)?.transform;
            if (original == null || train == null)
            {
                Debug.LogWarning("[Screenshots] Player camera or train root missing.");
                Destroy(gameObject);
                yield break;
            }

            // Isolated camera: no player teleport, cursor unlocking or gameplay pause.
            GameObject cameraHost = new GameObject("__TrainQACamera");
            cameraHost.hideFlags = HideFlags.DontSave;
            Camera qaCamera = cameraHost.AddComponent<Camera>();
            qaCamera.CopyFrom(original);
            qaCamera.enabled = false;
            qaCamera.allowMSAA = true;
            UniversalAdditionalCameraData qaData = qaCamera.GetUniversalAdditionalCameraData();
            UniversalAdditionalCameraData playerData = original.GetUniversalAdditionalCameraData();
            qaData.renderPostProcessing = playerData.renderPostProcessing;
            qaData.antialiasing = playerData.antialiasing;
            qaData.antialiasingQuality = playerData.antialiasingQuality;

            RenderTexture rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                name = "FrozenTrain_QA_Capture"
            };
            Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "Screenshots"));
            Directory.CreateDirectory(folder);

            Physics.SyncTransforms();
            List<View> plan = BuildPlan(train);
            if (plan.Count == 0)
            {
                Debug.LogWarning("[Screenshots] No safe views found.");
                Cleanup(cameraHost, rt, image);
                Destroy(gameObject);
                yield break;
            }

            // One current set: remove previous generated images, never create dated archives.
            foreach (string previous in Directory.GetFiles(folder))
            {
                string ext = Path.GetExtension(previous).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png") File.Delete(previous);
            }

            StringBuilder report = new StringBuilder();
            report.AppendLine("# FrozenTrain – güncel QA ekran görüntüleri");
            report.AppendLine();
            report.AppendLine("Son çekim (UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            report.AppendLine("Kameralar vagon ve kat sınırları, vagon içi merdivenler ve kapılardan otomatik seçildi.");
            report.AppendLine("Yalnızca son çekim dosyaları tutulur. GitHub üzerinden görülebilmeleri için push gerekir.");
            report.AppendLine();
            report.AppendLine("| Görüntü | Bölüm | Kamera (dünya) |");
            report.AppendLine("|---|---|---|");

            Transform environment = GameObject.Find("OUTSIDE_ENVIRONMENT")?.transform;
            Transform near = environment?.Find("NEAR_SCENERY");
            Transform far = environment?.Find("DISTANT_SCENERY");
            bool previousNear = near != null && near.gameObject.activeSelf;
            bool previousFar = far != null && far.gameObject.activeSelf;
            RenderTexture activeBefore = RenderTexture.active;
            bool previousFog = RenderSettings.fog;
            int saved = 0;

            try
            {
                rt.Create();
                qaCamera.targetTexture = rt;
                foreach (View view in plan.Take(MaxShots))
                {
                    bool exterior = view.Name.StartsWith("tren_dis_", StringComparison.Ordinal);
                    if (near != null) near.gameObject.SetActive(exterior ? false : previousNear);
                    if (far != null) far.gameObject.SetActive(exterior ? false : previousFar);
                    // Full-length exterior views sit ~120 m away; linear fog would wash them out.
                    RenderSettings.fog = !exterior && previousFog;

                    qaCamera.transform.position = view.Position;
                    qaCamera.transform.rotation = Quaternion.LookRotation(view.Target - view.Position, Vector3.up);
                    qaCamera.fieldOfView = view.Fov;
                    qaCamera.Render();
                    RenderTexture.active = rt;
                    image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                    image.Apply(false, false);
                    File.WriteAllBytes(Path.Combine(folder, view.Name + ".jpg"), image.EncodeToJPG(JpgQuality));
                    report.AppendLine("| " + view.Name + ".jpg | " + view.Label + " | " + view.Position.ToString("F2") + " |");
                    saved++;
                    RenderTexture.active = activeBefore;
                    yield return null;
                }
            }
            finally
            {
                if (near != null) near.gameObject.SetActive(previousNear);
                if (far != null) far.gameObject.SetActive(previousFar);
                RenderSettings.fog = previousFog;
                RenderTexture.active = activeBefore;
                qaCamera.targetTexture = null;
                Cleanup(cameraHost, rt, image);
            }

            File.WriteAllText(Path.Combine(folder, "README.md"), report.ToString(), Encoding.UTF8);
            Debug.Log("[Screenshots] " + saved + " current scene-adaptive QA views: " + folder +
                " (GitHub upload requires git commit/push).");
            Destroy(gameObject);
        }

        private static void Cleanup(GameObject host, RenderTexture texture, Texture2D image)
        {
            if (texture != null) { texture.Release(); Destroy(texture); }
            if (image != null) Destroy(image);
            if (host != null) Destroy(host);
        }

        private static List<View> BuildPlan(Transform train)
        {
            List<View> result = new List<View>();
            Transform[] cars = train.Cast<Transform>()
                .Where(t => t.name.Contains("Locomotive") || t.name.Contains("Generator") ||
                    t.name.Contains("Living") || t.name.Contains("Workshop") || t.name.Contains("Greenhouse"))
                .OrderBy(t => t.position.x).ToArray();
            if (cars.Length == 0) return result;

            Renderer[] allCarRenderers = cars.SelectMany(t => t.GetComponentsInChildren<Renderer>(true)).ToArray();
            Bounds full = BoundsOf(allCarRenderers);
            Vector3 center = full.center;

            // Fit the whole train horizontally: distance from the 16:9 horizontal FOV plus 10% margin.
            const float sideFov = 40f;
            float halfH = sideFov * .5f * Mathf.Deg2Rad;
            float halfHorizontal = Mathf.Atan(Mathf.Tan(halfH) * Width / (float)Height);
            float sideDistance = full.extents.z + full.extents.x * 1.10f / Mathf.Tan(halfHorizontal);
            float sideHeight = full.extents.y * .60f;
            Add(result, "tren_dis_yan", "Tren dış yan (tam boy)",
                center + new Vector3(0f, sideHeight, -sideDistance), center, sideFov);
            Add(result, "tren_dis_karsi", "Tren karşı yan (tam boy)",
                center + new Vector3(0f, sideHeight, sideDistance), center, sideFov);
            Add(result, "tren_dis_on", "Lokomotif önü", center +
                new Vector3(-full.extents.x - 12f, full.extents.y * .50f, -full.extents.z - 8f),
                center + Vector3.left * (full.extents.x * .72f), 65f);

            for (int i = 0; i < cars.Length; i++)
                AddCarViews(result, cars[i], i + 1);

            // In-car ladders (upper deck in every car, technical deck in the locomotive).
            DeckLadder[] ladders = train.GetComponentsInChildren<DeckLadder>(true)
                .OrderBy(l => l.transform.position.x).ToArray();
            foreach (DeckLadder ladder in ladders)
            {
                int carIndex = Array.FindIndex(cars, c => ladder.transform.IsChildOf(c)) + 1;
                AddLadderViews(result, ladder.transform, carIndex);
            }

            // Sample representative door panels from throughout the train.
            Transform[] doors = train.GetComponentsInChildren<Transform>(true)
                .Where(t => t.GetComponent<SlidingDoor>() != null)
                .OrderBy(t => t.position.x).ToArray();
            int stride = Mathf.Max(1, Mathf.CeilToInt(doors.Length / 8f));
            int doorNumber = 0;
            for (int i = 0; i < doors.Length && doorNumber < 8; i += stride)
            {
                if (!TryDoorView(doors[i], out Vector3 eye, out Vector3 target)) continue;
                Add(result, "kapi_" + (++doorNumber).ToString("00"),
                    doors[i].name + " yüzey kontrolü", eye, target, 62f);
            }
            return result;
        }

        private static void AddCarViews(List<View> views, Transform car, int index)
        {
            Transform shell = car.Find("Shell");
            if (shell == null) return;
            Bounds hull = BoundsOf(shell.GetComponentsInChildren<Renderer>(true));
            string[] decks = { "LowerFloor", "MainFloor", "UpperFloor" };
            for (int deck = 0; deck < decks.Length; deck++)
            {
                Renderer[] floorParts = shell.GetComponentsInChildren<Renderer>(true)
                    .Where(r => r.name.StartsWith(decks[deck], StringComparison.Ordinal)).ToArray();
                if (floorParts.Length == 0) continue;
                Bounds floor = BoundsOf(floorParts);
                float y = floor.max.y + EyeHeight;
                float z = floor.center.z;
                if (deck == 1 && index > 1)
                    z += floor.extents.z * .65f;

                for (int side = 0; side < 2; side++)
                {
                    float sign = side == 0 ? 1f : -1f;
                    Vector3 candidate = new Vector3(car.position.x + sign * hull.extents.x * .76f, y, z);
                    if (!TrySafeEye(candidate, out Vector3 eye)) continue;
                    Vector3 lookAt = new Vector3(car.position.x - sign * hull.extents.x * .55f, y - .10f, z);
                    Add(views, "vagon" + index + "_kat" + (deck + 1) +
                        (side == 0 ? "" : "_ters"), car.name + " kat " + (deck + 1), eye, lookAt, 75f);
                }
            }
        }

        private static void AddLadderViews(List<View> views, Transform ladder, int carIndex)
        {
            Transform low = ladder.Find("LowerExit"), high = ladder.Find("UpperExit");
            Renderer[] parts = ladder.GetComponentsInChildren<Renderer>(true);
            if (low == null || high == null || parts.Length == 0) return;
            Bounds b = BoundsOf(parts);
            string kind = ladder.name.StartsWith("Technical", StringComparison.Ordinal) ? "teknik" : "ust";
            string label = "Vagon " + carIndex + " – " + (kind == "teknik" ? "teknik kat merdiveni" : "üst kat merdiveni");

            // From the lower landing: step back from the wall-mounted ladder and look at it.
            Vector3 away = new Vector3(-.55f, 0f, -Mathf.Sign(ladder.position.z) * .95f);
            if (TrySafeEye(low.position + away + Vector3.up * EyeHeight, out Vector3 eye))
                Add(views, "merdiven_vagon" + carIndex + "_" + kind + "_alt", label + " (alt giriş)", eye,
                    new Vector3(b.center.x, low.position.y + 1.30f, b.center.z), 72f);

            // From the upper landing: look down into the guarded hatch.
            if (TrySafeEye(high.position + new Vector3(-.60f, 0f, 0f) + Vector3.up * EyeHeight, out eye))
                Add(views, "merdiven_vagon" + carIndex + "_" + kind + "_ust", label + " (üst çıkış)", eye,
                    new Vector3(b.center.x, high.position.y, b.center.z), 72f);
        }

        /// <summary>
        /// Looks at a door panel from either face along its normal and only accepts a camera point
        /// with a clear line of sight to that same door (not the back of an opposite door or a wall).
        /// </summary>
        private static bool TryDoorView(Transform door, out Vector3 eye, out Vector3 target)
        {
            eye = target = default;
            Renderer render = door.GetComponentInChildren<Renderer>(true);
            if (render == null) return false;
            Bounds b = render.bounds;
            Vector3 normal = b.size.x < b.size.z ? Vector3.right : Vector3.forward;
            target = b.center;
            float eyeY = b.min.y + EyeHeight;

            foreach (float distance in new[] { 1.70f, 1.30f, 2.20f, .95f })
                foreach (float sign in new[] { 1f, -1f })
                {
                    Vector3 candidate = b.center + normal * sign * distance;
                    candidate.y = eyeY;
                    if (Physics.CheckSphere(candidate, .17f, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore)) continue;
                    Vector3 toDoor = target - candidate;
                    if (!Physics.Raycast(candidate, toDoor.normalized, out RaycastHit hit,
                            toDoor.magnitude + .25f, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore)) continue;
                    if (!hit.transform.IsChildOf(door)) continue;
                    eye = candidate;
                    return true;
                }
            return false;
        }

        private static bool TrySafeEye(Vector3 preferred, out Vector3 eye)
        {
            // Probe several positions instead of putting the camera inside walls.
            Vector3[] shifts =
            {
                Vector3.zero,
                Vector3.right * .40f, Vector3.left * .40f,
                Vector3.forward * .40f, Vector3.back * .40f,
                Vector3.right * .85f, Vector3.left * .85f,
                Vector3.forward * .85f, Vector3.back * .85f
            };
            foreach (Vector3 move in shifts)
            {
                Vector3 candidate = preferred + move;
                if (!Physics.CheckSphere(candidate, .17f, Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    eye = candidate;
                    return true;
                }
            }
            eye = default;
            return false;
        }

        private static Bounds BoundsOf(IEnumerable<Renderer> renderers)
        {
            Bounds result = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;
            foreach (Renderer r in renderers)
            {
                if (r == null || !r.enabled) continue;
                if (!hasBounds) { result = r.bounds; hasBounds = true; }
                else result.Encapsulate(r.bounds);
            }
            return result;
        }

        private static void Add(List<View> views, string name, string label,
            Vector3 eye, Vector3 target, float fov)
        {
            if (Vector3.SqrMagnitude(target - eye) < .1f) return;
            if (views.Any(v => v.Name == name)) return;
            views.Add(new View(name, label, eye, target, fov));
        }

        private static Camera FindPlayerCamera()
        {
            FpsPlayerController player = FindFirstObjectByType<FpsPlayerController>();
            Camera cam = player != null ? player.GetComponentInChildren<Camera>(true) : null;
            return cam != null ? cam : Camera.main;
        }
    }
}
#endif
