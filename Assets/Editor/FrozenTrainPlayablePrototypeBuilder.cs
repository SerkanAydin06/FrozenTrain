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

        private const float CarLength = 15f;
        private const float CarWidth = 7.2f;
        private const float CarHeight = 3.9f;
        private const float WallThickness = 0.20f;
        private const float CarSpacing = 15.8f;

        private const float WindowBottom = 1.38f;
        private const float WindowTop = 2.72f;
        private const float WindowWidth = 2.05f;

        private static readonly float[] Centers = { -31.6f, -15.8f, 0f, 15.8f, 31.6f };
        private static readonly string[] CarNames = { "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse" };

        [MenuItem("Frozen Train/Build Playable Prototype")]
        public static void BuildPlayablePrototype()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = Material("Train_Floor", new Color(0.075f, 0.085f, 0.095f), 0.24f, 0.44f);
            Material wall = Material("Train_Wall", new Color(0.16f, 0.18f, 0.19f), 0.24f, 0.34f);
            Material frame = Material("Train_Frame", new Color(0.045f, 0.052f, 0.060f), 0.38f, 0.78f);
            Material steel = Material("Train_Steel", new Color(0.23f, 0.26f, 0.28f), 0.42f, 0.80f);
            Material rubber = Material("Train_Rubber", new Color(0.025f, 0.030f, 0.035f), 0.10f, 0.02f);
            Material generator = Material("Train_Generator", new Color(0.26f, 0.10f, 0.045f), 0.32f, 0.56f);
            Material living = Material("Train_Living", new Color(0.10f, 0.19f, 0.22f), 0.18f, 0.05f);
            Material mattress = Material("Train_Mattress", new Color(0.20f, 0.25f, 0.27f), 0.08f, 0f);
            Material workshop = Material("Train_Workshop", new Color(0.25f, 0.18f, 0.08f), 0.23f, 0.36f);
            Material greenhouse = Material("Train_Greenhouse", new Color(0.07f, 0.25f, 0.12f), 0.16f, 0.04f);
            Material plant = Material("Train_Plant", new Color(0.035f, 0.18f, 0.07f), 0.08f, 0f);
            Material screen = Material("Train_Screen", new Color(0.025f, 0.21f, 0.28f), 0.58f, 0.10f, new Color(0.03f, 0.70f, 0.90f) * 1.5f);
            Material amber = Material("Train_Amber", new Color(0.46f, 0.16f, 0.025f), 0.30f, 0.10f, new Color(0.95f, 0.31f, 0.035f) * 1.35f);
            Material snow = Material("Train_Snow", new Color(0.76f, 0.83f, 0.90f), 0.08f, 0f);
            Material darkRock = Material("Train_ExteriorRock", new Color(0.090f, 0.105f, 0.12f), 0.06f, 0f);
            Material distantRock = Material("Train_DistantRock", new Color(0.16f, 0.19f, 0.22f), 0.04f, 0f);
            Material glass = TransparentMaterial("Train_WindowGlass", new Color(0.42f, 0.62f, 0.72f, 0.13f), 0.80f, 0.04f);

            CreateLighting();
            CreateExterior(snow, darkRock, distantRock, frame);

            GameObject trainRoot = new GameObject("TRAIN_INTERIOR_PROTOTYPE");
            TrainPrototypeState state = trainRoot.AddComponent<TrainPrototypeState>();
            trainRoot.AddComponent<SurvivalSimulation>();
            trainRoot.AddComponent<AudioSource>();
            trainRoot.AddComponent<TrainAmbience>();

            for (int i = 0; i < Centers.Length; i++)
            {
                CreateCarShell(
                    trainRoot.transform,
                    CarNames[i],
                    Centers[i],
                    floor,
                    wall,
                    frame,
                    steel,
                    glass,
                    i == 0,
                    i == Centers.Length - 1);
            }

            for (int i = 0; i < Centers.Length - 1; i++)
            {
                float x = (Centers[i] + Centers[i + 1]) * 0.5f;
                CreateConnectorWithDoor(trainRoot.transform, x, floor, frame, rubber);
            }

            CreateLocomotive(trainRoot.transform, frame, steel, screen, amber, rubber);
            CreateGenerator(trainRoot.transform, generator, frame, steel, screen, amber);
            CreateLiving(trainRoot.transform, living, mattress, frame, steel, amber);
            CreateWorkshop(trainRoot.transform, workshop, frame, steel, screen, amber);
            CreateGreenhouse(trainRoot.transform, greenhouse, plant, frame, steel, amber);
            CreateInteriorLighting(trainRoot.transform, frame, amber);

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

            Debug.Log("FrozenTrain rebuilt: wide five-car survival layout, clean floors, functional interiors, real windows and sealed ends.");
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

            for (int i = 0; i < 22; i++)
            {
                float x = -55f + i * 6.3f;
                float side = i % 2 == 0 ? -1f : 1f;
                float z = side * Random.Range(9.0f, 14f);

                GameObject rock = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                rock.name = "PassingRock";
                rock.transform.SetParent(nearLayer.transform);
                rock.transform.localPosition = new Vector3(x, -0.12f, z);
                rock.transform.localScale = new Vector3(Random.Range(1.4f, 4.2f), Random.Range(1.0f, 3.6f), Random.Range(1.3f, 4.0f));
                rock.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 180f), Random.Range(-8f, 8f));
                rock.GetComponent<Renderer>().sharedMaterial = nearRock;
            }

            for (int i = 0; i < 18; i++)
            {
                float x = -58f + i * 7.8f;
                float z = i % 2 == 0 ? -8.0f : 8.0f;
                CreateCube(nearLayer.transform, "TrackSidePost", new Vector3(x, 1.35f, z), new Vector3(0.15f, 2.8f, 0.15f), frame);
            }

            GameObject farLayer = new GameObject("DISTANT_SCENERY");
            farLayer.transform.SetParent(environment.transform);
            farLayer.AddComponent<MovingScenery>().Configure(4.0f, -75f, 115f);

            for (int i = 0; i < 14; i++)
            {
                float x = -75f + i * 14f;
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

        private static void CreateCarShell(Transform parent, string name, float centerX, Material floor, Material wall, Material frame, Material steel, Material glass, bool firstCar, bool lastCar)
        {
            GameObject car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = new Vector3(centerX, 0f, 0f);

            CreateCube(car.transform, "Floor", Vector3.zero, new Vector3(CarLength, 0.22f, CarWidth), floor);
            CreateCube(car.transform, "Ceiling", new Vector3(0f, CarHeight, 0f), new Vector3(CarLength, 0.20f, CarWidth), wall);

            CreateWindowedSide(car.transform, -CarWidth * 0.5f, wall, frame, glass);
            CreateWindowedSide(car.transform, CarWidth * 0.5f, wall, frame, glass);

            if (firstCar) CreateCabFront(car.transform, -CarLength * 0.5f, wall, frame, glass);
            else CreateDoorBulkhead(car.transform, -CarLength * 0.5f, frame);

            if (lastCar) CreateSolidEndCap(car.transform, CarLength * 0.5f, wall, frame);
            else CreateDoorBulkhead(car.transform, CarLength * 0.5f, frame);

            for (float x = -6f; x <= 6f; x += 3f)
            {
                CreateCube(car.transform, "CeilingRib", new Vector3(x, CarHeight - 0.13f, 0f), new Vector3(0.15f, 0.20f, CarWidth - 0.20f), frame);
            }

            CreateCube(car.transform, "BaseTrimLeft", new Vector3(0f, 0.35f, -3.48f), new Vector3(CarLength - 0.25f, 0.13f, 0.12f), steel);
            CreateCube(car.transform, "BaseTrimRight", new Vector3(0f, 0.35f, 3.48f), new Vector3(CarLength - 0.25f, 0.13f, 0.12f), steel);
            CreateCube(car.transform, "CeilingDuctLeft", new Vector3(0f, 3.52f, -2.75f), new Vector3(CarLength - 0.7f, 0.28f, 0.42f), frame);
            CreateCube(car.transform, "CeilingDuctRight", new Vector3(0f, 3.52f, 2.75f), new Vector3(CarLength - 0.7f, 0.28f, 0.42f), frame);
        }

        private static void CreateWindowedSide(Transform parent, float localZ, Material wall, Material frame, Material glass)
        {
            float lowerHeight = WindowBottom - 0.11f;
            float upperHeight = CarHeight - WindowTop;
            float bandHeight = WindowTop - WindowBottom;
            float bandCenter = (WindowBottom + WindowTop) * 0.5f;

            CreateCube(parent, "WallLower", new Vector3(0f, 0.11f + lowerHeight * 0.5f, localZ), new Vector3(CarLength, lowerHeight, WallThickness), wall);
            CreateCube(parent, "WallUpper", new Vector3(0f, WindowTop + upperHeight * 0.5f, localZ), new Vector3(CarLength, upperHeight, WallThickness), wall);

            float[] windows = { -5.15f, -1.75f, 1.75f, 5.15f };
            float half = WindowWidth * 0.5f;
            float leftEdge = -CarLength * 0.5f;

            foreach (float center in windows)
            {
                float openingLeft = center - half;
                float segment = openingLeft - leftEdge;
                if (segment > 0.02f)
                {
                    CreateCube(parent, "WallPillar", new Vector3(leftEdge + segment * 0.5f, bandCenter, localZ), new Vector3(segment, bandHeight, WallThickness), wall);
                }
                CreateWindow(parent, center, localZ, frame, glass);
                leftEdge = center + half;
            }

            float finalWidth = CarLength * 0.5f - leftEdge;
            if (finalWidth > 0.02f)
            {
                CreateCube(parent, "WallPillar", new Vector3(leftEdge + finalWidth * 0.5f, bandCenter, localZ), new Vector3(finalWidth, bandHeight, WallThickness), wall);
            }
        }

        private static void CreateWindow(Transform parent, float x, float z, Material frame, Material glass)
        {
            float height = WindowTop - WindowBottom;
            float y = (WindowBottom + WindowTop) * 0.5f;
            float inward = z > 0f ? -1f : 1f;

            CreateCube(parent, "WindowTopFrame", new Vector3(x, WindowTop, z), new Vector3(WindowWidth + 0.16f, 0.10f, 0.13f), frame);
            CreateCube(parent, "WindowBottomFrame", new Vector3(x, WindowBottom, z), new Vector3(WindowWidth + 0.16f, 0.10f, 0.13f), frame);
            CreateCube(parent, "WindowLeftFrame", new Vector3(x - WindowWidth * 0.5f, y, z), new Vector3(0.10f, height, 0.13f), frame);
            CreateCube(parent, "WindowRightFrame", new Vector3(x + WindowWidth * 0.5f, y, z), new Vector3(0.10f, height, 0.13f), frame);
            CreateCube(parent, "WindowGlass", new Vector3(x, y, z + inward * 0.02f), new Vector3(WindowWidth - 0.12f, height - 0.12f, 0.035f), glass);
        }

        private static void CreateCabFront(Transform parent, float x, Material wall, Material frame, Material glass)
        {
            const float lowerTop = 1.42f;
            const float upperBottom = 2.82f;
            CreateCube(parent, "CabFrontLower", new Vector3(x, lowerTop * 0.5f, 0f), new Vector3(WallThickness, lowerTop, CarWidth), wall);
            CreateCube(parent, "CabFrontUpper", new Vector3(x, upperBottom + (CarHeight - upperBottom) * 0.5f, 0f), new Vector3(WallThickness, CarHeight - upperBottom, CarWidth), wall);

            float[] zCenters = { -2.15f, 0f, 2.15f };
            foreach (float z in zCenters)
            {
                GameObject pane = CreateCube(parent, "CabWindshield", new Vector3(x + 0.02f, 2.10f, z), new Vector3(0.035f, 1.16f, 1.65f), glass);
                pane.GetComponent<BoxCollider>().size = Vector3.one;
            }

            CreateCube(parent, "CabFrontPillarL", new Vector3(x, 2.10f, -1.08f), new Vector3(WallThickness, 1.38f, 0.20f), frame);
            CreateCube(parent, "CabFrontPillarR", new Vector3(x, 2.10f, 1.08f), new Vector3(WallThickness, 1.38f, 0.20f), frame);
        }

        private static void CreateSolidEndCap(Transform parent, float x, Material wall, Material frame)
        {
            CreateCube(parent, "RearEndWall", new Vector3(x, CarHeight * 0.5f, 0f), new Vector3(WallThickness, CarHeight, CarWidth), wall);
            CreateCube(parent, "RearEndBraceVertical", new Vector3(x - 0.02f, CarHeight * 0.5f, 0f), new Vector3(0.08f, CarHeight - 0.28f, 0.18f), frame);
            CreateCube(parent, "RearEndBraceHorizontal", new Vector3(x - 0.02f, 1.95f, 0f), new Vector3(0.08f, 0.16f, CarWidth - 0.45f), frame);
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

        private static void CreateConnectorWithDoor(Transform parent, float centerX, Material floor, Material frame, Material rubber)
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

        private static void CreateLocomotive(Transform root, Material frame, Material steel, Material screen, Material amber, Material rubber)
        {
            float c = Centers[0];
            GameObject group = NewGroup(root, "Locomotive_Interior");

            CreateCube(group.transform, "MainConsole", new Vector3(c - 5.65f, 0.82f, 0f), new Vector3(1.10f, 1.42f, 4.80f), frame);
            for (int i = -2; i <= 2; i++)
            {
                float z = i * 0.88f;
                CreateCube(group.transform, "MainDisplay", new Vector3(c - 5.05f, 1.38f, z), new Vector3(0.045f, 0.52f, 0.64f), screen);
            }

            CreateSeat(group.transform, new Vector3(c - 3.85f, 0.48f, -1.15f), frame, rubber);
            CreateSeat(group.transform, new Vector3(c - 3.85f, 0.48f, 1.15f), frame, rubber);

            CreateCabinet(group.transform, new Vector3(c - 0.7f, 1.25f, -3.18f), new Vector3(2.8f, 2.30f, 0.65f), frame, steel);
            CreateCabinet(group.transform, new Vector3(c + 2.6f, 1.25f, -3.18f), new Vector3(2.8f, 2.30f, 0.65f), frame, steel);

            GameObject terminal = new GameObject("ManagementTerminal");
            terminal.transform.SetParent(group.transform);
            terminal.transform.position = new Vector3(c + 2.4f, 0f, 2.95f);
            terminal.AddComponent<ManagementTerminal>();
            CreateCube(terminal.transform, "TerminalBody", new Vector3(0f, 0.95f, 0f), new Vector3(1.8f, 1.65f, 0.55f), frame);
            CreateCube(terminal.transform, "TerminalScreen", new Vector3(0f, 1.12f, -0.295f), new Vector3(1.35f, 0.72f, 0.035f), screen);
            CreateCube(terminal.transform, "TerminalControls", new Vector3(0f, 0.43f, -0.30f), new Vector3(1.35f, 0.16f, 0.40f), amber);
        }

        private static void CreateGenerator(Transform root, Material generatorMat, Material frame, Material steel, Material screen, Material amber)
        {
            float c = Centers[1];
            GameObject group = NewGroup(root, "Generator_Interior");

            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            main.name = "MainGenerator";
            main.transform.SetParent(group.transform);
            main.transform.position = new Vector3(c, 1.25f, 2.10f);
            main.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            main.transform.localScale = new Vector3(1.15f, 3.70f, 1.15f);
            main.GetComponent<Renderer>().sharedMaterial = generatorMat;

            CreateCube(group.transform, "GeneratorBase", new Vector3(c, 0.33f, 2.10f), new Vector3(7.8f, 0.42f, 2.55f), frame);
            CreateCube(group.transform, "ServiceRail", new Vector3(c, 0.88f, 0.65f), new Vector3(7.8f, 0.10f, 0.10f), steel);

            for (int i = -2; i <= 2; i++)
            {
                CreateCabinet(group.transform, new Vector3(c - 4.6f + i * 2.15f, 1.05f, -3.10f), new Vector3(1.65f, 1.90f, 0.72f), frame, steel);
            }

            CreateCube(group.transform, "GeneratorConsole", new Vector3(c - 5.45f, 0.90f, -1.95f), new Vector3(1.55f, 1.55f, 0.62f), frame);
            CreateCube(group.transform, "GeneratorScreen", new Vector3(c - 5.45f, 1.20f, -1.62f), new Vector3(1.05f, 0.50f, 0.04f), screen);

            for (int p = 0; p < 3; p++)
            {
                CreatePipe(group.transform, "OverheadPipe", new Vector3(c, 3.25f - p * 0.24f, 2.85f), new Vector3(0.07f, 5.8f, 0.07f), new Vector3(0f, 0f, 90f), p == 1 ? amber : steel);
            }
        }

        private static void CreateLiving(Transform root, Material living, Material mattress, Material frame, Material steel, Material amber)
        {
            float c = Centers[2];
            GameObject group = NewGroup(root, "Living_Interior");

            CreateBunkModule(group.transform, new Vector3(c - 4.9f, 0.48f, -2.72f), mattress, frame);
            CreateBunkModule(group.transform, new Vector3(c - 1.8f, 0.48f, -2.72f), mattress, frame);
            CreateBunkModule(group.transform, new Vector3(c + 1.8f, 0.48f, 2.72f), mattress, frame);
            CreateBunkModule(group.transform, new Vector3(c + 4.9f, 0.48f, 2.72f), mattress, frame);

            CreateCube(group.transform, "GalleyCounter", new Vector3(c + 4.65f, 0.82f, -2.65f), new Vector3(4.1f, 1.42f, 0.92f), living);
            CreateCube(group.transform, "GalleyTop", new Vector3(c + 4.65f, 1.55f, -2.65f), new Vector3(4.2f, 0.10f, 0.98f), steel);
            CreateCube(group.transform, "DiningTable", new Vector3(c - 4.15f, 0.78f, 1.25f), new Vector3(2.35f, 0.12f, 1.55f), steel);
            CreateBench(group.transform, new Vector3(c - 4.15f, 0.48f, 2.10f), living, frame);
            CreateBench(group.transform, new Vector3(c - 4.15f, 0.48f, 0.40f), living, frame);
            CreateCube(group.transform, "WallHeater", new Vector3(c + 0.6f, 0.88f, -3.18f), new Vector3(1.7f, 1.25f, 0.38f), amber);
        }

        private static void CreateWorkshop(Transform root, Material workshop, Material frame, Material steel, Material screen, Material amber)
        {
            float c = Centers[3];
            GameObject group = NewGroup(root, "Workshop_Interior");

            CreateWorkbench(group.transform, new Vector3(c - 4.35f, 0.80f, -2.78f), 4.5f, workshop, frame, steel);
            CreateWorkbench(group.transform, new Vector3(c + 2.15f, 0.80f, -2.78f), 5.4f, workshop, frame, steel);
            CreateWorkbench(group.transform, new Vector3(c - 2.0f, 0.80f, 2.78f), 6.2f, workshop, frame, steel);

            CreateCabinet(group.transform, new Vector3(c + 5.6f, 1.25f, 2.95f), new Vector3(2.2f, 2.35f, 0.72f), frame, steel);
            CreateCube(group.transform, "MachineStation", new Vector3(c + 4.9f, 0.86f, 1.45f), new Vector3(2.0f, 1.55f, 1.35f), frame);
            CreateCube(group.transform, "MachineScreen", new Vector3(c + 4.9f, 1.28f, 0.75f), new Vector3(1.15f, 0.48f, 0.04f), screen);
            CreateCube(group.transform, "OverheadToolRail", new Vector3(c, 3.12f, -2.60f), new Vector3(11.0f, 0.12f, 0.20f), steel);
        }

        private static void CreateGreenhouse(Transform root, Material green, Material plant, Material frame, Material steel, Material amber)
        {
            float c = Centers[4];
            GameObject group = NewGroup(root, "Greenhouse_Interior");

            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * 2.35f;
                for (int i = -2; i <= 2; i++)
                {
                    float x = c + i * 2.35f;
                    CreateCube(group.transform, "HydroponicBed", new Vector3(x, 0.62f, z), new Vector3(1.85f, 0.82f, 1.35f), frame);
                    CreateCube(group.transform, "PlantBed", new Vector3(x, 1.08f, z), new Vector3(1.62f, 0.28f, 1.12f), green);
                    for (int leaf = -2; leaf <= 2; leaf++)
                    {
                        CreateCube(group.transform, "Plant", new Vector3(x + leaf * 0.28f, 1.34f, z), new Vector3(0.18f, 0.42f, 0.38f), plant);
                    }
                    CreateCube(group.transform, "GrowLight", new Vector3(x, 2.85f, z), new Vector3(1.75f, 0.08f, 0.20f), amber);
                }
            }

            CreateCube(group.transform, "WaterTank", new Vector3(c + 5.9f, 1.15f, 0f), new Vector3(1.55f, 2.05f, 2.05f), steel);
            CreateCube(group.transform, "PumpUnit", new Vector3(c + 4.3f, 0.72f, 0f), new Vector3(1.15f, 1.15f, 1.15f), frame);
            CreatePipe(group.transform, "IrrigationLeft", new Vector3(c, 2.55f, -3.05f), new Vector3(0.05f, 6.0f, 0.05f), new Vector3(0f, 0f, 90f), steel);
            CreatePipe(group.transform, "IrrigationRight", new Vector3(c, 2.55f, 3.05f), new Vector3(0.05f, 6.0f, 0.05f), new Vector3(0f, 0f, 90f), steel);
        }

        private static void CreateInteriorLighting(Transform root, Material frame, Material amber)
        {
            for (int index = 0; index < Centers.Length; index++)
            {
                float center = Centers[index];
                for (int i = -1; i <= 1; i++)
                {
                    float x = center + i * 4.5f;
                    CreateCube(root, "CeilingLightHousing", new Vector3(x, 3.65f, 0f), new Vector3(3.1f, 0.08f, 0.30f), frame);
                    CreateCube(root, "CeilingLightStrip", new Vector3(x, 3.60f, 0f), new Vector3(2.7f, 0.035f, 0.18f), amber);
                }

                GameObject lightObject = new GameObject("InteriorLight");
                lightObject.transform.SetParent(root);
                lightObject.transform.position = new Vector3(center, 3.18f, 0f);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 9.5f;
                light.intensity = index == 1 ? 1.55f : 1.35f;
                light.color = index switch
                {
                    0 => new Color(0.82f, 0.90f, 1f),
                    1 => new Color(1f, 0.58f, 0.30f),
                    2 => new Color(1f, 0.76f, 0.52f),
                    3 => new Color(0.94f, 0.80f, 0.62f),
                    _ => new Color(0.72f, 0.95f, 0.72f)
                };
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

        private static void CreateBunkModule(Transform parent, Vector3 position, Material mattress, Material frame)
        {
            CreateCube(parent, "LowerBunkFrame", position, new Vector3(2.55f, 0.16f, 1.05f), frame);
            CreateCube(parent, "LowerMattress", position + new Vector3(0f, 0.14f, 0f), new Vector3(2.38f, 0.15f, 0.90f), mattress);
            CreateCube(parent, "UpperBunkFrame", position + new Vector3(0f, 1.18f, 0f), new Vector3(2.55f, 0.16f, 1.05f), frame);
            CreateCube(parent, "UpperMattress", position + new Vector3(0f, 1.32f, 0f), new Vector3(2.38f, 0.15f, 0.90f), mattress);
            CreateCube(parent, "BunkPostA", position + new Vector3(-1.16f, 0.66f, 0f), new Vector3(0.08f, 1.45f, 0.08f), frame);
            CreateCube(parent, "BunkPostB", position + new Vector3(1.16f, 0.66f, 0f), new Vector3(0.08f, 1.45f, 0.08f), frame);
        }

        private static void CreateBench(Transform parent, Vector3 position, Material cushion, Material frame)
        {
            CreateCube(parent, "BenchBase", position, new Vector3(2.25f, 0.58f, 0.72f), frame);
            CreateCube(parent, "BenchSeat", position + new Vector3(0f, 0.36f, 0f), new Vector3(2.10f, 0.16f, 0.68f), cushion);
            CreateCube(parent, "BenchBack", position + new Vector3(0f, 0.82f, 0.30f), new Vector3(2.10f, 0.78f, 0.14f), cushion);
        }

        private static void CreateSeat(Transform parent, Vector3 position, Material frame, Material cushion)
        {
            CreateCube(parent, "SeatBase", position, new Vector3(0.72f, 0.62f, 0.72f), frame);
            CreateCube(parent, "SeatCushion", position + new Vector3(0f, 0.36f, 0f), new Vector3(0.66f, 0.14f, 0.66f), cushion);
            CreateCube(parent, "SeatBack", position + new Vector3(0.32f, 0.78f, 0f), new Vector3(0.14f, 0.82f, 0.66f), cushion);
        }

        private static void CreateWorkbench(Transform parent, Vector3 position, float length, Material accent, Material frame, Material steel)
        {
            CreateCube(parent, "WorkbenchBase", position, new Vector3(length, 1.35f, 0.92f), frame);
            CreateCube(parent, "WorkbenchTop", position + new Vector3(0f, 0.72f, 0f), new Vector3(length + 0.08f, 0.12f, 1.00f), steel);
            CreateCube(parent, "BackBoard", position + new Vector3(0f, 1.38f, -0.43f), new Vector3(length, 1.05f, 0.10f), accent);
        }

        private static void CreateCabinet(Transform parent, Vector3 position, Vector3 size, Material body, Material trim)
        {
            CreateCube(parent, "CabinetBody", position, size, body);
            CreateCube(parent, "CabinetDoor", position + new Vector3(0f, 0f, -size.z * 0.51f), new Vector3(size.x * 0.88f, size.y * 0.86f, 0.04f), trim);
        }

        private static void CreatePipe(Transform parent, string name, Vector3 position, Vector3 scale, Vector3 rotation, Material material)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = name;
            pipe.transform.SetParent(parent);
            pipe.transform.localPosition = position;
            pipe.transform.localEulerAngles = rotation;
            pipe.transform.localScale = scale;
            pipe.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static GameObject NewGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent);
            return group;
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
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
