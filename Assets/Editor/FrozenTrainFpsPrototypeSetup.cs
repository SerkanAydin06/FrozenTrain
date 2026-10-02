#if UNITY_EDITOR
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainFpsPrototypeSetup
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";
        private const float CarLength = 12f;
        private const float CarWidth = 5f;
        private const float CarHeight = 3.4f;
        private const float WallThickness = 0.18f;

        [MenuItem("Frozen Train/Create FPS Train Prototype")]
        public static void CreateFpsTrainPrototype()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floorMaterial = GetOrCreateMaterial("FPS_Floor", new Color(0.10f, 0.12f, 0.13f), 0.25f);
            Material wallMaterial = GetOrCreateMaterial("FPS_Wall", new Color(0.16f, 0.19f, 0.21f), 0.30f);
            Material frameMaterial = GetOrCreateMaterial("FPS_Frame", new Color(0.07f, 0.09f, 0.10f), 0.40f);
            Material generatorMaterial = GetOrCreateMaterial("FPS_Generator", new Color(0.30f, 0.16f, 0.08f), 0.38f);
            Material livingMaterial = GetOrCreateMaterial("FPS_Living", new Color(0.14f, 0.22f, 0.24f), 0.24f);
            Material snowMaterial = GetOrCreateMaterial("FPS_Snow", new Color(0.75f, 0.82f, 0.88f), 0.12f);

            CreateWorldLighting();
            CreateOutsideEnvironment(snowMaterial);

            GameObject trainRoot = new GameObject("TRAIN_INTERIOR_PROTOTYPE");

            CreateHollowCar(trainRoot.transform, "01_Locomotive", -12.4f, floorMaterial, wallMaterial, frameMaterial);
            CreateHollowCar(trainRoot.transform, "02_Generator", 0f, floorMaterial, wallMaterial, frameMaterial);
            CreateHollowCar(trainRoot.transform, "03_Living", 12.4f, floorMaterial, wallMaterial, frameMaterial);

            CreateConnector(trainRoot.transform, -6.2f, floorMaterial, frameMaterial);
            CreateConnector(trainRoot.transform, 6.2f, floorMaterial, frameMaterial);

            CreateGeneratorProps(trainRoot.transform, generatorMaterial, frameMaterial);
            CreateLivingProps(trainRoot.transform, livingMaterial, frameMaterial);
            CreateCeilingLights(trainRoot.transform);
            CreatePlayer(new Vector3(12.4f, 0.12f, 0f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = trainRoot;
            Debug.Log("FPS prototype created. Open FPSPrototype, press Play, then use WASD, mouse, Shift and Space. Escape releases the cursor.");
        }

        private static void CreateWorldLighting()
        {
            GameObject sunObject = new GameObject("ColdExteriorLight");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.55f;
            sun.color = new Color(0.62f, 0.74f, 0.95f);
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(38f, -28f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.10f, 0.15f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.07f, 0.09f, 0.12f);
            RenderSettings.ambientGroundColor = new Color(0.025f, 0.03f, 0.04f);
        }

        private static void CreateOutsideEnvironment(Material snowMaterial)
        {
            GameObject environment = new GameObject("OUTSIDE_ENVIRONMENT");

            GameObject snow = GameObject.CreatePrimitive(PrimitiveType.Plane);
            snow.name = "SnowGround";
            snow.transform.SetParent(environment.transform);
            snow.transform.position = new Vector3(0f, -0.65f, 0f);
            snow.transform.localScale = new Vector3(15f, 1f, 8f);
            snow.GetComponent<Renderer>().sharedMaterial = snowMaterial;
        }

        private static void CreateHollowCar(
            Transform parent,
            string name,
            float centerX,
            Material floorMaterial,
            Material wallMaterial,
            Material frameMaterial)
        {
            GameObject car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(car.transform, "Floor", new Vector3(0f, 0f, 0f), new Vector3(CarLength, 0.2f, CarWidth), floorMaterial);
            CreateCube(car.transform, "Ceiling", new Vector3(0f, CarHeight, 0f), new Vector3(CarLength, 0.18f, CarWidth), wallMaterial);

            CreateCube(
                car.transform,
                "Wall_Left",
                new Vector3(0f, CarHeight * 0.5f, -CarWidth * 0.5f),
                new Vector3(CarLength, CarHeight, WallThickness),
                wallMaterial);

            CreateCube(
                car.transform,
                "Wall_Right",
                new Vector3(0f, CarHeight * 0.5f, CarWidth * 0.5f),
                new Vector3(CarLength, CarHeight, WallThickness),
                wallMaterial);

            CreateDoorBulkhead(car.transform, -CarLength * 0.5f, frameMaterial);
            CreateDoorBulkhead(car.transform, CarLength * 0.5f, frameMaterial);

            for (float x = -4.5f; x <= 4.5f; x += 3f)
            {
                CreateCube(
                    car.transform,
                    "CeilingRib",
                    new Vector3(x, CarHeight - 0.12f, 0f),
                    new Vector3(0.18f, 0.20f, CarWidth - 0.15f),
                    frameMaterial);
            }
        }

        private static void CreateDoorBulkhead(Transform parent, float localX, Material material)
        {
            const float doorWidth = 1.6f;
            const float doorHeight = 2.35f;

            float sideWidth = (CarWidth - doorWidth) * 0.5f;
            float sideCenter = doorWidth * 0.5f + sideWidth * 0.5f;

            CreateCube(
                parent,
                "BulkheadSide",
                new Vector3(localX, CarHeight * 0.5f, -sideCenter),
                new Vector3(WallThickness, CarHeight, sideWidth),
                material);

            CreateCube(
                parent,
                "BulkheadSide",
                new Vector3(localX, CarHeight * 0.5f, sideCenter),
                new Vector3(WallThickness, CarHeight, sideWidth),
                material);

            float lintelHeight = CarHeight - doorHeight;
            CreateCube(
                parent,
                "BulkheadLintel",
                new Vector3(localX, doorHeight + lintelHeight * 0.5f, 0f),
                new Vector3(WallThickness, lintelHeight, doorWidth),
                material);
        }

        private static void CreateConnector(Transform parent, float centerX, Material floorMaterial, Material frameMaterial)
        {
            GameObject connector = new GameObject("CarConnector");
            connector.transform.SetParent(parent);
            connector.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(connector.transform, "ConnectorFloor", Vector3.zero, new Vector3(0.55f, 0.2f, 1.8f), floorMaterial);
            CreateCube(connector.transform, "ConnectorCeiling", new Vector3(0f, 2.55f, 0f), new Vector3(0.55f, 0.15f, 1.8f), frameMaterial);
            CreateCube(connector.transform, "ConnectorLeft", new Vector3(0f, 1.3f, -0.9f), new Vector3(0.55f, 2.6f, 0.12f), frameMaterial);
            CreateCube(connector.transform, "ConnectorRight", new Vector3(0f, 1.3f, 0.9f), new Vector3(0.55f, 2.6f, 0.12f), frameMaterial);
        }

        private static void CreateGeneratorProps(Transform trainRoot, Material generatorMaterial, Material frameMaterial)
        {
            GameObject props = new GameObject("Generator_Props");
            props.transform.SetParent(trainRoot);

            GameObject generator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            generator.name = "MainGenerator";
            generator.transform.SetParent(props.transform);
            generator.transform.position = new Vector3(0f, 1.05f, 1.25f);
            generator.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            generator.transform.localScale = new Vector3(0.95f, 2.6f, 0.95f);
            generator.GetComponent<Renderer>().sharedMaterial = generatorMaterial;

            CreateCube(props.transform, "ControlConsole", new Vector3(-2.7f, 0.75f, -1.55f), new Vector3(2.2f, 1.35f, 0.65f), frameMaterial);
            CreateCube(props.transform, "PipeRun", new Vector3(2.7f, 2.65f, 1.85f), new Vector3(3.4f, 0.22f, 0.22f), generatorMaterial);
        }

        private static void CreateLivingProps(Transform trainRoot, Material livingMaterial, Material frameMaterial)
        {
            GameObject props = new GameObject("Living_Props");
            props.transform.SetParent(trainRoot);

            CreateBunk(props.transform, new Vector3(9.5f, 0.55f, -1.75f), livingMaterial, frameMaterial);
            CreateBunk(props.transform, new Vector3(14.3f, 0.55f, -1.75f), livingMaterial, frameMaterial);
            CreateCube(props.transform, "StorageCrate", new Vector3(15.6f, 0.55f, 1.65f), new Vector3(1.2f, 1.0f, 1.2f), frameMaterial);
        }

        private static void CreateBunk(Transform parent, Vector3 position, Material mattressMaterial, Material frameMaterial)
        {
            CreateCube(parent, "BunkFrame", position, new Vector3(2.3f, 0.18f, 0.85f), frameMaterial);
            CreateCube(parent, "Mattress", position + new Vector3(0f, 0.14f, 0f), new Vector3(2.15f, 0.16f, 0.76f), mattressMaterial);
            CreateCube(parent, "UpperBunkFrame", position + new Vector3(0f, 1.15f, 0f), new Vector3(2.3f, 0.18f, 0.85f), frameMaterial);
            CreateCube(parent, "UpperMattress", position + new Vector3(0f, 1.29f, 0f), new Vector3(2.15f, 0.16f, 0.76f), mattressMaterial);
        }

        private static void CreateCeilingLights(Transform trainRoot)
        {
            float[] xPositions = { -12.4f, -8.5f, -3.5f, 0f, 3.5f, 8.5f, 12.4f, 16f };

            foreach (float x in xPositions)
            {
                GameObject lightObject = new GameObject("InteriorLight");
                lightObject.transform.SetParent(trainRoot);
                lightObject.transform.position = new Vector3(x, 3.0f, 0f);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 7f;
                light.intensity = 2.0f;
                light.color = new Color(1f, 0.73f, 0.46f);
                light.shadows = LightShadows.None;
            }
        }

        private static void CreatePlayer(Vector3 position)
        {
            GameObject player = new GameObject("FPS_Player");
            player.transform.position = position;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.34f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.32f;
            controller.slopeLimit = 50f;

            GameObject cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.tag = "MainCamera";

            Camera playerCamera = cameraObject.AddComponent<Camera>();
            playerCamera.fieldOfView = 72f;
            playerCamera.nearClipPlane = 0.04f;
            playerCamera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();

            player.AddComponent<FpsPlayerController>();
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material GetOrCreateMaterial(string fileName, Color color, float smoothness)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = fileName,
                color = color
            };

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath);
            if (!string.IsNullOrEmpty(parent))
            {
                parent = parent.Replace('\\', '/');
            }

            string name = System.IO.Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
