#if UNITY_EDITOR
using FrozenTrain.Atmosphere;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainPlayablePrototypeBuilder
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";
        private const float CarLength = 12f;
        private const float CarWidth = 5f;
        private const float CarHeight = 3.4f;
        private const float WallThickness = 0.18f;

        [MenuItem("Frozen Train/Build Playable Prototype")]
        public static void BuildPlayablePrototype()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = Material("FPS_Floor", new Color(0.10f, 0.12f, 0.13f), 0.25f, 0.15f);
            Material wall = Material("FPS_Wall", new Color(0.16f, 0.19f, 0.21f), 0.30f, 0.35f);
            Material frame = Material("FPS_Frame", new Color(0.055f, 0.07f, 0.08f), 0.42f, 0.65f);
            Material generator = Material("FPS_Generator", new Color(0.30f, 0.13f, 0.055f), 0.38f, 0.35f);
            Material living = Material("FPS_Living", new Color(0.14f, 0.22f, 0.24f), 0.20f, 0.08f);
            Material workshop = Material("FPS_Workshop", new Color(0.22f, 0.17f, 0.10f), 0.28f, 0.38f);
            Material greenhouse = Material("FPS_Greenhouse", new Color(0.10f, 0.28f, 0.16f), 0.18f, 0.05f);
            Material screen = Material("FPS_Screen", new Color(0.05f, 0.32f, 0.42f), 0.65f, 0.18f, new Color(0.05f, 0.75f, 0.95f) * 1.8f);
            Material amber = Material("FPS_Amber", new Color(0.48f, 0.20f, 0.04f), 0.38f, 0.15f, new Color(0.95f, 0.34f, 0.05f) * 1.5f);
            Material snow = Material("FPS_Snow", new Color(0.72f, 0.80f, 0.87f), 0.10f, 0f);
            Material darkRock = Material("FPS_ExteriorRock", new Color(0.10f, 0.12f, 0.14f), 0.08f, 0f);

            CreateLighting();
            CreateExterior(snow, darkRock, frame);

            GameObject trainRoot = new GameObject("TRAIN_INTERIOR_PROTOTYPE");
            TrainPrototypeState state = trainRoot.AddComponent<TrainPrototypeState>();
            trainRoot.AddComponent<AudioSource>();
            trainRoot.AddComponent<TrainAmbience>();

            float[] centers = { -12.4f, 0f, 12.4f, 24.8f, 37.2f };
            string[] names = { "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse" };
            for (int i = 0; i < centers.Length; i++)
            {
                CreateHollowCar(trainRoot.transform, names[i], centers[i], floor, wall, frame);
            }

            float[] connectorCenters = { -6.2f, 6.2f, 18.6f, 31.0f };
            foreach (float x in connectorCenters)
            {
                CreateConnectorWithDoor(trainRoot.transform, x, floor, frame);
            }

            CreateLocomotive(trainRoot.transform, frame, screen, amber);
            CreateGenerator(trainRoot.transform, generator, frame, amber);
            CreateLiving(trainRoot.transform, living, frame, amber);
            CreateWorkshop(trainRoot.transform, workshop, frame, amber);
            CreateGreenhouse(trainRoot.transform, greenhouse, frame, amber);
            CreateWindowsAndLights(trainRoot.transform, centers, frame, screen, amber);

            GameObject player = CreatePlayer(new Vector3(12.4f, 0.12f, 0f));
            FpsPlayerController controller = player.GetComponent<FpsPlayerController>();
            Camera camera = player.GetComponentInChildren<Camera>(true);
            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
            interaction.Configure(camera, 2.8f);

            TrainManagementPanel panel = player.AddComponent<TrainManagementPanel>();
            panel.Configure(state, controller);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;

            Debug.Log("FrozenTrain playable prototype built: 5 cars, FPS, 4 sliding doors, management panel, moving scenery and train ambience. Press Play.");
        }

        private static void CreateLighting()
        {
            GameObject sunObject = new GameObject("ColdExteriorLight");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.55f;
            sun.color = new Color(0.62f, 0.74f, 0.95f);
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(38f, -28f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.08f, 0.13f, 0.20f);
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.07f, 0.10f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.025f, 0.03f);
        }

        private static void CreateExterior(Material snow, Material rock, Material frame)
        {
            GameObject environment = new GameObject("OUTSIDE_ENVIRONMENT");
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SnowGround";
            ground.transform.SetParent(environment.transform);
            ground.transform.position = new Vector3(12f, -0.65f, 0f);
            ground.transform.localScale = new Vector3(12f, 1f, 7f);
            ground.GetComponent<Renderer>().sharedMaterial = snow;

            GameObject moving = new GameObject("MOVING_SCENERY");
            moving.transform.SetParent(environment.transform);
            moving.AddComponent<MovingScenery>();

            for (int i = 0; i < 14; i++)
            {
                float x = -30f + i * 7f;
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject rockObj = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                rockObj.name = "PassingRock";
                rockObj.transform.SetParent(moving.transform);
                rockObj.transform.localPosition = new Vector3(x, -0.05f, side * Random.Range(7.5f, 13f));
                rockObj.transform.localScale = new Vector3(Random.Range(1.2f, 3.5f), Random.Range(1f, 3f), Random.Range(1.2f, 3.5f));
                rockObj.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 180f), Random.Range(-8f, 8f));
                rockObj.GetComponent<Renderer>().sharedMaterial = rock;
            }

            for (int i = 0; i < 12; i++)
            {
                float x = -28f + i * 8f;
                float z = i % 2 == 0 ? -6.8f : 6.8f;
                CreateCube(moving.transform, "TrackSidePost", new Vector3(x, 1.2f, z), new Vector3(0.18f, 2.5f, 0.18f), frame);
            }
        }

        private static void CreateHollowCar(Transform parent, string name, float centerX, Material floor, Material wall, Material frame)
        {
            GameObject car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = new Vector3(centerX, 0f, 0f);
            CreateCube(car.transform, "Floor", Vector3.zero, new Vector3(CarLength, 0.20f, CarWidth), floor);
            CreateCube(car.transform, "Ceiling", new Vector3(0f, CarHeight, 0f), new Vector3(CarLength, 0.18f, CarWidth), wall);
            CreateCube(car.transform, "Wall_Left", new Vector3(0f, CarHeight * 0.5f, -CarWidth * 0.5f), new Vector3(CarLength, CarHeight, WallThickness), wall);
            CreateCube(car.transform, "Wall_Right", new Vector3(0f, CarHeight * 0.5f, CarWidth * 0.5f), new Vector3(CarLength, CarHeight, WallThickness), wall);
            CreateDoorBulkhead(car.transform, -CarLength * 0.5f, frame);
            CreateDoorBulkhead(car.transform, CarLength * 0.5f, frame);
            for (float x = -4.5f; x <= 4.5f; x += 3f)
                CreateCube(car.transform, "CeilingRib", new Vector3(x, CarHeight - 0.12f, 0f), new Vector3(0.18f, 0.20f, CarWidth - 0.15f), frame);
        }

        private static void CreateDoorBulkhead(Transform parent, float localX, Material material)
        {
            const float doorWidth = 1.6f;
            const float doorHeight = 2.35f;
            float sideWidth = (CarWidth - doorWidth) * 0.5f;
            float sideCenter = doorWidth * 0.5f + sideWidth * 0.5f;
            CreateCube(parent, "BulkheadSide", new Vector3(localX, CarHeight * 0.5f, -sideCenter), new Vector3(WallThickness, CarHeight, sideWidth), material);
            CreateCube(parent, "BulkheadSide", new Vector3(localX, CarHeight * 0.5f, sideCenter), new Vector3(WallThickness, CarHeight, sideWidth), material);
            float lintelHeight = CarHeight - doorHeight;
            CreateCube(parent, "BulkheadLintel", new Vector3(localX, doorHeight + lintelHeight * 0.5f, 0f), new Vector3(WallThickness, lintelHeight, doorWidth), material);
        }

        private static void CreateConnectorWithDoor(Transform parent, float centerX, Material floor, Material frame)
        {
            GameObject connector = new GameObject("CarConnector");
            connector.transform.SetParent(parent);
            connector.transform.position = new Vector3(centerX, 0f, 0f);
            CreateCube(connector.transform, "ConnectorFloor", Vector3.zero, new Vector3(0.55f, 0.20f, 1.8f), floor);
            CreateCube(connector.transform, "ConnectorCeiling", new Vector3(0f, 2.55f, 0f), new Vector3(0.55f, 0.15f, 1.8f), frame);
            CreateCube(connector.transform, "ConnectorLeft", new Vector3(0f, 1.3f, -0.9f), new Vector3(0.55f, 2.6f, 0.12f), frame);
            CreateCube(connector.transform, "ConnectorRight", new Vector3(0f, 1.3f, 0.9f), new Vector3(0.55f, 2.6f, 0.12f), frame);
            GameObject door = CreateCube(connector.transform, "SlidingDoor", new Vector3(0f, 1.18f, 0f), new Vector3(0.12f, 2.25f, 1.48f), frame);
            SlidingDoor sliding = door.AddComponent<SlidingDoor>();
            sliding.Configure(new Vector3(0f, 0f, -1.55f), 2.8f);
        }

        private static void CreateLocomotive(Transform root, Material frame, Material screen, Material amber)
        {
            CreateCube(root, "DriverConsole", new Vector3(-16.7f, 0.75f, 0f), new Vector3(0.85f, 1.30f, 3.3f), frame);
            for (int i = -1; i <= 1; i++)
                CreateCube(root, "DriverScreen", new Vector3(-16.22f, 1.35f, i * 0.95f), new Vector3(0.04f, 0.55f, 0.70f), screen);

            GameObject terminal = new GameObject("ManagementTerminal");
            terminal.transform.SetParent(root);
            terminal.transform.position = new Vector3(-14.7f, 0f, -1.95f);
            terminal.AddComponent<ManagementTerminal>();
            CreateCube(terminal.transform, "TerminalBody", new Vector3(0f, 0.78f, 0f), new Vector3(1.6f, 1.45f, 0.42f), frame);
            CreateCube(terminal.transform, "TerminalScreen", new Vector3(0f, 0.95f, 0.235f), new Vector3(1.12f, 0.58f, 0.035f), screen);
            CreateCube(terminal.transform, "TerminalKeys", new Vector3(0f, 0.36f, 0.22f), new Vector3(1.30f, 0.15f, 0.45f), amber);
        }

        private static void CreateGenerator(Transform root, Material generator, Material frame, Material amber)
        {
            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            main.name = "MainGenerator";
            main.transform.SetParent(root);
            main.transform.position = new Vector3(0f, 1.05f, 1.25f);
            main.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            main.transform.localScale = new Vector3(0.95f, 2.6f, 0.95f);
            main.GetComponent<Renderer>().sharedMaterial = generator;
            CreateCube(root, "GeneratorServiceDeck", new Vector3(0f, 0.17f, 1.25f), new Vector3(5.6f, 0.12f, 2.0f), frame);
            CreateCube(root, "GeneratorConsole", new Vector3(-3.6f, 0.85f, -1.85f), new Vector3(1.5f, 1.5f, 0.45f), frame);
            CreateCube(root, "GeneratorIndicator", new Vector3(-3.6f, 1.22f, -1.60f), new Vector3(0.9f, 0.45f, 0.04f), amber);
        }

        private static void CreateLiving(Transform root, Material living, Material frame, Material amber)
        {
            CreateBunk(root, new Vector3(9.5f, 0.55f, -1.72f), living, frame);
            CreateBunk(root, new Vector3(13.0f, 0.55f, -1.72f), living, frame);
            CreateBunk(root, new Vector3(9.5f, 0.55f, 1.72f), living, frame);
            CreateCube(root, "LivingTable", new Vector3(15.4f, 0.72f, 0f), new Vector3(1.8f, 0.12f, 1.35f), frame);
            CreateCube(root, "LivingHeater", new Vector3(8.0f, 0.8f, 2.05f), new Vector3(1.1f, 1.15f, 0.35f), amber);
        }

        private static void CreateWorkshop(Transform root, Material workshop, Material frame, Material amber)
        {
            CreateCube(root, "WorkshopBench_Left", new Vector3(22.0f, 0.82f, -1.72f), new Vector3(3.0f, 1.45f, 0.75f), workshop);
            CreateCube(root, "WorkshopBench_Right", new Vector3(27.2f, 0.82f, 1.72f), new Vector3(3.0f, 1.45f, 0.75f), workshop);
            CreateCube(root, "PartsRack", new Vector3(27.8f, 1.25f, -1.95f), new Vector3(1.7f, 2.3f, 0.45f), frame);
            for (int i = 0; i < 4; i++)
                CreateCube(root, "ToolGlow", new Vector3(21.0f + i * 0.55f, 1.48f, -1.31f), new Vector3(0.20f, 0.08f, 0.04f), amber);
        }

        private static void CreateGreenhouse(Transform root, Material green, Material frame, Material amber)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = 33.5f + i * 2.3f;
                CreateCube(root, "HydroponicBed", new Vector3(x, 0.55f, -1.35f), new Vector3(1.65f, 0.75f, 1.2f), frame);
                CreateCube(root, "PlantMass", new Vector3(x, 1.02f, -1.35f), new Vector3(1.45f, 0.28f, 1.0f), green);
                CreateCube(root, "GrowLight", new Vector3(x, 2.68f, -1.35f), new Vector3(1.6f, 0.08f, 0.18f), amber);
            }
            CreateCube(root, "WaterTank", new Vector3(40.3f, 1.0f, 1.75f), new Vector3(1.25f, 1.8f, 1.25f), frame);
        }

        private static void CreateWindowsAndLights(Transform root, float[] centers, Material frame, Material screen, Material amber)
        {
            foreach (float center in centers)
            {
                foreach (float offset in new[] { -3.2f, 0f, 3.2f })
                {
                    float x = center + offset;
                    CreateCube(root, "WindowFrame", new Vector3(x, 2.0f, -2.39f), new Vector3(2.0f, 1.05f, 0.055f), frame);
                    CreateCube(root, "WindowGlass", new Vector3(x, 2.0f, -2.35f), new Vector3(1.70f, 0.78f, 0.03f), screen);
                    CreateCube(root, "WindowFrame", new Vector3(x, 2.0f, 2.39f), new Vector3(2.0f, 1.05f, 0.055f), frame);
                    CreateCube(root, "WindowGlass", new Vector3(x, 2.0f, 2.35f), new Vector3(1.70f, 0.78f, 0.03f), screen);
                }

                CreateCube(root, "CeilingLightStrip", new Vector3(center, 3.24f, 0f), new Vector3(4.0f, 0.055f, 0.18f), amber);
                GameObject lightObj = new GameObject("InteriorLight");
                lightObj.transform.SetParent(root);
                lightObj.transform.position = new Vector3(center, 3.0f, 0f);
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 8f;
                light.intensity = 2.0f;
                light.color = new Color(1f, 0.72f, 0.43f);
                light.shadows = LightShadows.None;
            }
        }

        private static GameObject CreatePlayer(Vector3 position)
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
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            player.AddComponent<FpsPlayerController>();
            return player;
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

        private static Material Material(string fileName, Color color, float smoothness, float metallic, Color? emission = null)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new System.InvalidOperationException("URP/Lit shader not found.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = fileName };
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
