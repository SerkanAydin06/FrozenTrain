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
    public static class FrozenTrainPlayablePrototypeBuilder
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";

        private const float CarLength = 12f;
        private const float CarWidth = 5f;
        private const float CarHeight = 3.4f;
        private const float WallThickness = 0.18f;

        private const float WindowBottom = 1.42f;
        private const float WindowTop = 2.48f;
        private const float WindowWidth = 1.76f;

        [MenuItem("Frozen Train/Build Playable Prototype")]
        public static void BuildPlayablePrototype()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = Material("FPS_Floor", new Color(0.085f, 0.10f, 0.11f), 0.22f, 0.32f);
            Material wall = Material("FPS_Wall", new Color(0.14f, 0.17f, 0.19f), 0.28f, 0.42f);
            Material frame = Material("FPS_Frame", new Color(0.045f, 0.055f, 0.065f), 0.40f, 0.72f);
            Material generator = Material("FPS_Generator", new Color(0.28f, 0.12f, 0.05f), 0.34f, 0.40f);
            Material living = Material("FPS_Living", new Color(0.12f, 0.20f, 0.22f), 0.18f, 0.06f);
            Material workshop = Material("FPS_Workshop", new Color(0.21f, 0.16f, 0.09f), 0.25f, 0.40f);
            Material greenhouse = Material("FPS_Greenhouse", new Color(0.09f, 0.25f, 0.14f), 0.16f, 0.04f);
            Material screen = Material("FPS_Screen", new Color(0.04f, 0.27f, 0.34f), 0.62f, 0.18f, new Color(0.04f, 0.62f, 0.78f) * 1.6f);
            Material amber = Material("FPS_Amber", new Color(0.44f, 0.17f, 0.035f), 0.34f, 0.12f, new Color(0.92f, 0.30f, 0.04f) * 1.45f);
            Material snow = Material("FPS_Snow", new Color(0.74f, 0.81f, 0.88f), 0.08f, 0f);
            Material darkRock = Material("FPS_ExteriorRock", new Color(0.095f, 0.11f, 0.13f), 0.06f, 0f);
            Material distantRock = Material("FPS_DistantRock", new Color(0.16f, 0.19f, 0.22f), 0.04f, 0f);
            Material glass = TransparentMaterial("FPS_WindowGlass", new Color(0.46f, 0.68f, 0.78f, 0.16f), 0.78f, 0.05f);

            CreateLighting();
            CreateExterior(snow, darkRock, distantRock, frame);

            GameObject trainRoot = new GameObject("TRAIN_INTERIOR_PROTOTYPE");
            TrainPrototypeState state = trainRoot.AddComponent<TrainPrototypeState>();
            trainRoot.AddComponent<AudioSource>();
            trainRoot.AddComponent<TrainAmbience>();

            float[] centers = { -12.4f, 0f, 12.4f, 24.8f, 37.2f };
            string[] names = { "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse" };

            for (int i = 0; i < centers.Length; i++)
            {
                CreateCarShell(
                    trainRoot.transform,
                    names[i],
                    centers[i],
                    floor,
                    wall,
                    frame,
                    glass,
                    i == 0,
                    i == centers.Length - 1);
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
            CreateInteriorLighting(trainRoot.transform, centers, frame, amber);

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

            Debug.Log("FrozenTrain playable prototype rebuilt: sealed train ends, real windows, visible layered scenery and polished 5-car interior.");
        }

        private static void CreateLighting()
        {
            GameObject sunObject = new GameObject("ColdExteriorLight");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.62f;
            sun.color = new Color(0.62f, 0.74f, 0.95f);
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(34f, -32f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.085f, 0.13f, 0.19f);
            RenderSettings.ambientEquatorColor = new Color(0.045f, 0.060f, 0.082f);
            RenderSettings.ambientGroundColor = new Color(0.018f, 0.022f, 0.028f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.47f, 0.55f, 0.62f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 135f;
        }

        private static void CreateExterior(Material snow, Material nearRock, Material farRock, Material frame)
        {
            GameObject environment = new GameObject("OUTSIDE_ENVIRONMENT");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SnowGround";
            ground.transform.SetParent(environment.transform);
            ground.transform.position = new Vector3(12f, -0.65f, 0f);
            ground.transform.localScale = new Vector3(18f, 1f, 14f);
            ground.GetComponent<Renderer>().sharedMaterial = snow;

            GameObject nearLayer = new GameObject("NEAR_SCENERY");
            nearLayer.transform.SetParent(environment.transform);
            MovingScenery nearMovement = nearLayer.AddComponent<MovingScenery>();
            nearMovement.Configure(14f, -42f, 72f);

            for (int i = 0; i < 18; i++)
            {
                float x = -38f + i * 6.2f;
                float side = i % 2 == 0 ? -1f : 1f;
                float z = side * Random.Range(7.2f, 11.5f);

                GameObject rock = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                rock.name = "PassingRock";
                rock.transform.SetParent(nearLayer.transform);
                rock.transform.localPosition = new Vector3(x, -0.12f, z);
                rock.transform.localScale = new Vector3(Random.Range(1.2f, 3.8f), Random.Range(1.0f, 3.4f), Random.Range(1.2f, 3.8f));
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-9f, 9f), Random.Range(0f, 180f), Random.Range(-9f, 9f));
                rock.GetComponent<Renderer>().sharedMaterial = nearRock;

                CreateCube(
                    nearLayer.transform,
                    "SnowBank",
                    new Vector3(x + 2.3f, 0.05f, side * Random.Range(6.1f, 8.2f)),
                    new Vector3(Random.Range(2.0f, 5.0f), Random.Range(0.35f, 0.9f), Random.Range(1.2f, 2.5f)),
                    snow);
            }

            for (int i = 0; i < 15; i++)
            {
                float x = -40f + i * 7.5f;
                float z = i % 2 == 0 ? -6.35f : 6.35f;
                CreateCube(nearLayer.transform, "TrackSidePost", new Vector3(x, 1.35f, z), new Vector3(0.16f, 2.8f, 0.16f), frame);
                CreateCube(nearLayer.transform, "PostCrossArm", new Vector3(x, 2.45f, z), new Vector3(0.16f, 0.14f, 1.15f), frame);
            }

            GameObject farLayer = new GameObject("DISTANT_SCENERY");
            farLayer.transform.SetParent(environment.transform);
            MovingScenery farMovement = farLayer.AddComponent<MovingScenery>();
            farMovement.Configure(4.0f, -60f, 92f);

            for (int i = 0; i < 12; i++)
            {
                float x = -55f + i * 13f;
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mountain.name = "DistantMountain";
                mountain.transform.SetParent(farLayer.transform);
                mountain.transform.localPosition = new Vector3(x, 4.0f, side * Random.Range(18f, 28f));
                mountain.transform.localScale = new Vector3(Random.Range(8f, 15f), Random.Range(7f, 15f), Random.Range(5f, 10f));
                mountain.transform.localRotation = Quaternion.Euler(0f, Random.Range(20f, 70f), Random.Range(20f, 42f));
                mountain.GetComponent<Renderer>().sharedMaterial = farRock;
            }
        }

        private static void CreateCarShell(
            Transform parent,
            string name,
            float centerX,
            Material floor,
            Material wall,
            Material frame,
            Material glass,
            bool firstCar,
            bool lastCar)
        {
            GameObject car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(car.transform, "Floor", Vector3.zero, new Vector3(CarLength, 0.20f, CarWidth), floor);
            CreateCube(car.transform, "AislePlate", new Vector3(0f, 0.115f, 0f), new Vector3(CarLength - 0.45f, 0.03f, 1.38f), frame);
            CreateCube(car.transform, "Ceiling", new Vector3(0f, CarHeight, 0f), new Vector3(CarLength, 0.18f, CarWidth), wall);

            CreateWindowedSide(car.transform, -CarWidth * 0.5f, wall, frame, glass);
            CreateWindowedSide(car.transform, CarWidth * 0.5f, wall, frame, glass);

            if (firstCar)
            {
                CreateCabFront(car.transform, -CarLength * 0.5f, wall, frame, glass);
            }
            else
            {
                CreateDoorBulkhead(car.transform, -CarLength * 0.5f, frame);
            }

            if (lastCar)
            {
                CreateSolidEndCap(car.transform, CarLength * 0.5f, wall, frame);
            }
            else
            {
                CreateDoorBulkhead(car.transform, CarLength * 0.5f, frame);
            }

            for (float x = -4.5f; x <= 4.5f; x += 3f)
            {
                CreateCube(car.transform, "CeilingRib", new Vector3(x, CarHeight - 0.12f, 0f), new Vector3(0.18f, 0.20f, CarWidth - 0.15f), frame);
            }

            CreateCube(car.transform, "LowerRail_Left", new Vector3(0f, 0.58f, -2.37f), new Vector3(CarLength - 0.25f, 0.08f, 0.09f), frame);
            CreateCube(car.transform, "LowerRail_Right", new Vector3(0f, 0.58f, 2.37f), new Vector3(CarLength - 0.25f, 0.08f, 0.09f), frame);
        }

        private static void CreateWindowedSide(Transform parent, float localZ, Material wall, Material frame, Material glass)
        {
            float lowerHeight = WindowBottom - 0.10f;
            float upperHeight = CarHeight - WindowTop;
            float windowBandHeight = WindowTop - WindowBottom;
            float windowBandCenter = (WindowBottom + WindowTop) * 0.5f;

            CreateCube(
                parent,
                "WallLower",
                new Vector3(0f, 0.10f + lowerHeight * 0.5f, localZ),
                new Vector3(CarLength, lowerHeight, WallThickness),
                wall);

            CreateCube(
                parent,
                "WallUpper",
                new Vector3(0f, WindowTop + upperHeight * 0.5f, localZ),
                new Vector3(CarLength, upperHeight, WallThickness),
                wall);

            float[] windowCenters = { -3.2f, 0f, 3.2f };
            float halfWindow = WindowWidth * 0.5f;
            float leftEdge = -CarLength * 0.5f;

            foreach (float windowCenter in windowCenters)
            {
                float openingLeft = windowCenter - halfWindow;
                float segmentWidth = openingLeft - leftEdge;
                if (segmentWidth > 0.02f)
                {
                    CreateCube(
                        parent,
                        "WallPillar",
                        new Vector3(leftEdge + segmentWidth * 0.5f, windowBandCenter, localZ),
                        new Vector3(segmentWidth, windowBandHeight, WallThickness),
                        wall);
                }

                CreateWindow(parent, windowCenter, localZ, frame, glass);
                leftEdge = windowCenter + halfWindow;
            }

            float finalWidth = CarLength * 0.5f - leftEdge;
            if (finalWidth > 0.02f)
            {
                CreateCube(
                    parent,
                    "WallPillar",
                    new Vector3(leftEdge + finalWidth * 0.5f, windowBandCenter, localZ),
                    new Vector3(finalWidth, windowBandHeight, WallThickness),
                    wall);
            }
        }

        private static void CreateWindow(Transform parent, float localX, float localZ, Material frame, Material glass)
        {
            float height = WindowTop - WindowBottom;
            float centerY = (WindowBottom + WindowTop) * 0.5f;
            float direction = localZ > 0f ? -1f : 1f;
            float glassZ = localZ + direction * 0.018f;

            CreateCube(parent, "WindowTopFrame", new Vector3(localX, WindowTop, localZ), new Vector3(WindowWidth + 0.16f, 0.10f, 0.12f), frame);
            CreateCube(parent, "WindowBottomFrame", new Vector3(localX, WindowBottom, localZ), new Vector3(WindowWidth + 0.16f, 0.10f, 0.12f), frame);
            CreateCube(parent, "WindowLeftFrame", new Vector3(localX - WindowWidth * 0.5f, centerY, localZ), new Vector3(0.10f, height, 0.12f), frame);
            CreateCube(parent, "WindowRightFrame", new Vector3(localX + WindowWidth * 0.5f, centerY, localZ), new Vector3(0.10f, height, 0.12f), frame);

            GameObject pane = CreateCube(
                parent,
                "WindowGlass",
                new Vector3(localX, centerY, glassZ),
                new Vector3(WindowWidth - 0.12f, height - 0.12f, 0.035f),
                glass);

            BoxCollider collider = pane.GetComponent<BoxCollider>();
            collider.size = Vector3.one;
        }

        private static void CreateCabFront(Transform parent, float localX, Material wall, Material frame, Material glass)
        {
            const float lowerTop = 1.40f;
            const float upperBottom = 2.55f;
            const float windshieldWidth = 1.42f;
            const float windshieldHeight = 0.92f;
            const float windshieldCenterY = 1.98f;

            CreateCube(parent, "CabFrontLower", new Vector3(localX, lowerTop * 0.5f, 0f), new Vector3(WallThickness, lowerTop, CarWidth), wall);
            CreateCube(parent, "CabFrontUpper", new Vector3(localX, upperBottom + (CarHeight - upperBottom) * 0.5f, 0f), new Vector3(WallThickness, CarHeight - upperBottom, CarWidth), wall);
            CreateCube(parent, "CabFrontCenterPillar", new Vector3(localX, windshieldCenterY, 0f), new Vector3(WallThickness, 1.15f, 0.26f), frame);
            CreateCube(parent, "CabFrontLeftPillar", new Vector3(localX, windshieldCenterY, -2.12f), new Vector3(WallThickness, 1.15f, 0.76f), wall);
            CreateCube(parent, "CabFrontRightPillar", new Vector3(localX, windshieldCenterY, 2.12f), new Vector3(WallThickness, 1.15f, 0.76f), wall);

            foreach (float z in new[] { -1.0f, 1.0f })
            {
                GameObject pane = CreateCube(
                    parent,
                    "CabWindshield",
                    new Vector3(localX + 0.02f, windshieldCenterY, z),
                    new Vector3(0.035f, windshieldHeight, windshieldWidth),
                    glass);
                pane.GetComponent<BoxCollider>().size = Vector3.one;
            }
        }

        private static void CreateSolidEndCap(Transform parent, float localX, Material wall, Material frame)
        {
            CreateCube(parent, "RearEndWall", new Vector3(localX, CarHeight * 0.5f, 0f), new Vector3(WallThickness, CarHeight, CarWidth), wall);
            CreateCube(parent, "RearEndBraceVertical", new Vector3(localX - 0.015f, CarHeight * 0.5f, 0f), new Vector3(0.08f, CarHeight - 0.25f, 0.16f), frame);
            CreateCube(parent, "RearEndBraceHorizontal", new Vector3(localX - 0.015f, 1.72f, 0f), new Vector3(0.08f, 0.16f, CarWidth - 0.35f), frame);
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
            CreateCube(root, "DriverConsole", new Vector3(-16.55f, 0.75f, 0f), new Vector3(0.88f, 1.30f, 3.25f), frame);

            for (int i = -1; i <= 1; i++)
            {
                CreateCube(root, "DriverScreen", new Vector3(-16.06f, 1.36f, i * 0.94f), new Vector3(0.045f, 0.54f, 0.68f), screen);
                CreateCube(root, "DriverStatus", new Vector3(-16.03f, 1.02f, i * 0.94f), new Vector3(0.035f, 0.08f, 0.26f), amber);
            }

            CreateCube(root, "DriverSeat", new Vector3(-14.72f, 0.62f, 0f), new Vector3(0.72f, 0.84f, 0.82f), frame);

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

            for (int i = -2; i <= 2; i++)
            {
                CreateCube(root, "FloorHazardMark", new Vector3(i * 0.72f, 0.235f, -0.10f), new Vector3(0.42f, 0.025f, 0.14f), amber);
            }
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
            {
                CreateCube(root, "ToolGlow", new Vector3(21.0f + i * 0.55f, 1.48f, -1.31f), new Vector3(0.20f, 0.08f, 0.04f), amber);
            }
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

        private static void CreateInteriorLighting(Transform root, float[] centers, Material frame, Material amber)
        {
            foreach (float center in centers)
            {
                CreateCube(root, "CeilingLightHousing", new Vector3(center, 3.235f, 0f), new Vector3(4.25f, 0.08f, 0.28f), frame);
                CreateCube(root, "CeilingLightStrip", new Vector3(center, 3.19f, 0f), new Vector3(3.75f, 0.035f, 0.16f), amber);

                GameObject lightObject = new GameObject("InteriorLight");
                lightObject.transform.SetParent(root);
                lightObject.transform.position = new Vector3(center, 2.95f, 0f);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 7.6f;
                light.intensity = 1.55f;
                light.color = new Color(1f, 0.70f, 0.42f);
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
            camera.farClipPlane = 220f;
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
            if (shader == null)
            {
                throw new System.InvalidOperationException("URP/Lit shader not found.");
            }

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

        private static Material TransparentMaterial(string fileName, Color color, float smoothness, float metallic)
        {
            Material material = Material(fileName, color, smoothness, metallic);

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
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
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
