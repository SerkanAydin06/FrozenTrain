#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using FrozenTrain.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.DevTools
{
    public sealed class TrainScreenshotCapture : MonoBehaviour
    {
        public static readonly bool Enabled = true;

        private const string MainSceneName = "FPSPrototype";
        private const string TrainRootName = "TRAIN_INTERIOR_PROTOTYPE";
        private const int Width = 1280;
        private const int Height = 720;
        private const int JpgQuality = 84;
        private const float StartDelaySeconds = 2.5f;
        private const float ReviewFov = 74f;
        private static readonly string[] CarNames = { "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse" };
        private static readonly string[] CarLabels = { "Lokomotif", "Jeneratör", "Yaşam", "Atölye", "Sera" };
        private const float CarSpacing = 28f;
        private const float VestibuleLength = 4f;
        private static readonly float[] DeckFloorY = { -3.0f, 0.11f, 3.24f };
        private static readonly string[] DeckLabels = { "alt / teknik", "orta / özel kabin veya sağ transit koridor", "üst / özel konut" };
        private const float EyeHeight = 1.55f;
        private const float EndInset = 10.45f;
        private const float MainCorridorZ = 3.15f;
        private const float MainCompartmentZ = -1.35f;
        private const float UpperCabinX = -7.15f;

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
            Camera cam = FindPlayerCamera();
            if (cam == null) { Debug.LogWarning("[Screenshots] Kamera bulunamadı; görüntü alınmadı."); Destroy(gameObject); yield break; }

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "Screenshots"));
            Directory.CreateDirectory(folder);
            // Never mix obsolete 18 m / in-car-stair screenshots with the current architecture.
            foreach (string old in Directory.GetFiles(folder, "*.jpg")) File.Delete(old);
            foreach (string old in Directory.GetFiles(folder, "*.jpeg")) File.Delete(old);

            FpsPlayerController controller = FindFirstObjectByType<FpsPlayerController>();
            PlayerInteraction interaction = FindFirstObjectByType<PlayerInteraction>();
            TrainManagementPanel panel = FindFirstObjectByType<TrainManagementPanel>();
            if (panel != null) panel.ClosePanel();
            bool controllerWasEnabled = controller != null && controller.enabled;
            bool interactionWasEnabled = interaction != null && interaction.enabled;
            bool panelWasEnabled = panel != null && panel.enabled;
            if (controller != null) controller.enabled = false;
            if (interaction != null) interaction.enabled = false;
            if (panel != null) panel.enabled = false;
            CursorLockMode previousLock = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;

            Renderer[] playerRenderers = controller != null ? controller.GetComponentsInChildren<Renderer>() : new Renderer[0];
            bool[] rendererStates = new bool[playerRenderers.Length];
            for (int i = 0; i < playerRenderers.Length; i++) { rendererStates[i] = playerRenderers[i].enabled; playerRenderers[i].enabled = false; }

            Transform camTransform = cam.transform;
            Transform originalParent = camTransform.parent;
            Vector3 originalLocalPos = camTransform.localPosition;
            Quaternion originalLocalRot = camTransform.localRotation;
            float originalFov = cam.fieldOfView;
            RenderTexture originalTarget = cam.targetTexture;
            RenderTexture rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 4 };
            Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            StringBuilder manifest = new StringBuilder();
            manifest.AppendLine("# FrozenTrain otomatik görüntüler\n");
            manifest.AppendLine("Son çekim: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
            manifest.AppendLine("Yeni mimari: 5 x 24 m vagon + 4 x 4 m ara vestibül. Vagon içlerinde normal merdiven yoktur. `gecisN_*` görüntüleri vestibül geçişini ve üst/alt merdiven çekirdeğini kontrol eder.\n");
            manifest.AppendLine("| Dosya | Açıklama | Kamera (dünya) |\n|---|---|---|");

            camTransform.SetParent(null, true);
            cam.fieldOfView = ReviewFov;
            cam.targetTexture = rt;
            int saved = 0;
            GameObject trainObject = GameObject.Find(TrainRootName);
            Transform trainRoot = trainObject != null ? trainObject.transform : null;

            for (int car = 0; car < CarNames.Length; car++)
            {
                Transform carTransform = trainRoot != null ? trainRoot.Find(CarNames[car]) : null;
                for (int deck = 0; deck < DeckFloorY.Length; deck++)
                {
                    float y = DeckFloorY[deck] + EyeHeight;
                    float z = deck == 1 ? (car == 0 ? 0f : MainCorridorZ) : 0f;
                    string baseName = "vagon" + (car + 1) + "_kat" + (deck + 1);
                    string label = CarLabels[car] + ", kat " + (deck + 1) + " (" + DeckLabels[deck] + ")";
                    yield return Shoot(cam, image, rt, folder, baseName, label + ", öne bakış", carTransform, car,
                        new Vector3(EndInset, y, z), -90f, manifest, () => saved++);
                    yield return Shoot(cam, image, rt, folder, baseName + "_ters", label + ", arkaya bakış", carTransform, car,
                        new Vector3(-EndInset, y, z), 90f, manifest, () => saved++);
                }

                float mainY = DeckFloorY[1] + EyeHeight;
                yield return Shoot(cam, image, rt, folder, "vagon" + (car + 1) + "_kat2_bolme",
                    car == 0 ? "Lokomotif tam özel sürüş kabini" : CarLabels[car] + ", 24 m kapalı işlev bölümü", carTransform, car,
                    new Vector3(9.25f, mainY, car == 0 ? 0f : MainCompartmentZ), -90f, manifest, () => saved++);

                float upperY = DeckFloorY[2] + EyeHeight;
                yield return Shoot(cam, image, rt, folder, "vagon" + (car + 1) + "_kat3_oda",
                    CarLabels[car] + ", 24 m üst konut örnek oda/koridor", carTransform, car,
                    new Vector3(UpperCabinX, upperY, 0.02f), 0f, manifest, () => saved++);
            }

            for (int gap = 0; gap < CarNames.Length - 1; gap++)
            {
                float vestibuleX = ((gap - 1.5f) * CarSpacing);
                Vector3 origin = trainRoot != null ? trainRoot.TransformPoint(new Vector3(vestibuleX, 0f, 0f)) : new Vector3(vestibuleX, 0f, 0f);
                string prefix = "gecis" + (gap + 1);

                yield return ShootWorld(cam, image, rt, folder, prefix + "_kat2",
                    "Ara vestibül " + (gap + 1) + ", sağ ana geçiş", origin + new Vector3(-VestibuleLength * .32f, DeckFloorY[1] + EyeHeight, MainCorridorZ),
                    Quaternion.Euler(5f, 90f, 0f), manifest, () => saved++);

                yield return ShootWorld(cam, image, rt, folder, prefix + "_merdiven",
                    "Ara vestibül " + (gap + 1) + ", sol merdiven çekirdeği", origin + new Vector3(0f, DeckFloorY[1] + EyeHeight, 3.30f),
                    Quaternion.Euler(5f, 180f, 0f), manifest, () => saved++);

                yield return ShootWorld(cam, image, rt, folder, prefix + "_kat3",
                    "Ara vestibül " + (gap + 1) + ", üst konut sahanlığı", origin + new Vector3(0f, DeckFloorY[2] + EyeHeight, .95f),
                    Quaternion.Euler(5f, 180f, 0f), manifest, () => saved++);

                yield return ShootWorld(cam, image, rt, folder, prefix + "_kat1",
                    "Ara vestibül " + (gap + 1) + ", alt teknik sahanlık", origin + new Vector3(0f, DeckFloorY[0] + EyeHeight, .95f),
                    Quaternion.Euler(5f, 180f, 0f), manifest, () => saved++);
            }

            Vector3 trainCenter = trainRoot != null ? trainRoot.position : Vector3.zero;
            yield return ShootWorld(cam, image, rt, folder, "tren_dis_yan", "Tüm 136 m tren, yandan",
                trainCenter + new Vector3(0f, 18f, -72f), Quaternion.Euler(10f, 0f, 0f), manifest, () => saved++);
            yield return ShootWorld(cam, image, rt, folder, "tren_dis_on", "Uzun lokomotif önü, yakın çapraz",
                trainCenter + new Vector3(-2f * CarSpacing - 18f, 5.5f, -16f), Quaternion.Euler(7f, 54f, 0f), manifest, () => saved++);

            File.WriteAllText(Path.Combine(folder, "README.md"), manifest.ToString(), Encoding.UTF8);
            cam.targetTexture = originalTarget;
            cam.fieldOfView = originalFov;
            camTransform.SetParent(originalParent, false);
            camTransform.localPosition = originalLocalPos;
            camTransform.localRotation = originalLocalRot;
            for (int i = 0; i < playerRenderers.Length; i++) if (playerRenderers[i] != null) playerRenderers[i].enabled = rendererStates[i];
            if (controller != null) controller.enabled = controllerWasEnabled;
            if (interaction != null) interaction.enabled = interactionWasEnabled;
            if (panel != null) panel.enabled = panelWasEnabled;
            Cursor.lockState = previousLock;
            rt.Release(); Destroy(rt); Destroy(image);
            Debug.Log("[Screenshots] " + saved + " görüntü kaydedildi: " + folder);
            Destroy(gameObject);
        }

        private IEnumerator Shoot(Camera cam, Texture2D image, RenderTexture rt, string folder, string fileName, string label,
            Transform carTransform, int carIndex, Vector3 local, float yaw, StringBuilder manifest, System.Action onSaved)
        {
            Vector3 position;
            Quaternion rotation;
            if (carTransform != null) { position = carTransform.TransformPoint(local); rotation = carTransform.rotation * Quaternion.Euler(5f, yaw, 0f); }
            else { position = new Vector3((carIndex - 2) * CarSpacing + local.x, local.y, local.z); rotation = Quaternion.Euler(5f, yaw, 0f); }
            yield return ShootWorld(cam, image, rt, folder, fileName, label, position, rotation, manifest, onSaved);
        }

        private static IEnumerator ShootWorld(Camera cam, Texture2D image, RenderTexture rt, string folder, string fileName, string label,
            Vector3 position, Quaternion rotation, StringBuilder manifest, System.Action onSaved)
        {
            cam.transform.SetPositionAndRotation(position, rotation);
            for (int i = 0; i < 4; i++) yield return null;
            yield return new WaitForEndOfFrame();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply(false);
            RenderTexture.active = previous;
            File.WriteAllBytes(Path.Combine(folder, fileName + ".jpg"), image.EncodeToJPG(JpgQuality));
            manifest.AppendLine("| " + fileName + ".jpg | " + label + " | " + position.ToString("F1") + " |");
            onSaved();
        }

        private static Camera FindPlayerCamera()
        {
            FpsPlayerController controller = FindFirstObjectByType<FpsPlayerController>();
            Camera cam = controller != null ? controller.GetComponentInChildren<Camera>(true) : null;
            return cam != null ? cam : Camera.main;
        }
    }
}
#endif