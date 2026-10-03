#if UNITY_EDITOR
using FrozenTrain.Atmosphere;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Internal first-run source used only when authored prefab assets do not exist yet.
    /// It intentionally exposes no Unity menu entry. The public workflow is
    /// Frozen Train > Rebuild Authored Train.
    /// </summary>
    public static class FrozenTrainPlayablePrototypeBuilder
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";

        private const float CarLength = 15f;
        private const float CarWidth = 7.2f;
        private const float CarHeight = 3.9f;
        private const float WallThickness = 0.20f;
        private const float CarSpacing = 15.8f;

        private static readonly float[] Centers = { -31.6f, -15.8f, 0f, 15.8f, 31.6f };
        private static readonly string[] CarNames = { "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse" };

        public static void BuildPlayablePrototype()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = GetOrCreateMaterial("Train_Floor", new Color(0.075f, 0.085f, 0.095f), 0.24f, 0.44f);
            Material wall = GetOrCreateMaterial("Train_Wall", new Color(0.16f, 0.18f, 0.19f), 0.24f, 0.34f);
            Material frame = GetOrCreateMaterial("Train_Frame", new Color(0.045f, 0.052f, 0.060f), 0.38f, 0.78f);
            Material steel = GetOrCreateMaterial("Train_Steel", new Color(0.23f, 0.26f, 0.28f), 0.42f, 0.80f);
            Material rubber = GetOrCreateMaterial("Train_Rubber", new Color(0.025f, 0.030f, 0.035f), 0.10f, 0.02f);
            Material generator = GetOrCreateMaterial("Train_Generator", new Color(0.26f, 0.10f, 0.045f), 0.32f, 0.56f);
            Material living = GetOrCreateMaterial("Train_Living", new Color(0.10f, 0.19f, 0.22f), 0.18f, 0.05f);
            Material mattress = GetOrCreateMaterial("Train_Mattress", new Color(0.20f, 0.25f, 0.27f), 0.08f, 0f);
            Material workshop = GetOrCreateMaterial("Train_Workshop", new Color(0.25f, 0.18f, 0.08f), 0.23f, 0.36f);
            Material greenhouse = GetOrCreateMaterial("Train_Greenhouse", new Color(0.07f, 0.25f, 0.12f), 0.16f, 0.04f);
            Material plant = GetOrCreateMaterial("Train_Plant", new Color(0.035f, 0.18f, 0.07f), 0.08f, 0f);
            Material screen = GetOrCreateMaterial("Train_Screen", new Color(0.025f, 0.21f, 0.28f), 0.58f, 0.10f, new Color(0.03f, 0.70f, 0.90f) * 1.5f);
            Material amber = GetOrCreateMaterial("Train_Amber", new Color(0.46f, 0.16f, 0.025f), 0.30f, 0.10f, new Color(0.95f, 0.31f, 0.035f) * 1.35f);
            Material snow = GetOrCreateMaterial("Train_Snow", new Color(0.76f, 0.83f, 0.90f), 0.08f, 0f);
            Material nearRock = GetOrCreateMaterial("Train_ExteriorRock", new Color(0.090f, 0.105f, 0.12f), 0.06f, 0f);
            Material farRock = GetOrCreateMaterial("Train_DistantRock", new Color(0.16f, 0.19f, 0.22f), 0.04f, 0f);
            Material glass = GetOrCreateTransparentMaterial("Train_WindowGlass", new Color(0.42f, 0.62f, 0.72f, 0.13f), 0.80f, 0.04f);

            // Keep these references alive as project materials even though the internal base scene is intentionally minimal.
            _ = generator;
            _ = living;
            _ = mattress;
            _ = workshop;
            _ = greenhouse;
            _ = plant;
            _ = screen;
            _ = amber;

            CreateLighting();
            CreateExterior(snow, nearRock, farRock, frame);

            GameObject trainRoot = new GameObject("TRAIN_INTERIOR_PROTOTYPE");
            TrainPrototypeState state = trainRoot.AddComponent<TrainPrototypeState>();
            trainRoot.AddComponent<SurvivalSimulation>();
            trainRoot.AddComponent<AudioSource>();
            trainRoot.AddComponent<TrainAmbience>();

            for (int i = 0; i < Centers.Length; i++)
            {
                CreateBaseCarShell(
                    trainRoot.transform,
                    CarNames[i],
                    Centers[i],
                    floor,
                    wall,
                    frame,
                    glass,
                    i == 0,
                    i == Centers.Length - 1);
            }

            for (int i = 0; i < Centers.Length - 1; i++)
            {
                float x = (Centers[i] + Centers[i + 1]) * 0.5f;
                CreateConnector(trainRoot.transform, x, floor, frame, rubber);
            }

            CreateInteriorLights(trainRoot.transform, frame, amber);

            GameObject player = CreatePlayer(new Vector3(0f, 0.12f, 0f));
            FpsPlayerController controller = player.GetComponent<FpsPlayerController>();
            Camera camera = player.GetComponentInChildren<Camera>(true);
            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
            interaction.Configure(camera, 3.1f);
            TrainManagementPanel panel = player.AddComponent<TrainManagementPanel>();
            panel.Configure(state, controller);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;
        }

        private static void CreateLighting()
        {
            GameObject sunObject = new GameObject("ColdExteriorLight");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.58f;
            sun.color = new Color(0.60f, 0.72f, 0.94f);
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(33f, -28f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.085f, 0.12f, 0.17f);
            RenderSettings.ambientEquatorColor = new Color(0.045f, 0.055f, 0.070f);
            RenderSettings.ambientGroundColor = new Color(0.018f, 0.022f, 0.028f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.46f, 0.53f, 0.60f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 42f;
            RenderSettings.fogEndDistance = 155f;
        }

        private static void CreateExterior(Material snow, Material nearRock, Material farRock, Material frame)
        {
            GameObject environment = new GameObject("OUTSIDE_ENVIRONMENT");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SnowGround";
            ground.transform.SetParent(environment.transform);
            ground.transform.position = new Vector3(0f, -0.66f, 0f);
            ground.transform.localScale = new Vector3(28f, 1f, 18f);
            ground.GetComponent<Renderer>().sharedMaterial = snow;

            GameObject nearLayer = new GameObject("NEAR_SCENERY");
            nearLayer.transform.SetParent(environment.transform);
            nearLayer.AddComponent<MovingScenery>().Configure(14f, -55f, 90f);

            for (int i = 0; i < 18; i++)
            {
                float x = -55f + i * 7.2f;
                float side = i % 2 == 0 ? -1f : 1f;
                float z = side * Random.Range(9.5f, 14f);
                GameObject rock = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                rock.name = "PassingRock";
                rock.transform.SetParent(nearLayer.transform);
                rock.transform.localPosition = new Vector3(x, -0.12f, z);
                rock.transform.localScale = new Vector3(Random.Range(1.4f, 4.2f), Random.Range(1.0f, 3.6f), Random.Range(1.3f, 4.0f));
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 180f), Random.Range(-8f, 8f));
                rock.GetComponent<Renderer>().sharedMaterial = nearRock;
            }

            for (int i = 0; i < 14; i++)
            {
                float x = -58f + i * 9f;
                float z = i % 2 == 0 ? -8.2f : 8.2f;
                CreateCube(nearLayer.transform, "TrackSidePost", new Vector3(x, 1.35f, z), new Vector3(0.15f, 2.8f, 0.15f), frame);
            }

            GameObject farLayer = new GameObject("DISTANT_SCENERY");
            farLayer.transform.SetParent(environment.transform);
            farLayer.AddComponent<MovingScenery>().Configure(4f, -75f, 115f);

            for (int i = 0; i < 12; i++)
            {
                float x = -75f + i * 15f;
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mountain.name = "DistantMountain";
                mountain.transform.SetParent(farLayer.transform);
                mountain.transform.localPosition = new Vector3(x, 4.8f, side * Random.Range(22f, 34f));
                mountain.transform.localScale = new Vector3(Random.Range(9f, 17f), Random.Range(8f, 16f), Random.Range(6f, 11f));
                mountain.transform.localRotation = Quaternion.Euler(0f, Random.Range(20f, 70f), Random.Range(20f, 40f));
                mountain.GetComponent<Renderer>().sharedMaterial = farRock;
            }
        }

        private static void CreateBaseCarShell(Transform parent, string name, float centerX, Material floor, Material wall, Material frame, Material glass, bool firstCar, bool lastCar)
        {
            GameObject car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(car.transform, "Floor", Vector3.zero, new Vector3(CarLength, 0.22f, CarWidth), floor);
            CreateCube(car.transform, "Ceiling", new Vector3(0f, CarHeight, 0f), new Vector3(CarLength, 0.20f, CarWidth), wall);
            CreateCube(car.transform, "WallLeft", new Vector3(0f, CarHeight * 0.5f, -CarWidth * 0.5f), new Vector3(CarLength, CarHeight, WallThickness), wall);
            CreateCube(car.transform, "WallRight", new Vector3(0f, CarHeight * 0.5f, CarWidth * 0.5f), new Vector3(CarLength, CarHeight, WallThickness), wall);

            if (firstCar) CreateCabFront(car.transform, -CarLength * 0.5f, wall, frame, glass);
            else CreateDoorBulkhead(car.transform, -CarLength * 0.5f, frame);

            if (lastCar) CreateSolidEndCap(car.transform, CarLength * 0.5f, wall, frame);
            else CreateDoorBulkhead(car.transform, CarLength * 0.5f, frame);
        }

        private static void CreateCabFront(Transform parent, float x, Material wall, Material frame, Material glass)
        {
            const float lowerTop = 1.42f;
            const float upperBottom = 2.82f;
            CreateCube(parent, "CabFrontLower", new Vector3(x, lowerTop * 0.5f, 0f), new Vector3(WallThickness, lowerTop, CarWidth), wall);
            CreateCube(parent, "CabFrontUpper", new Vector3(x, upperBottom + (CarHeight - upperBottom) * 0.5f, 0f), new Vector3(WallThickness, CarHeight - upperBottom, CarWidth), wall);

            float[] zCenters = { -2.15f, 0f, 2.15f };
            foreach (float z in zCenters)
                CreateCube(parent, "CabWindshield", new Vector3(x + 0.02f, 2.10f, z), new Vector3(0.035f, 1.16f, 1.65f), glass);

            CreateCube(parent, "CabFrontPillarL", new Vector3(x, 2.10f, -1.08f), new Vector3(WallThickness, 1.38f, 0.20f), frame);
            CreateCube(parent, "CabFrontPillarR", new Vector3(x, 2.10f, 1.08f), new Vector3(WallThickness, 1.38f, 0.20f), frame);
        }

        private static void CreateSolidEndCap(Transform parent, float x, Material wall, Material frame)
        {
            CreateCube(parent, "RearEndWall", new Vector3(x, CarHeight * 0.5f, 0f), new Vector3(WallThickness, CarHeight, CarWidth), wall);
            CreateCube(parent, "RearEndBraceVertical", new Vector3(x - 0.02f, CarHeight * 0.5f, 0f), new Vector3(0.08f, CarHeight - 0.28f, 0.18f), frame);
        }

        private static void CreateDoorBulkhead(Transform parent, float x, Material material)
        {
            const float doorWidth = 1.95f;
            const float doorHeight = 2.62f;
            float sideWidth = (CarWidth - doorWidth) * 0.5f;
            float sideCenter = doorWidth * 0.5f + sideWidth * 0.5f;
            CreateCube(parent, "BulkheadSide", new Vector3(x, CarHeight * 0.5f, -sideCenter), new Vector3(WallThickness, CarHeight, sideWidth), material);
            CreateCube(parent, "BulkheadSide", new Vector3(x, CarHeight * 0.5f, sideCenter), new Vector3(WallThickness, CarHeight, sideWidth), material);
            float lintel = CarHeight - doorHeight;
            CreateCube(parent, "BulkheadLintel", new Vector3(x, doorHeight + lintel * 0.5f, 0f), new Vector3(WallThickness, lintel, doorWidth), material);
        }

        private static void CreateConnector(Transform parent, float centerX, Material floor, Material frame, Material rubber)
        {
            GameObject connector = new GameObject("CarConnector");
            connector.transform.SetParent(parent);
            connector.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(connector.transform, "ConnectorFloor", Vector3.zero, new Vector3(0.80f, 0.22f, 2.15f), floor);
            CreateCube(connector.transform, "ConnectorCeiling", new Vector3(0f, 2.88f, 0f), new Vector3(0.80f, 0.16f, 2.15f), frame);
            CreateCube(connector.transform, "BellowsLeft", new Vector3(0f, 1.45f, -1.06f), new Vector3(0.80f, 2.9f, 0.12f), rubber);
            CreateCube(connector.transform, "BellowsRight", new Vector3(0f, 1.45f, 1.06f), new Vector3(0.80f, 2.9f, 0.12f), rubber);

            GameObject door = CreateCube(connector.transform, "SlidingDoor", new Vector3(0f, 1.30f, 0f), new Vector3(0.12f, 2.48f, 1.82f), frame);
            door.AddComponent<SlidingDoor>().Configure(new Vector3(0f, 0f, -1.90f), 2.8f);
        }

        private static void CreateInteriorLights(Transform root, Material frame, Material amber)
        {
            for (int index = 0; index < Centers.Length; index++)
            {
                float center = Centers[index];
                CreateCube(root, "CeilingLightHousing", new Vector3(center, 3.58f, 0f), new Vector3(3.0f, 0.08f, 0.30f), frame);
                CreateCube(root, "CeilingLightStrip", new Vector3(center, 3.53f, 0f), new Vector3(2.6f, 0.035f, 0.18f), amber);

                GameObject lightObject = new GameObject("InteriorLight");
                lightObject.transform.SetParent(root);
                lightObject.transform.position = new Vector3(center, 3.05f, 0f);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 9.0f;
                light.intensity = 1.2f;
                light.color = new Color(0.92f, 0.80f, 0.64f);
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
            camera.farClipPlane = 260f;
            cameraObject.AddComponent<AudioListener>();

            player.AddComponent<FpsPlayerController>();
            return player;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = position;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = scale;
            if (material != null) cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material GetOrCreateMaterial(string fileName, Color color, float smoothness, float metallic, Color? emission = null)
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
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
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

        private static Material GetOrCreateTransparentMaterial(string fileName, Color color, float smoothness, float metallic)
        {
            Material material = GetOrCreateMaterial(fileName, color, smoothness, metallic);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
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