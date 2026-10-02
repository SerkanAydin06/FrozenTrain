#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainInteriorDetailSetup
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";
        private const string DetailRootName = "INTERIOR_DETAIL_PASS";

        [MenuItem("Frozen Train/Add Interior Detail Pass")]
        public static void AddInteriorDetailPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");

            if (trainRoot == null)
            {
                Debug.LogError("FrozenTrain: FPS prototype was not found. Rebuild the FPS prototype first.");
                return;
            }

            Transform existing = trainRoot.transform.Find(DetailRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject detailRoot = new GameObject(DetailRootName);
            detailRoot.transform.SetParent(trainRoot.transform);
            detailRoot.transform.localPosition = Vector3.zero;

            Material darkPanel = GetOrCreateMaterial(
                "FPS_InteriorPanel",
                new Color(0.075f, 0.09f, 0.105f),
                0.38f,
                0.55f);

            Material windowGlass = GetOrCreateMaterial(
                "FPS_WindowGlass",
                new Color(0.055f, 0.16f, 0.22f),
                0.72f,
                0.15f,
                new Color(0.04f, 0.24f, 0.34f) * 1.6f);

            Material amber = GetOrCreateMaterial(
                "FPS_AccentAmber",
                new Color(0.52f, 0.20f, 0.045f),
                0.42f,
                0.3f,
                new Color(0.75f, 0.20f, 0.03f) * 1.4f);

            Material hazard = GetOrCreateMaterial(
                "FPS_HazardYellow",
                new Color(0.66f, 0.48f, 0.07f),
                0.26f,
                0.15f);

            Material fabric = GetOrCreateMaterial(
                "FPS_Fabric",
                new Color(0.20f, 0.25f, 0.27f),
                0.08f,
                0f);

            CreateWindowPanels(detailRoot.transform, windowGlass, darkPanel);
            CreateCeilingLightStrips(detailRoot.transform, amber);
            CreateLocomotiveDetails(detailRoot.transform, darkPanel, windowGlass, amber, fabric);
            CreateGeneratorDetails(detailRoot.transform, darkPanel, hazard, amber);
            CreateLivingDetails(detailRoot.transform, darkPanel, fabric, amber);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("FrozenTrain: interior detail pass added to locomotive, generator and living cars.");
        }

        private static void CreateWindowPanels(Transform parent, Material glass, Material frame)
        {
            float[] xPositions = { -15.2f, -11.9f, -8.7f, -3.1f, 0f, 3.1f, 9.0f, 12.4f, 15.8f };

            foreach (float x in xPositions)
            {
                CreateWindow(parent, new Vector3(x, 2.0f, -2.39f), glass, frame);
                CreateWindow(parent, new Vector3(x, 2.0f, 2.39f), glass, frame);
            }
        }

        private static void CreateWindow(Transform parent, Vector3 position, Material glass, Material frame)
        {
            CreateCube(parent, "WindowFrame", position, new Vector3(2.05f, 1.10f, 0.055f), frame);
            CreateCube(parent, "WindowGlass", position + new Vector3(0f, 0f, position.z > 0f ? -0.035f : 0.035f), new Vector3(1.72f, 0.80f, 0.03f), glass);
        }

        private static void CreateCeilingLightStrips(Transform parent, Material amber)
        {
            float[] xPositions = { -15.4f, -11.9f, -8.4f, -3.5f, 0f, 3.5f, 8.5f, 12.4f, 16.0f };

            foreach (float x in xPositions)
            {
                CreateCube(parent, "CeilingLightStrip", new Vector3(x, 3.245f, 0f), new Vector3(1.9f, 0.055f, 0.20f), amber);
            }
        }

        private static void CreateLocomotiveDetails(
            Transform parent,
            Material darkPanel,
            Material screen,
            Material accent,
            Material fabric)
        {
            GameObject root = new GameObject("Locomotive_Details");
            root.transform.SetParent(parent);

            CreateCube(root.transform, "DriverConsoleBody", new Vector3(-16.85f, 0.72f, 0f), new Vector3(0.78f, 1.15f, 3.35f), darkPanel);

            GameObject top = CreateCube(root.transform, "DriverConsoleTop", new Vector3(-16.40f, 1.22f, 0f), new Vector3(0.95f, 0.18f, 3.10f), darkPanel);
            top.transform.rotation = Quaternion.Euler(0f, 0f, -18f);

            float[] displayZ = { -0.95f, 0f, 0.95f };
            foreach (float z in displayZ)
            {
                CreateCube(root.transform, "DriverDisplay", new Vector3(-15.99f, 1.42f, z), new Vector3(0.045f, 0.48f, 0.66f), screen);
                CreateCube(root.transform, "StatusLamp", new Vector3(-15.955f, 1.10f, z), new Vector3(0.035f, 0.10f, 0.22f), accent);
            }

            CreateCube(root.transform, "DriverSeatBase", new Vector3(-14.85f, 0.43f, 0.10f), new Vector3(0.85f, 0.72f, 0.90f), darkPanel);
            CreateCube(root.transform, "DriverSeatCushion", new Vector3(-14.78f, 0.83f, 0.10f), new Vector3(0.72f, 0.16f, 0.78f), fabric);
            CreateCube(root.transform, "DriverSeatBack", new Vector3(-14.38f, 1.20f, 0.10f), new Vector3(0.16f, 0.90f, 0.78f), fabric);

            CreateCube(root.transform, "LeftEquipmentRack", new Vector3(-10.0f, 1.25f, -2.05f), new Vector3(2.2f, 2.3f, 0.52f), darkPanel);
            CreateCube(root.transform, "RightEquipmentRack", new Vector3(-10.0f, 1.25f, 2.05f), new Vector3(2.2f, 2.3f, 0.52f), darkPanel);

            for (int i = 0; i < 4; i++)
            {
                float x = -10.75f + i * 0.48f;
                CreateCube(root.transform, "RackIndicator", new Vector3(x, 1.55f, -1.76f), new Vector3(0.22f, 0.12f, 0.035f), accent);
            }
        }

        private static void CreateGeneratorDetails(Transform parent, Material darkPanel, Material hazard, Material accent)
        {
            GameObject root = new GameObject("Generator_Details");
            root.transform.SetParent(parent);

            CreateCube(root.transform, "GeneratorServiceDeck", new Vector3(0f, 0.16f, 1.25f), new Vector3(5.7f, 0.12f, 2.0f), darkPanel);

            CreateCylinder(root.transform, "AuxTank_Left", new Vector3(-3.85f, 1.10f, 1.55f), new Vector3(0.62f, 1.05f, 0.62f), darkPanel);
            CreateCylinder(root.transform, "AuxTank_Right", new Vector3(3.85f, 1.10f, 1.55f), new Vector3(0.62f, 1.05f, 0.62f), darkPanel);

            float[] pipeX = { -3.1f, 0f, 3.1f };
            foreach (float x in pipeX)
            {
                CreateCylinder(root.transform, "VerticalPipe", new Vector3(x, 1.55f, -2.05f), new Vector3(0.14f, 1.45f, 0.14f), accent);
            }

            CreateCube(root.transform, "OverheadPipe", new Vector3(0f, 2.88f, -2.05f), new Vector3(7.1f, 0.20f, 0.20f), accent);

            for (int i = -2; i <= 2; i++)
            {
                CreateCube(root.transform, "HazardMarker", new Vector3(i * 1.05f, 0.235f, 0.18f), new Vector3(0.58f, 0.025f, 0.22f), hazard);
            }

            CreateCube(root.transform, "ServiceConsole", new Vector3(-4.35f, 1.0f, -1.75f), new Vector3(1.3f, 1.7f, 0.48f), darkPanel);
            CreateCube(root.transform, "ServiceConsoleScreen", new Vector3(-4.35f, 1.35f, -1.49f), new Vector3(0.82f, 0.52f, 0.035f), accent);
        }

        private static void CreateLivingDetails(Transform parent, Material darkPanel, Material fabric, Material accent)
        {
            GameObject root = new GameObject("Living_Details");
            root.transform.SetParent(parent);

            CreateBunk(root.transform, new Vector3(9.5f, 0.55f, 1.72f), fabric, darkPanel);
            CreateBunk(root.transform, new Vector3(13.2f, 0.55f, 1.72f), fabric, darkPanel);

            CreateCube(root.transform, "CommonTable", new Vector3(15.6f, 0.72f, 0f), new Vector3(1.65f, 0.12f, 1.35f), darkPanel);
            CreateCube(root.transform, "TableLegA", new Vector3(15.1f, 0.36f, 0f), new Vector3(0.14f, 0.72f, 0.80f), darkPanel);
            CreateCube(root.transform, "TableLegB", new Vector3(16.1f, 0.36f, 0f), new Vector3(0.14f, 0.72f, 0.80f), darkPanel);

            for (int i = 0; i < 3; i++)
            {
                CreateCube(root.transform, "Locker", new Vector3(16.6f + i * 0.48f, 1.05f, 1.98f), new Vector3(0.40f, 1.95f, 0.55f), darkPanel);
                CreateCube(root.transform, "LockerHandle", new Vector3(16.6f + i * 0.48f, 1.08f, 1.68f), new Vector3(0.05f, 0.18f, 0.04f), accent);
            }

            CreateCube(root.transform, "WallHeater", new Vector3(8.0f, 0.75f, 2.08f), new Vector3(1.15f, 1.10f, 0.38f), darkPanel);
            for (int i = -2; i <= 2; i++)
            {
                CreateCube(root.transform, "HeaterGlow", new Vector3(8.0f + i * 0.18f, 0.76f, 1.87f), new Vector3(0.08f, 0.64f, 0.025f), accent);
            }
        }

        private static void CreateBunk(Transform parent, Vector3 position, Material mattress, Material frame)
        {
            CreateCube(parent, "LowerBunkFrame", position, new Vector3(2.25f, 0.16f, 0.82f), frame);
            CreateCube(parent, "LowerMattress", position + new Vector3(0f, 0.14f, 0f), new Vector3(2.08f, 0.15f, 0.70f), mattress);
            CreateCube(parent, "UpperBunkFrame", position + new Vector3(0f, 1.12f, 0f), new Vector3(2.25f, 0.16f, 0.82f), frame);
            CreateCube(parent, "UpperMattress", position + new Vector3(0f, 1.26f, 0f), new Vector3(2.08f, 0.15f, 0.70f), mattress);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = position;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent);
            cylinder.transform.localPosition = position;
            cylinder.transform.localRotation = Quaternion.identity;
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            return cylinder;
        }

        private static Material GetOrCreateMaterial(
            string fileName,
            Color color,
            float smoothness,
            float metallic,
            Color? emission = null)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
            {
                Debug.LogError("FrozenTrain: URP/Lit shader could not be found.");
                return material;
            }

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = fileName
                };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.color = color;

            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);

            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }

            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif
