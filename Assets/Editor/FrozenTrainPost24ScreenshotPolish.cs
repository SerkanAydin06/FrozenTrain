#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Visual-only polish after the canonical 24 m architecture has been generated.
    /// It reacts to the real screenshot set without changing survival state, interaction ownership,
    /// the right-side corridor envelope or the inter-car stair/vestibule architecture.
    /// </summary>
    internal static class FrozenTrainPost24ScreenshotPolish
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__Post24ScreenshotPolishV1";
        private const float PartitionZ = 1.78f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Fabric;
            public Material Hazard;
            public Material Warm;
            public Material Screen;
            public Material Green;
            public Material Rubber;
        }

        public static void Apply()
        {
            Mats m = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++) PolishCar(i, m);
            PolishVestibule(m);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void PolishCar(int index, Mats m)
        {
            string path = PrefabRoot + "/" + L.Cars[index].Name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RemoveDirectChild(root.transform, MarkerName);
                Transform marker = Group(root.transform, MarkerName);

                switch (index)
                {
                    case 0: PolishLocomotive(root.transform, marker, m); break;
                    case 1: PolishGenerator(root.transform, marker, m); break;
                    case 2: PolishLiving(root.transform, marker, m); break;
                    case 3: PolishWorkshop(root.transform, marker, m); break;
                    case 4: PolishGreenhouse(root.transform, marker, m); break;
                }

                if (index > 0) PolishPublicCorridor(root.transform, marker, index, m);
                PolishResidential(root.transform, marker, index, m);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // ---- Locomotive ---------------------------------------------------------------------

        private static void PolishLocomotive(Transform root, Transform marker, Mats m)
        {
            // Keep the centreline walkable from the rear vestibule to the driving position. The added
            // operations island is deliberately offset to starboard instead of becoming another barrier.
            Transform cab = Group(marker, "LocomotiveOperationsZone");
            const float islandZ = -1.48f;

            Cube(cab, "OperationsIslandBase", new Vector3(1.55f, L.MainTop + .43f, islandZ),
                new Vector3(2.65f, .82f, 1.28f), m.Frame, true);
            Cube(cab, "OperationsIslandTop", new Vector3(1.55f, L.MainTop + .88f, islandZ),
                new Vector3(2.78f, .08f, 1.40f), m.Steel, false);
            Cube(cab, "OperationsIslandScreenA", new Vector3(.92f, L.MainTop + 1.13f, islandZ - .34f),
                new Vector3(.86f, .42f, .055f), m.Screen, false);
            Cube(cab, "OperationsIslandScreenB", new Vector3(2.16f, L.MainTop + 1.13f, islandZ + .34f),
                new Vector3(.86f, .42f, .055f), m.Screen, false);

            Cube(cab, "PortDiagnosticConsole", new Vector3(-1.55f, L.MainTop + .62f, 4.08f),
                new Vector3(2.70f, 1.12f, .46f), m.Frame, true);
            Cube(cab, "PortDiagnosticScreen", new Vector3(-1.55f, L.MainTop + 1.22f, 3.82f),
                new Vector3(2.05f, .48f, .045f), m.Screen, false);

            Cube(cab, "StarboardServiceConsole", new Vector3(5.55f, L.MainTop + .58f, -4.08f),
                new Vector3(2.35f, 1.05f, .46f), m.Frame, true);
            Cube(cab, "StarboardServiceScreen", new Vector3(5.55f, L.MainTop + 1.16f, -3.82f),
                new Vector3(1.72f, .42f, .045f), m.Screen, false);

            Cube(cab, "RearCrewBench", new Vector3(8.85f, L.MainTop + .28f, -1.95f),
                new Vector3(2.00f, .50f, .72f), m.Fabric, true);
            Cube(cab, "RearCrewBack", new Vector3(9.70f, L.MainTop + .77f, -1.95f),
                new Vector3(.16f, .92f, .72f), m.Fabric, true);

            Cube(cab, "FrontOperationsBand", new Vector3(-6.60f, L.MainTop + .025f, 0f),
                new Vector3(5.10f, .025f, 3.00f), m.Fabric, false);
            Cube(cab, "CentreOperationsBand", new Vector3(1.55f, L.MainTop + .025f, islandZ),
                new Vector3(3.60f, .025f, 2.20f), m.Floor, false);
        }

        // ---- Functional rooms ---------------------------------------------------------------

        private static void PolishGenerator(Transform root, Transform marker, Mats m)
        {
            SetPos(root, "OperatorDesk", -6.70f, -3.66f);
            SetPos(root, "GeneratorConsole", -6.70f, -3.98f);
            SetPos(root, "ControlTable", 1.30f, -3.62f);
            SetPos(root, "MonitoringRack", 7.55f, -3.88f);

            Transform room = Group(marker, "GeneratorRoomPolish");
            Cube(room, "GeneratorServiceAisle", new Vector3(0f, L.MainTop + .020f, -.95f),
                new Vector3(20.2f, .025f, 1.15f), m.Floor, false);
            foreach (float x in new[] { -8.7f, -2.8f, 3.7f, 9.3f })
            {
                Cube(room, "GeneratorStatusPanel", new Vector3(x, 1.70f, PartitionZ - .10f),
                    new Vector3(1.30f, .72f, .045f), m.Frame, false);
                Cube(room, "GeneratorStatusStrip", new Vector3(x, 1.91f, PartitionZ - .135f),
                    new Vector3(.92f, .10f, .025f), m.Hazard, false);
            }
        }

        private static void PolishLiving(Transform root, Transform marker, Mats m)
        {
            Transform[] bays = FindAll(root, "SeatingBay").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets =
            {
                new Vector3(-7.30f, L.MainTop, -3.02f),
                new Vector3(-2.55f, L.MainTop, -3.02f),
                new Vector3(2.45f, L.MainTop, -3.02f),
                new Vector3(7.15f, L.MainTop, -3.02f),
            };
            for (int i = 0; i < bays.Length && i < targets.Length; i++) bays[i].localPosition = targets[i];

            SetPos(root, "GalleyCounter", 6.35f, .78f);
            SetPos(root, "GalleyTop", 6.35f, .78f);
            MoveDetail(root, "__Production_GalleyDetails", 6.35f, .78f);
            SetPos(root, "MedicalCabinet", 9.75f, -3.86f);
            SetPos(root, "WallHeater", -10.20f, -4.08f);

            Transform room = Group(marker, "LivingRoomPolish");
            Cube(room, "LoungeRugA", new Vector3(-4.95f, L.MainTop + .020f, -2.15f),
                new Vector3(7.2f, .025f, 3.45f), m.Fabric, false);
            Cube(room, "DiningRugB", new Vector3(4.85f, L.MainTop + .020f, -2.15f),
                new Vector3(7.2f, .025f, 3.45f), m.Fabric, false);
            Cube(room, "GalleyBacksplash", new Vector3(6.35f, 1.38f, 1.50f),
                new Vector3(3.00f, .86f, .055f), m.Steel, false);
        }

        private static void PolishWorkshop(Transform root, Transform marker, Mats m)
        {
            Transform[] benches = FindAll(root, "Workbench").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets =
            {
                new Vector3(-7.70f, L.MainTop, -3.76f),
                new Vector3(-1.45f, L.MainTop, -3.76f),
                new Vector3(4.75f, L.MainTop, -3.76f),
            };
            for (int i = 0; i < benches.Length && i < targets.Length; i++) benches[i].localPosition = targets[i];

            SetPos(root, "MachineStation", 8.20f, -2.92f);
            SetPos(root, "MachineScreen", 8.20f, -2.16f);
            MoveDetail(root, "__Production_MachineStationDetails", 8.20f, -2.92f);
            SetPos(root, "ToolCabinet", 9.80f, -3.90f);

            Transform room = Group(marker, "WorkshopRoomPolish");
            Cube(room, "WorkshopServiceAisle", new Vector3(0f, L.MainTop + .020f, -.85f),
                new Vector3(20.2f, .025f, 1.20f), m.Floor, false);
            foreach (float x in new[] { -8.0f, -2.0f, 4.0f })
            {
                Cube(room, "WorkshopWallRail", new Vector3(x, 1.72f, -4.30f),
                    new Vector3(4.45f, .11f, .10f), m.Steel, false);
                Cube(room, "WorkshopToolBand", new Vector3(x, 2.08f, -4.30f),
                    new Vector3(3.70f, .20f, .045f), m.Hazard, false);
            }
        }

        private static void PolishGreenhouse(Transform root, Transform marker, Mats m)
        {
            Transform[] beds = FindAll(root, "HydroponicBed").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            var targets = new List<Vector3>();
            foreach (float z in new[] { -3.10f, -.20f })
                foreach (float x in new[] { -8.4f, -4.2f, 0f, 4.2f, 8.4f })
                    targets.Add(new Vector3(x, L.MainTop, z));

            for (int i = 0; i < beds.Length && i < targets.Count; i++) beds[i].localPosition = targets[i];
            SetPos(root, "ClimatePanel", 10.05f, 1.34f);
            SetPos(root, "ClimateScreen", 10.05f, 1.18f);

            Transform room = Group(marker, "GreenhouseRoomPolish");
            Cube(room, "GreenhouseServiceAisle", new Vector3(0f, L.MainTop + .020f, -1.66f),
                new Vector3(20.4f, .025f, .78f), m.Floor, false);
            foreach (float x in new[] { -8.4f, -4.2f, 0f, 4.2f, 8.4f })
            {
                Cube(room, "IrrigationCrossHeader", new Vector3(x, 2.50f, -1.66f),
                    new Vector3(.055f, .055f, 3.20f), m.Steel, false);
            }
        }

        // ---- Public corridor ---------------------------------------------------------------

        private static void PolishPublicCorridor(Transform root, Transform marker, int index, Mats m)
        {
            Transform corridor = FindFirst(root, "RightSidePublicCorridor");
            if (corridor == null) return;

            Transform detail = Group(marker, "PublicCorridorPolish");
            Material accent = Accent(index, m);
            foreach (float x in new[] { -8.2f, -4.1f, 4.1f, 8.2f })
            {
                Cube(detail, "CorridorIdentityPanel", new Vector3(x, 1.70f, PartitionZ - .055f),
                    new Vector3(1.20f, .54f, .030f), m.Frame, false);
                Cube(detail, "CorridorIdentityBand", new Vector3(x, 1.83f, PartitionZ - .075f),
                    new Vector3(.84f, .09f, .018f), accent, false);
            }
            Cube(detail, "WorkDoorBeacon", new Vector3(0f, 2.70f, PartitionZ - .07f),
                new Vector3(1.34f, .16f, .025f), accent, false);
            Point(detail, "WorkDoorLight", new Vector3(0f, 2.48f, 2.12f), .28f, 2.4f,
                index == 4 ? new Color(.66f, .90f, .70f) : new Color(.88f, .70f, .48f));
        }

        // ---- Residential -------------------------------------------------------------------

        private static void PolishResidential(Transform root, Transform marker, int index, Mats m)
        {
            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper == null) return;

            // Reuse the five existing no-shadow hall fill lights instead of spawning ten extra lights per car.
            foreach (Transform fill in FindAll(root, "UpperHallFill"))
            {
                Light light = fill.GetComponent<Light>();
                if (light == null) continue;
                light.intensity = .72f;
                light.range = 5.0f;
                light.color = new Color(.93f, .78f, .58f);
            }

            Transform detail = Group(marker, "ResidentialCabinPolish");
            Material accent = Accent(index, m);
            foreach (float x in L.SleepingPodX)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    float z = side * 3.05f;
                    Cube(detail, "CabinCeilingLight", new Vector3(x, L.RoofBottom - .0275f, z),
                        new Vector3(.82f, .055f, .24f), m.Warm, false);
                    Cube(detail, "CabinDoorMarker", new Vector3(x - .34f, 4.74f, side * (L.CorridorHalf + .0125f)),
                        new Vector3(.26f, .18f, .025f), accent, false);
                }
            }
        }

        // ---- Vestibule polish ---------------------------------------------------------------

        private static void PolishVestibule(Mats m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                RemoveDirectChild(root.transform, MarkerName);
                Transform marker = Group(root.transform, MarkerName);

                foreach (float x in new[] { -L.VestibuleLength * .5f + .10f, L.VestibuleLength * .5f - .10f })
                {
                    foreach (float side in new[] { -1f, 1f })
                        Cube(marker, "BellowsSideCollar", new Vector3(x, 1.55f, side * (L.HalfW + .035f)),
                            new Vector3(.20f, 9.25f, .14f), m.Rubber, false);
                    Cube(marker, "BellowsRoofCollar", new Vector3(x, L.RoofY + .03f, 0f),
                        new Vector3(.20f, .12f, L.CarWidth - .25f), m.Rubber, false);
                    Cube(marker, "BellowsUnderCollar", new Vector3(x, L.UnderframeBottom + .12f, 0f),
                        new Vector3(.22f, .18f, L.CarWidth - .90f), m.Rubber, false);
                }

                foreach (Transform rail in FindAll(root.transform, "StairHandrail"))
                {
                    Renderer r = rail.GetComponent<Renderer>();
                    if (r != null) r.sharedMaterial = m.Steel;
                }

                Cube(marker, "StairCoreMainFloorMark", new Vector3(0f, L.MainTop + .025f, -.72f),
                    new Vector3(3.05f, .028f, 1.15f), m.Fabric, false);
                Point(marker, "StairCoreMainFill", new Vector3(0f, 2.22f, -2.05f), .88f, 5.6f,
                    new Color(.76f, .83f, .92f));
                Point(marker, "StairCoreUpperFill", new Vector3(-.82f, 5.45f, -2.20f), .64f, 4.2f,
                    new Color(.88f, .80f, .68f));
                Point(marker, "StairCoreLowerFill", new Vector3(.82f, -1.18f, -2.20f), .58f, 4.0f,
                    new Color(.67f, .76f, .86f));

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // ---- Helpers ------------------------------------------------------------------------

        private static Mats LoadMaterials() => new Mats
        {
            Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)),
            Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
            Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
            Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
            Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
            Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
            Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f)),
            Screen = Mat("FT_PRO_Screen", new Color(.03f, .45f, .60f)),
            Green = Mat("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f)),
            Rubber = PrototypeMat("Train_Rubber", new Color(.055f, .06f, .065f)),
        };

        private static Material Accent(int index, Mats m)
        {
            if (index == 1) return Mat("FT_PRO_Generator", new Color(.34f, .105f, .028f));
            if (index == 3) return Mat("FT_PRO_Workshop", new Color(.34f, .245f, .075f));
            if (index == 4) return m.Green;
            return index == 2 ? m.Fabric : m.Hazard;
        }

        private static Material Mat(string name, Color fallback)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            EnsureFolder(MaterialRoot);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new System.InvalidOperationException("URP/Lit shader not found.");
            material = new Material(shader) { name = name, color = fallback };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", fallback);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material PrototypeMat(string name, Color fallback)
        {
            string path = "Assets/Materials/Prototype/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            return material != null ? material : Mat(name + "_Persistent", fallback);
        }

        private static void SetPos(Transform root, string name, float x, float z)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            p.x = x;
            p.z = z;
            t.localPosition = p;
        }

        private static void MoveDetail(Transform root, string name, float x, float z)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            p.x = x;
            p.z = z;
            t.localPosition = p;
        }

        private static Transform FindFirst(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindFirst(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static IEnumerable<Transform> FindAll(Transform root, string name)
        {
            if (root == null) yield break;
            if (root.name == name) yield return root;
            foreach (Transform child in root)
                foreach (Transform found in FindAll(child, name))
                    yield return found;
        }

        private static void RemoveDirectChild(Transform root, string name)
        {
            Transform child = root.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (!collider && c != null) Object.DestroyImmediate(c);
            return go;
        }

        private static void Point(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = Group(parent, name);
            t.localPosition = pos;
            Light l = t.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.intensity = intensity;
            l.range = range;
            l.color = color;
            l.shadows = LightShadows.None;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
