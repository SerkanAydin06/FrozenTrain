#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Applies the locked five-car architectural design directly to authored prefab assets.
    /// Runtime gameplay never creates this geometry.
    /// </summary>
    public static class FrozenTrainFiveCarDesignAuthoring
    {
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const float CarLength = 16.5f;
        private const float CarWidth = 8.4f;
        private const float CarHeight = 3.9f;
        private const float WallThickness = 0.20f;
        private const float SideZ = CarWidth * 0.5f;
        private const float WindowBottom = 1.38f;
        private const float WindowTop = 2.72f;
        private const float WindowHeight = WindowTop - WindowBottom;

        [MenuItem("Frozen Train/Apply Five-Car Architectural Design", priority = 4)]
        public static void ApplyFromMenu()
        {
            FrozenTrainAuthoredLayoutResize.ResizeAuthoredTrain();
            ApplyAll();
            FrozenTrainPhysicalSystemsAuthoring.InstallOrRefresh();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain five-car architectural design applied: selective windows, enclosed technical cars, clear sight-lines and functional layouts.");
        }

        public static void ApplyAll()
        {
            Material wall = Mat("Train_Wall");
            Material frame = Mat("Train_Frame");
            Material steel = Mat("Train_Steel");
            Material rubber = Mat("Train_Rubber");
            Material generator = Mat("Train_Generator");
            Material living = Mat("Train_Living");
            Material mattress = Mat("Train_Mattress");
            Material workshop = Mat("Train_Workshop");
            Material greenhouse = Mat("Train_Greenhouse");
            Material plant = Mat("Train_Plant");
            Material screen = Mat("Train_Screen");
            Material amber = Mat("Train_Amber");
            Material glass = Mat("Train_WindowGlass");

            ConfigurePrefab("01_Locomotive", root =>
            {
                RebuildSideShell(root, SideProfile.LocomotiveCab, wall, frame, glass);
                ClearInterior(root, "Locomotive_Interior");
                BuildLocomotive(root.transform, frame, steel, screen, amber, rubber);
            });

            ConfigurePrefab("02_Generator", root =>
            {
                RebuildSideShell(root, SideProfile.Sealed, wall, frame, glass);
                ClearInterior(root, "Generator_Interior");
                BuildGenerator(root.transform, generator, frame, steel, screen, amber);
            });

            ConfigurePrefab("03_Living", root =>
            {
                RebuildSideShell(root, SideProfile.LivingLounge, wall, frame, glass);
                ClearInterior(root, "Living_Interior");
                BuildLiving(root.transform, living, mattress, frame, steel, amber);
            });

            ConfigurePrefab("04_Workshop", root =>
            {
                RebuildSideShell(root, SideProfile.Sealed, wall, frame, glass);
                ClearInterior(root, "Workshop_Interior");
                BuildWorkshop(root.transform, workshop, frame, steel, screen);
            });

            ConfigurePrefab("05_Greenhouse", root =>
            {
                RebuildSideShell(root, SideProfile.Sealed, wall, frame, glass);
                ClearInterior(root, "Greenhouse_Interior");
                BuildGreenhouse(root.transform, greenhouse, plant, frame, steel, amber, screen);
            });
        }

        private enum SideProfile
        {
            Sealed,
            LocomotiveCab,
            LivingLounge
        }

        private static void ConfigurePrefab(string prefabName, System.Action<GameObject> configure)
        {
            string path = TrainPrefabFolder + "/" + prefabName + ".prefab";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning("FrozenTrain: prefab missing for architectural design: " + path);
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                configure?.Invoke(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RebuildSideShell(GameObject root, SideProfile profile, Material wall, Material frame, Material glass)
        {
            var removeNames = new HashSet<string>
            {
                "WallLower", "WallUpper", "WallPillar",
                "WindowTopFrame", "WindowBottomFrame", "WindowLeftFrame", "WindowRightFrame", "WindowGlass",
                "ArchitecturalSideLeft", "ArchitecturalSideRight"
            };

            var directChildren = new List<Transform>();
            for (int i = 0; i < root.transform.childCount; i++)
                directChildren.Add(root.transform.GetChild(i));

            foreach (Transform child in directChildren)
            {
                if (removeNames.Contains(child.name))
                    Object.DestroyImmediate(child.gameObject);
            }

            BuildSide(root.transform, -SideZ, profile, wall, frame, glass, "ArchitecturalSideLeft");
            BuildSide(root.transform, SideZ, profile, wall, frame, glass, "ArchitecturalSideRight");
        }

        private static void BuildSide(Transform car, float z, SideProfile profile, Material wall, Material frame, Material glass, string rootName)
        {
            GameObject side = new GameObject(rootName);
            side.transform.SetParent(car, false);

            if (profile == SideProfile.Sealed)
            {
                Cube(side.transform, "SolidSideWall", new Vector3(0f, CarHeight * 0.5f, z), new Vector3(CarLength, CarHeight, WallThickness), wall);
                AddWallRibs(side.transform, z, frame);
                return;
            }

            float lowerHeight = WindowBottom;
            float upperHeight = CarHeight - WindowTop;
            Cube(side.transform, "WallLower", new Vector3(0f, lowerHeight * 0.5f, z), new Vector3(CarLength, lowerHeight, WallThickness), wall);
            Cube(side.transform, "WallUpper", new Vector3(0f, WindowTop + upperHeight * 0.5f, z), new Vector3(CarLength, upperHeight, WallThickness), wall);

            List<(float center, float width)> windows = profile == SideProfile.LocomotiveCab
                ? new List<(float, float)> { (-5.9f, 2.15f), (-3.25f, 2.15f) }
                : new List<(float, float)> { (-2.75f, 2.25f), (0f, 2.25f), (2.75f, 2.25f) };

            FillWindowBand(side.transform, z, windows, wall, frame, glass);
        }

        private static void FillWindowBand(Transform side, float z, List<(float center, float width)> windows, Material wall, Material frame, Material glass)
        {
            float bandCenter = (WindowBottom + WindowTop) * 0.5f;
            float left = -CarLength * 0.5f;

            foreach ((float center, float width) in windows.OrderBy(w => w.center))
            {
                float openingLeft = center - width * 0.5f;
                float fill = openingLeft - left;
                if (fill > 0.01f)
                    Cube(side, "SolidWindowBand", new Vector3(left + fill * 0.5f, bandCenter, z), new Vector3(fill, WindowHeight, WallThickness), wall);

                Window(side, center, width, z, frame, glass);
                left = center + width * 0.5f;
            }

            float remaining = CarLength * 0.5f - left;
            if (remaining > 0.01f)
                Cube(side, "SolidWindowBand", new Vector3(left + remaining * 0.5f, bandCenter, z), new Vector3(remaining, WindowHeight, WallThickness), wall);
        }

        private static void Window(Transform parent, float x, float width, float z, Material frame, Material glass)
        {
            float y = (WindowBottom + WindowTop) * 0.5f;
            float inward = z > 0f ? -1f : 1f;
            Cube(parent, "WindowTopFrame", new Vector3(x, WindowTop, z), new Vector3(width + 0.16f, 0.10f, 0.14f), frame);
            Cube(parent, "WindowBottomFrame", new Vector3(x, WindowBottom, z), new Vector3(width + 0.16f, 0.10f, 0.14f), frame);
            Cube(parent, "WindowLeftFrame", new Vector3(x - width * 0.5f, y, z), new Vector3(0.10f, WindowHeight, 0.14f), frame);
            Cube(parent, "WindowRightFrame", new Vector3(x + width * 0.5f, y, z), new Vector3(0.10f, WindowHeight, 0.14f), frame);
            Cube(parent, "WindowGlass", new Vector3(x, y, z + inward * 0.025f), new Vector3(width - 0.12f, WindowHeight - 0.12f, 0.035f), glass);
        }

        private static void AddWallRibs(Transform parent, float z, Material frame)
        {
            for (float x = -6.2f; x <= 6.2f; x += 3.1f)
                Cube(parent, "WallRib", new Vector3(x, CarHeight * 0.5f, z + (z > 0f ? -0.035f : 0.035f)), new Vector3(0.10f, CarHeight - 0.25f, 0.10f), frame);
        }

        private static void ClearInterior(GameObject root, string groupName)
        {
            Transform old = FindDeep(root.transform, groupName);
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
        }

        private static void BuildLocomotive(Transform car, Material frame, Material steel, Material screen, Material amber, Material rubber)
        {
            Transform g = Group(car, "Locomotive_Interior");

            Cube(g, "MainConsole", new Vector3(-6.35f, 0.82f, 0f), new Vector3(1.15f, 1.42f, 5.30f), frame);
            for (int i = -2; i <= 2; i++)
                Cube(g, "MainDisplay", new Vector3(-5.74f, 1.38f, i * 0.95f), new Vector3(0.045f, 0.52f, 0.70f), screen);

            Seat(g, new Vector3(-4.35f, 0.48f, -1.20f), frame, rubber);
            Seat(g, new Vector3(-4.35f, 0.48f, 1.20f), frame, rubber);

            Cabinet(g, new Vector3(1.15f, 1.25f, -3.72f), new Vector3(3.0f, 2.30f, 0.62f), frame, steel);
            Cabinet(g, new Vector3(4.65f, 1.25f, -3.72f), new Vector3(2.8f, 2.30f, 0.62f), frame, steel);
            Cabinet(g, new Vector3(2.25f, 1.25f, 3.72f), new Vector3(2.4f, 2.30f, 0.62f), frame, steel);

            GameObject terminal = new GameObject("ManagementTerminal");
            terminal.transform.SetParent(g, false);
            terminal.transform.localPosition = new Vector3(5.25f, 0f, 3.62f);
            terminal.AddComponent<ManagementTerminal>();
            Cube(terminal.transform, "TerminalBody", new Vector3(0f, 0.95f, 0f), new Vector3(1.8f, 1.65f, 0.55f), frame);
            Cube(terminal.transform, "TerminalScreen", new Vector3(0f, 1.12f, -0.295f), new Vector3(1.35f, 0.72f, 0.035f), screen);
            Cube(terminal.transform, "TerminalControls", new Vector3(0f, 0.43f, -0.30f), new Vector3(1.35f, 0.16f, 0.40f), amber);
        }

        private static void BuildGenerator(Transform car, Material generatorMat, Material frame, Material steel, Material screen, Material amber)
        {
            Transform g = Group(car, "Generator_Interior");

            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            main.name = "MainGenerator";
            main.transform.SetParent(g, false);
            main.transform.localPosition = new Vector3(0.35f, 1.30f, 2.65f);
            main.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            main.transform.localScale = new Vector3(1.15f, 4.15f, 1.15f);
            main.GetComponent<Renderer>().sharedMaterial = generatorMat;

            Cube(g, "GeneratorBase", new Vector3(0.35f, 0.32f, 2.65f), new Vector3(8.8f, 0.40f, 2.30f), frame);
            Cube(g, "SafetyRail", new Vector3(0.35f, 0.90f, 1.25f), new Vector3(8.8f, 0.10f, 0.10f), steel);

            for (int i = -2; i <= 2; i++)
                Cabinet(g, new Vector3(-4.6f + i * 2.25f, 1.08f, -3.72f), new Vector3(1.72f, 1.95f, 0.62f), frame, steel);

            Cube(g, "GeneratorConsole", new Vector3(-6.20f, 0.90f, -2.65f), new Vector3(1.60f, 1.55f, 0.70f), frame);
            Cube(g, "GeneratorScreen", new Vector3(-6.20f, 1.20f, -2.28f), new Vector3(1.10f, 0.50f, 0.04f), screen);

            Pipe(g, "OverheadPipeA", new Vector3(0f, 3.32f, 3.25f), new Vector3(0.07f, 6.4f, 0.07f), new Vector3(0f, 0f, 90f), steel);
            Pipe(g, "OverheadPipeB", new Vector3(0f, 3.06f, 3.25f), new Vector3(0.07f, 6.4f, 0.07f), new Vector3(0f, 0f, 90f), amber);
            Pipe(g, "OverheadPipeC", new Vector3(0f, 2.80f, 3.25f), new Vector3(0.07f, 6.4f, 0.07f), new Vector3(0f, 0f, 90f), steel);
        }

        private static void BuildLiving(Transform car, Material living, Material mattress, Material frame, Material steel, Material amber)
        {
            Transform g = Group(car, "Living_Interior");

            Bunk(g, new Vector3(-6.25f, 0.48f, -2.85f), mattress, frame);
            Bunk(g, new Vector3(-6.25f, 0.48f, 2.85f), mattress, frame);
            Bunk(g, new Vector3(6.05f, 0.48f, -2.85f), mattress, frame);
            Bunk(g, new Vector3(6.05f, 0.48f, 2.85f), mattress, frame);

            Bench(g, new Vector3(-2.65f, 0.46f, -3.35f), living, frame, false);
            Bench(g, new Vector3(0f, 0.46f, -3.35f), living, frame, false);
            Bench(g, new Vector3(2.65f, 0.46f, -3.35f), living, frame, false);
            Bench(g, new Vector3(-2.65f, 0.46f, 3.35f), living, frame, true);
            Bench(g, new Vector3(0f, 0.46f, 3.35f), living, frame, true);
            Bench(g, new Vector3(2.65f, 0.46f, 3.35f), living, frame, true);

            Cube(g, "DiningTable", new Vector3(0f, 0.76f, 0f), new Vector3(2.80f, 0.12f, 1.45f), steel);
            Cube(g, "GalleyCounter", new Vector3(5.70f, 0.82f, -1.45f), new Vector3(3.15f, 1.42f, 0.92f), living);
            Cube(g, "GalleyTop", new Vector3(5.70f, 1.55f, -1.45f), new Vector3(3.22f, 0.10f, 0.98f), steel);
            Cabinet(g, new Vector3(5.85f, 1.12f, 1.72f), new Vector3(1.55f, 2.00f, 0.65f), frame, steel);
            Cube(g, "WallHeater", new Vector3(-5.70f, 0.84f, 0f), new Vector3(1.65f, 1.20f, 0.42f), amber);
        }

        private static void BuildWorkshop(Transform car, Material workshop, Material frame, Material steel, Material screen)
        {
            Transform g = Group(car, "Workshop_Interior");

            Workbench(g, new Vector3(-3.55f, 0.80f, -3.45f), 7.2f, workshop, frame, steel, false);
            Workbench(g, new Vector3(4.45f, 0.80f, -3.45f), 6.2f, workshop, frame, steel, false);
            Workbench(g, new Vector3(-3.00f, 0.80f, 3.45f), 8.0f, workshop, frame, steel, true);

            Cabinet(g, new Vector3(5.95f, 1.25f, 3.62f), new Vector3(2.50f, 2.35f, 0.72f), frame, steel);
            Cube(g, "MachineStation", new Vector3(5.15f, 0.86f, 1.80f), new Vector3(2.20f, 1.55f, 1.45f), frame);
            Cube(g, "MachineScreen", new Vector3(5.15f, 1.28f, 1.04f), new Vector3(1.25f, 0.48f, 0.04f), screen);
            Cube(g, "CentralAssemblyTable", new Vector3(0.65f, 0.72f, 0f), new Vector3(3.8f, 1.15f, 1.55f), steel);
            Cube(g, "OverheadToolRail", new Vector3(0f, 3.12f, -3.25f), new Vector3(12.5f, 0.12f, 0.20f), steel);
        }

        private static void BuildGreenhouse(Transform car, Material green, Material plant, Material frame, Material steel, Material amber, Material screen)
        {
            Transform g = Group(car, "Greenhouse_Interior");

            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * 2.75f;
                for (int i = -2; i <= 2; i++)
                {
                    float x = i * 2.55f - 0.65f;
                    Cube(g, "HydroponicBed", new Vector3(x, 0.62f, z), new Vector3(2.00f, 0.82f, 1.30f), frame);
                    Cube(g, "PlantBed", new Vector3(x, 1.08f, z), new Vector3(1.76f, 0.28f, 1.08f), green);
                    for (int leaf = -2; leaf <= 2; leaf++)
                        Cube(g, "Plant", new Vector3(x + leaf * 0.30f, 1.34f, z), new Vector3(0.18f, 0.42f, 0.38f), plant);
                    Cube(g, "GrowLight", new Vector3(x, 2.88f, z), new Vector3(1.85f, 0.08f, 0.20f), amber);
                }
            }

            Cube(g, "WaterTank", new Vector3(6.50f, 1.15f, -1.85f), new Vector3(1.55f, 2.05f, 2.05f), steel);
            Cube(g, "PumpUnit", new Vector3(6.35f, 0.72f, 0.65f), new Vector3(1.20f, 1.15f, 1.15f), frame);
            Cube(g, "GreenhouseControl", new Vector3(6.45f, 1.05f, 2.65f), new Vector3(1.30f, 1.70f, 0.55f), frame);
            Cube(g, "GreenhouseScreen", new Vector3(6.45f, 1.28f, 2.35f), new Vector3(0.90f, 0.52f, 0.04f), screen);

            Pipe(g, "IrrigationLeft", new Vector3(-0.45f, 2.58f, -3.35f), new Vector3(0.05f, 6.8f, 0.05f), new Vector3(0f, 0f, 90f), steel);
            Pipe(g, "IrrigationRight", new Vector3(-0.45f, 2.58f, 3.35f), new Vector3(0.05f, 6.8f, 0.05f), new Vector3(0f, 0f, 90f), steel);
        }

        private static Material Mat(string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/" + name + ".mat");
            if (material == null)
                Debug.LogWarning("FrozenTrain: material missing: " + name);
            return material;
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = localScale;
            if (material != null) obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj;
        }

        private static void Cabinet(Transform parent, Vector3 position, Vector3 size, Material body, Material trim)
        {
            Cube(parent, "CabinetBody", position, size, body);
            Cube(parent, "CabinetDoor", position + new Vector3(0f, 0f, -size.z * 0.51f), new Vector3(size.x * 0.88f, size.y * 0.86f, 0.04f), trim);
        }

        private static void Seat(Transform parent, Vector3 position, Material frame, Material cushion)
        {
            Cube(parent, "SeatBase", position, new Vector3(0.72f, 0.62f, 0.72f), frame);
            Cube(parent, "SeatCushion", position + new Vector3(0f, 0.36f, 0f), new Vector3(0.66f, 0.14f, 0.66f), cushion);
            Cube(parent, "SeatBack", position + new Vector3(0.32f, 0.78f, 0f), new Vector3(0.14f, 0.82f, 0.66f), cushion);
        }

        private static void Bunk(Transform parent, Vector3 position, Material mattress, Material frame)
        {
            Cube(parent, "LowerBunkFrame", position, new Vector3(2.55f, 0.16f, 1.05f), frame);
            Cube(parent, "LowerMattress", position + new Vector3(0f, 0.14f, 0f), new Vector3(2.38f, 0.15f, 0.90f), mattress);
            Cube(parent, "UpperBunkFrame", position + new Vector3(0f, 1.18f, 0f), new Vector3(2.55f, 0.16f, 1.05f), frame);
            Cube(parent, "UpperMattress", position + new Vector3(0f, 1.32f, 0f), new Vector3(2.38f, 0.15f, 0.90f), mattress);
            Cube(parent, "BunkPostA", position + new Vector3(-1.16f, 0.66f, 0f), new Vector3(0.08f, 1.45f, 0.08f), frame);
            Cube(parent, "BunkPostB", position + new Vector3(1.16f, 0.66f, 0f), new Vector3(0.08f, 1.45f, 0.08f), frame);
        }

        private static void Bench(Transform parent, Vector3 position, Material cushion, Material frame, bool faceNegativeZ)
        {
            Transform root = Group(parent, "WindowBench");
            root.localPosition = position;
            root.localRotation = faceNegativeZ ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            Cube(root, "BenchBase", Vector3.zero, new Vector3(2.20f, 0.58f, 0.68f), frame);
            Cube(root, "BenchSeat", new Vector3(0f, 0.36f, 0f), new Vector3(2.05f, 0.16f, 0.64f), cushion);
            Cube(root, "BenchBack", new Vector3(0f, 0.66f, 0.27f), new Vector3(2.05f, 0.42f, 0.12f), cushion);
        }

        private static void Workbench(Transform parent, Vector3 position, float length, Material accent, Material frame, Material steel, bool faceNegativeZ)
        {
            Transform root = Group(parent, "Workbench");
            root.localPosition = position;
            root.localRotation = faceNegativeZ ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            Cube(root, "WorkbenchBase", Vector3.zero, new Vector3(length, 1.35f, 0.92f), frame);
            Cube(root, "WorkbenchTop", new Vector3(0f, 0.72f, 0f), new Vector3(length + 0.08f, 0.12f, 1.00f), steel);
            Cube(root, "BackBoard", new Vector3(0f, 1.38f, -0.43f), new Vector3(length, 1.05f, 0.10f), accent);
        }

        private static void Pipe(Transform parent, string name, Vector3 position, Vector3 scale, Vector3 rotation, Material material)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = name;
            pipe.transform.SetParent(parent, false);
            pipe.transform.localPosition = position;
            pipe.transform.localEulerAngles = rotation;
            pipe.transform.localScale = scale;
            if (material != null) pipe.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
