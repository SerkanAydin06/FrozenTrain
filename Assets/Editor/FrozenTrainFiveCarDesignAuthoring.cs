#if UNITY_EDITOR
using FrozenTrain.Management;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Internal main-deck authoring helper. The public project entry point is
    /// FrozenTrainUnifiedAuthoring.RebuildAuthoredTrain().
    /// </summary>
    public static class FrozenTrainFiveCarDesignAuthoring
    {
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";

        public static void ApplyAll()
        {
            Material frame = Mat("Train_Frame");
            Material steel = Mat("Train_Steel");
            Material rubber = Mat("Train_Rubber");
            Material generator = Mat("Train_Generator");
            Material living = Mat("Train_Living");
            Material workshop = Mat("Train_Workshop");
            Material greenhouse = Mat("Train_Greenhouse");
            Material plant = Mat("Train_Plant");
            Material screen = Mat("Train_Screen");
            Material amber = Mat("Train_Amber");

            ConfigurePrefab("01_Locomotive", root => BuildLocomotive(root.transform, frame, steel, screen, amber, rubber));
            ConfigurePrefab("02_Generator", root => BuildGenerator(root.transform, generator, frame, steel, screen, amber));
            ConfigurePrefab("03_Living", root => BuildLivingBase(root.transform, living, frame, steel, amber));
            ConfigurePrefab("04_Workshop", root => BuildWorkshop(root.transform, workshop, frame, steel, screen));
            ConfigurePrefab("05_Greenhouse", root => BuildGreenhouse(root.transform, greenhouse, plant, frame, steel, amber, screen));
        }

        private static void ConfigurePrefab(string prefabName, System.Action<GameObject> build)
        {
            string path = TrainPrefabFolder + "/" + prefabName + ".prefab";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                build?.Invoke(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildLocomotive(Transform root, Material frame, Material steel, Material screen, Material amber, Material rubber)
        {
            ReplaceGroup(root, "Locomotive_Interior", out Transform g);

            Cube(g, "MainConsole", new Vector3(-6.35f, 0.82f, 0f), new Vector3(1.15f, 1.42f, 5.30f), frame);
            for (int i = -2; i <= 2; i++)
                Cube(g, "MainDisplay", new Vector3(-5.74f, 1.38f, i * 0.95f), new Vector3(0.045f, 0.52f, 0.70f), screen);

            Seat(g, new Vector3(-4.35f, 0.48f, -1.20f), frame, rubber);
            Seat(g, new Vector3(-4.35f, 0.48f, 1.20f), frame, rubber);

            Cabinet(g, new Vector3(1.15f, 1.25f, -3.72f), new Vector3(3.0f, 2.30f, 0.62f), frame, steel, true);
            Cabinet(g, new Vector3(4.65f, 1.25f, -3.72f), new Vector3(2.8f, 2.30f, 0.62f), frame, steel, true);
            Cabinet(g, new Vector3(2.25f, 1.25f, 3.72f), new Vector3(2.4f, 2.30f, 0.62f), frame, steel, false);

            GameObject terminal = new GameObject("ManagementTerminal");
            terminal.transform.SetParent(g, false);
            terminal.transform.localPosition = new Vector3(5.25f, 0f, 3.62f);
            terminal.AddComponent<ManagementTerminal>();
            Cube(terminal.transform, "TerminalBody", new Vector3(0f, 0.95f, 0f), new Vector3(1.8f, 1.65f, 0.55f), frame);
            Cube(terminal.transform, "TerminalScreen", new Vector3(0f, 1.12f, -0.295f), new Vector3(1.35f, 0.72f, 0.035f), screen);
            Cube(terminal.transform, "TerminalControls", new Vector3(0f, 0.43f, -0.30f), new Vector3(1.35f, 0.16f, 0.40f), amber);
        }

        private static void BuildGenerator(Transform root, Material generatorMat, Material frame, Material steel, Material screen, Material amber)
        {
            ReplaceGroup(root, "Generator_Interior", out Transform g);

            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            main.name = "MainGenerator";
            main.transform.SetParent(g, false);
            main.transform.localPosition = new Vector3(0.35f, 1.30f, 2.65f);
            main.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            main.transform.localScale = new Vector3(1.15f, 4.15f, 1.15f);
            main.GetComponent<Renderer>().sharedMaterial = generatorMat;

            Cube(g, "GeneratorBase", new Vector3(0.35f, 0.32f, 2.65f), new Vector3(8.8f, 0.40f, 2.30f), frame);
            Cube(g, "ServiceRail", new Vector3(0.35f, 0.90f, 1.25f), new Vector3(8.8f, 0.10f, 0.10f), steel);

            for (int i = -2; i <= 2; i++)
                Cabinet(g, new Vector3(-4.6f + i * 2.25f, 1.08f, -3.72f), new Vector3(1.72f, 1.95f, 0.62f), frame, steel, true);

            Cube(g, "GeneratorConsole", new Vector3(-6.20f, 0.90f, -2.65f), new Vector3(1.60f, 1.55f, 0.70f), frame);
            Cube(g, "GeneratorScreen", new Vector3(-6.20f, 1.20f, -2.28f), new Vector3(1.10f, 0.50f, 0.04f), screen);

            Pipe(g, "OverheadPipeA", new Vector3(0f, 2.78f, 3.25f), new Vector3(0.07f, 6.4f, 0.07f), new Vector3(0f, 0f, 90f), steel);
            Pipe(g, "OverheadPipeB", new Vector3(0f, 2.55f, 3.25f), new Vector3(0.07f, 6.4f, 0.07f), new Vector3(0f, 0f, 90f), amber);
        }

        private static void BuildLivingBase(Transform root, Material living, Material frame, Material steel, Material amber)
        {
            ReplaceGroup(root, "Living_Interior", out Transform g);
            Cube(g, "GalleyCounter", new Vector3(6.25f, 0.80f, -2.85f), new Vector3(2.45f, 1.38f, 0.92f), living);
            Cube(g, "GalleyTop", new Vector3(6.25f, 1.52f, -2.85f), new Vector3(2.52f, 0.10f, 0.98f), steel);
            Cube(g, "WallHeater", new Vector3(-6.55f, 0.82f, -3.70f), new Vector3(1.45f, 1.12f, 0.34f), amber);
        }

        private static void BuildWorkshop(Transform root, Material workshop, Material frame, Material steel, Material screen)
        {
            ReplaceGroup(root, "Workshop_Interior", out Transform g);

            Workbench(g, new Vector3(-3.95f, 0.80f, -3.25f), 5.1f, workshop, frame, steel, false);
            Workbench(g, new Vector3(2.20f, 0.80f, -3.25f), 5.7f, workshop, frame, steel, false);
            Workbench(g, new Vector3(-2.20f, 0.80f, 3.25f), 6.0f, workshop, frame, steel, true);

            Cabinet(g, new Vector3(5.95f, 1.18f, 3.35f), new Vector3(2.1f, 2.20f, 0.68f), frame, steel, false);
            Cube(g, "MachineStation", new Vector3(5.05f, 0.86f, 1.55f), new Vector3(2.0f, 1.55f, 1.35f), frame);
            Cube(g, "MachineScreen", new Vector3(5.05f, 1.28f, 0.85f), new Vector3(1.15f, 0.48f, 0.04f), screen);
        }

        private static void BuildGreenhouse(Transform root, Material green, Material plant, Material frame, Material steel, Material amber, Material screen)
        {
            ReplaceGroup(root, "Greenhouse_Interior", out Transform g);

            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * 2.55f;
                for (int i = -2; i <= 2; i++)
                {
                    float x = i * 2.50f;
                    Cube(g, "HydroponicBed", new Vector3(x, 0.60f, z), new Vector3(1.90f, 0.80f, 1.28f), frame);
                    Cube(g, "PlantBed", new Vector3(x, 1.04f, z), new Vector3(1.68f, 0.25f, 1.08f), green);
                    for (int leaf = -2; leaf <= 2; leaf++)
                        Cube(g, "Plant", new Vector3(x + leaf * 0.28f, 1.30f, z), new Vector3(0.18f, 0.38f, 0.34f), plant);
                    Cube(g, "GrowLight", new Vector3(x, 2.60f, z), new Vector3(1.75f, 0.07f, 0.20f), amber);
                }
            }

            Cube(g, "WaterTank", new Vector3(6.20f, 1.08f, 2.75f), new Vector3(1.45f, 1.90f, 1.65f), steel);
            Cube(g, "PumpUnit", new Vector3(5.90f, 0.68f, -2.80f), new Vector3(1.15f, 1.10f, 1.15f), frame);
            Cube(g, "ClimatePanel", new Vector3(6.55f, 1.20f, -3.62f), new Vector3(1.25f, 1.25f, 0.40f), frame);
            Cube(g, "ClimateScreen", new Vector3(6.55f, 1.35f, -3.40f), new Vector3(0.86f, 0.45f, 0.04f), screen);
        }

        private static void ReplaceGroup(Transform root, string name, out Transform group)
        {
            Transform old = FindDeep(root, name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            group = go.transform;
        }

        private static Material Mat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/" + name + ".mat");
        }

        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void Seat(Transform parent, Vector3 position, Material frame, Material cushion)
        {
            Cube(parent, "SeatBase", position, new Vector3(0.72f, 0.62f, 0.72f), frame);
            Cube(parent, "SeatCushion", position + new Vector3(0f, 0.36f, 0f), new Vector3(0.66f, 0.14f, 0.66f), cushion);
            Cube(parent, "SeatBack", position + new Vector3(0.32f, 0.78f, 0f), new Vector3(0.14f, 0.82f, 0.66f), cushion);
        }

        private static void Cabinet(Transform parent, Vector3 position, Vector3 size, Material body, Material trim, bool doorFacesPositiveZ)
        {
            Cube(parent, "CabinetBody", position, size, body);
            float doorZ = (doorFacesPositiveZ ? 1f : -1f) * size.z * 0.51f;
            Cube(parent, "CabinetDoor", position + new Vector3(0f, 0f, doorZ), new Vector3(size.x * 0.88f, size.y * 0.86f, 0.04f), trim);
        }

        private static void Workbench(Transform parent, Vector3 position, float length, Material accent, Material frame, Material steel, bool faceNegativeZ)
        {
            GameObject go = new GameObject("Workbench");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = faceNegativeZ ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            Cube(go.transform, "WorkbenchBase", Vector3.zero, new Vector3(length, 1.35f, 0.92f), frame);
            Cube(go.transform, "WorkbenchTop", new Vector3(0f, 0.72f, 0f), new Vector3(length + 0.08f, 0.12f, 1.00f), steel);
            Cube(go.transform, "BackBoard", new Vector3(0f, 1.38f, -0.43f), new Vector3(length, 1.05f, 0.10f), accent);
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