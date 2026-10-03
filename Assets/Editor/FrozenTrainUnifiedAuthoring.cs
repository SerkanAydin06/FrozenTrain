#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// The single public Frozen Train authoring command.
    /// Builds/refreshes the fixed five-car train as authored prefabs, then applies the
    /// multi-deck architecture and scene-level atmosphere fixes.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const string AtmosphereMaterialPath = "Assets/Materials/Atmosphere/Train_SnowParticles.mat";

        private const float CarLength = 16.5f;
        private const float CarWidth = 8.4f;
        private const float SideZ = CarWidth * 0.5f;
        private const float WallThickness = 0.20f;
        private const float BasementFloorY = -2.15f;
        private const float MainFloorY = 0f;
        private const float UpperFloorY = 3.10f;
        private const float RoofY = 6.20f;
        private const float MainWindowBottom = 1.25f;
        private const float MainWindowTop = 2.45f;

        private static readonly string[] CarNames =
        {
            "01_Locomotive",
            "02_Generator",
            "03_Living",
            "04_Workshop",
            "05_Greenhouse"
        };

        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            EnsureBaseAuthoredAssets();
            FrozenTrainFiveCarDesignAuthoring.ApplyAll();
            ApplyMultiDeckArchitecture();
            RebuildMultiDeckConnector();
            FrozenTrainPhysicalSystemsAuthoring.InstallOrRefresh();
            RefreshSceneAtmosphere();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: corrected travel direction, exterior-only snow, stable interactions, and three-level five-car architecture.");
        }

        private static void EnsureBaseAuthoredAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TrainPrefabFolder + "/01_Locomotive.prefab") == null)
            {
                FrozenTrainScenePrefabMigration.ConvertToScenePrefabs();
            }

            FrozenTrainAuthoredLayoutResize.ResizeAuthoredTrain();
        }

        private enum MainSideProfile
        {
            Sealed,
            LocomotiveCab,
            LivingSocial
        }

        private static void ApplyMultiDeckArchitecture()
        {
            for (int index = 0; index < CarNames.Length; index++)
            {
                string path = TrainPrefabFolder + "/" + CarNames[index] + ".prefab";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    Debug.LogWarning("FrozenTrain: missing car prefab " + path);
                    continue;
                }

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Material floor = Mat("Train_Floor");
                    Material wall = Mat("Train_Wall");
                    Material frame = Mat("Train_Frame");
                    Material steel = Mat("Train_Steel");
                    Material living = Mat("Train_Living");
                    Material mattress = Mat("Train_Mattress");
                    Material amber = Mat("Train_Amber");
                    Material glass = Mat("Train_WindowGlass");
                    Material screen = Mat("Train_Screen");

                    MainSideProfile profile = index == 0
                        ? MainSideProfile.LocomotiveCab
                        : index == 2 ? MainSideProfile.LivingSocial : MainSideProfile.Sealed;

                    RebuildThreeLevelShell(root, index, profile, floor, wall, frame, steel, glass);
                    RebuildLevelLighting(root, index, frame, amber);
                    BuildUpperSleepingDeck(root, index, frame, steel, living, mattress);
                    BuildBasementDeck(root, index, frame, steel, amber, screen);

                    if (index == 2)
                    {
                        RebuildLivingMainDeck(root, living, frame, steel, amber);
                        BuildLivingVerticalAccess(root, frame, steel);
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void RebuildThreeLevelShell(
            GameObject root,
            int carIndex,
            MainSideProfile profile,
            Material floor,
            Material wall,
            Material frame,
            Material steel,
            Material glass)
        {
            DestroyDirect(root.transform,
                "ArchitecturalSideLeft", "ArchitecturalSideRight", "MultiDeckArchitecture",
                "Ceiling", "CeilingRib", "BaseTrimLeft", "BaseTrimRight", "CeilingDuctLeft", "CeilingDuctRight");

            Transform architecture = Group(root.transform, "MultiDeckArchitecture");

            // The existing Floor at Y=0 becomes the structural slab between basement and main deck.
            Transform floorObject = FindDirect(root.transform, "Floor");
            if (floorObject != null)
            {
                floorObject.localPosition = new Vector3(0f, MainFloorY, 0f);
                floorObject.localScale = new Vector3(CarLength, 0.22f, CarWidth);
            }

            Cube(architecture, "BasementFloor", new Vector3(0f, BasementFloorY, 0f), new Vector3(CarLength, 0.20f, CarWidth), floor);
            Cube(architecture, "UpperDeckFloor", new Vector3(0f, UpperFloorY, 0f), new Vector3(CarLength, 0.18f, CarWidth), floor);
            Cube(architecture, "Roof", new Vector3(0f, RoofY, 0f), new Vector3(CarLength, 0.22f, CarWidth), wall);

            BuildMainSide(architecture, -SideZ, profile, wall, frame, glass);
            BuildMainSide(architecture, SideZ, profile, wall, frame, glass);

            float upperHeight = RoofY - UpperFloorY;
            float upperCenter = UpperFloorY + upperHeight * 0.5f;
            Cube(architecture, "UpperSideLeft", new Vector3(0f, upperCenter, -SideZ), new Vector3(CarLength, upperHeight, WallThickness), wall);
            Cube(architecture, "UpperSideRight", new Vector3(0f, upperCenter, SideZ), new Vector3(CarLength, upperHeight, WallThickness), wall);

            float basementHeight = MainFloorY - BasementFloorY;
            float basementCenter = BasementFloorY + basementHeight * 0.5f;
            Cube(architecture, "BasementSideLeft", new Vector3(0f, basementCenter, -SideZ), new Vector3(CarLength, basementHeight, WallThickness), wall);
            Cube(architecture, "BasementSideRight", new Vector3(0f, basementCenter, SideZ), new Vector3(CarLength, basementHeight, WallThickness), wall);

            for (float x = -6.4f; x <= 6.4f; x += 3.2f)
            {
                Cube(architecture, "UpperWallRibL", new Vector3(x, upperCenter, -SideZ + 0.05f), new Vector3(0.10f, upperHeight - 0.18f, 0.10f), frame);
                Cube(architecture, "UpperWallRibR", new Vector3(x, upperCenter, SideZ - 0.05f), new Vector3(0.10f, upperHeight - 0.18f, 0.10f), frame);
                Cube(architecture, "BasementWallRibL", new Vector3(x, basementCenter, -SideZ + 0.05f), new Vector3(0.09f, basementHeight - 0.15f, 0.09f), steel);
                Cube(architecture, "BasementWallRibR", new Vector3(x, basementCenter, SideZ - 0.05f), new Vector3(0.09f, basementHeight - 0.15f, 0.09f), steel);
            }

            BuildUpperAndBasementEnds(architecture, carIndex, wall, frame);
        }

        private static void BuildMainSide(Transform parent, float z, MainSideProfile profile, Material wall, Material frame, Material glass)
        {
            float deckTop = UpperFloorY - 0.09f;
            if (profile == MainSideProfile.Sealed)
            {
                Cube(parent, "MainSolidSide", new Vector3(0f, deckTop * 0.5f, z), new Vector3(CarLength, deckTop, WallThickness), wall);
                return;
            }

            float lowerHeight = MainWindowBottom;
            float upperHeight = deckTop - MainWindowTop;
            float bandHeight = MainWindowTop - MainWindowBottom;
            float bandCenter = (MainWindowTop + MainWindowBottom) * 0.5f;
            Cube(parent, "MainWallLower", new Vector3(0f, lowerHeight * 0.5f, z), new Vector3(CarLength, lowerHeight, WallThickness), wall);
            Cube(parent, "MainWallUpper", new Vector3(0f, MainWindowTop + upperHeight * 0.5f, z), new Vector3(CarLength, upperHeight, WallThickness), wall);

            List<(float center, float width)> windows = profile == MainSideProfile.LocomotiveCab
                ? new List<(float, float)> { (-5.9f, 2.10f), (-3.25f, 2.10f) }
                : new List<(float, float)> { (-3.10f, 2.25f), (0f, 2.25f), (3.10f, 2.25f) };

            float left = -CarLength * 0.5f;
            foreach ((float center, float width) in windows.OrderBy(w => w.center))
            {
                float openingLeft = center - width * 0.5f;
                float fill = openingLeft - left;
                if (fill > 0.01f)
                {
                    Cube(parent, "MainSolidBand", new Vector3(left + fill * 0.5f, bandCenter, z), new Vector3(fill, bandHeight, WallThickness), wall);
                }

                CreateWindow(parent, center, width, z, frame, glass);
                left = center + width * 0.5f;
            }

            float remaining = CarLength * 0.5f - left;
            if (remaining > 0.01f)
            {
                Cube(parent, "MainSolidBand", new Vector3(left + remaining * 0.5f, bandCenter, z), new Vector3(remaining, bandHeight, WallThickness), wall);
            }
        }

        private static void CreateWindow(Transform parent, float x, float width, float z, Material frame, Material glass)
        {
            float height = MainWindowTop - MainWindowBottom;
            float y = (MainWindowTop + MainWindowBottom) * 0.5f;
            float inward = z > 0f ? -1f : 1f;
            Cube(parent, "WindowTopFrame", new Vector3(x, MainWindowTop, z), new Vector3(width + 0.16f, 0.10f, 0.14f), frame);
            Cube(parent, "WindowBottomFrame", new Vector3(x, MainWindowBottom, z), new Vector3(width + 0.16f, 0.10f, 0.14f), frame);
            Cube(parent, "WindowLeftFrame", new Vector3(x - width * 0.5f, y, z), new Vector3(0.10f, height, 0.14f), frame);
            Cube(parent, "WindowRightFrame", new Vector3(x + width * 0.5f, y, z), new Vector3(0.10f, height, 0.14f), frame);
            Cube(parent, "WindowGlass", new Vector3(x, y, z + inward * 0.025f), new Vector3(width - 0.12f, height - 0.12f, 0.035f), glass);
        }

        private static void BuildUpperAndBasementEnds(Transform parent, int carIndex, Material wall, Material frame)
        {
            bool first = carIndex == 0;
            bool last = carIndex == CarNames.Length - 1;
            float leftX = -CarLength * 0.5f;
            float rightX = CarLength * 0.5f;

            BuildLevelEnd(parent, leftX, UpperFloorY, RoofY, first, wall, frame, "UpperFront");
            BuildLevelEnd(parent, rightX, UpperFloorY, RoofY, last, wall, frame, "UpperRear");
            BuildLevelEnd(parent, leftX, BasementFloorY, MainFloorY, first, wall, frame, "BasementFront");
            BuildLevelEnd(parent, rightX, BasementFloorY, MainFloorY, last, wall, frame, "BasementRear");
        }

        private static void BuildLevelEnd(Transform parent, float x, float bottomY, float topY, bool sealedEnd, Material wall, Material frame, string name)
        {
            float height = topY - bottomY;
            float centerY = bottomY + height * 0.5f;
            if (sealedEnd)
            {
                Cube(parent, name + "Solid", new Vector3(x, centerY, 0f), new Vector3(WallThickness, height, CarWidth), wall);
                return;
            }

            float doorWidth = bottomY < 0f ? 1.55f : 1.80f;
            float doorHeight = Mathf.Min(height - 0.16f, bottomY < 0f ? 1.85f : 2.35f);
            float sideWidth = (CarWidth - doorWidth) * 0.5f;
            float sideCenter = doorWidth * 0.5f + sideWidth * 0.5f;
            Cube(parent, name + "SideL", new Vector3(x, centerY, -sideCenter), new Vector3(WallThickness, height, sideWidth), wall);
            Cube(parent, name + "SideR", new Vector3(x, centerY, sideCenter), new Vector3(WallThickness, height, sideWidth), wall);
            float lintelHeight = height - doorHeight;
            Cube(parent, name + "Lintel", new Vector3(x, bottomY + doorHeight + lintelHeight * 0.5f, 0f), new Vector3(WallThickness, lintelHeight, doorWidth), frame);
        }

        private static void RebuildLivingMainDeck(GameObject root, Material living, Material frame, Material steel, Material amber)
        {
            DestroyDeepNamed(root.transform, "Living_Interior");
            Transform g = Group(root.transform, "Living_Interior");

            // Four-seat train bays: paired double seats face each other along the train axis.
            FourSeatBay(g, new Vector3(-2.90f, 0f, -2.70f), living, frame, steel);
            FourSeatBay(g, new Vector3(1.90f, 0f, -2.70f), living, frame, steel);
            FourSeatBay(g, new Vector3(-2.90f, 0f, 2.70f), living, frame, steel);
            FourSeatBay(g, new Vector3(1.90f, 0f, 2.70f), living, frame, steel);

            // Tall service furniture stays in solid end zones, never in front of windows.
            Cube(g, "GalleyCounter", new Vector3(6.25f, 0.80f, -2.85f), new Vector3(2.45f, 1.38f, 0.92f), living);
            Cube(g, "GalleyTop", new Vector3(6.25f, 1.52f, -2.85f), new Vector3(2.52f, 0.10f, 0.98f), steel);
            Cabinet(g, "MedicalCabinet", new Vector3(6.25f, 1.12f, 2.95f), new Vector3(1.85f, 2.00f, 0.65f), frame, steel, true);
            Cube(g, "WallHeater", new Vector3(-6.55f, 0.82f, -3.70f), new Vector3(1.45f, 1.12f, 0.34f), amber);
        }

        private static void FourSeatBay(Transform parent, Vector3 center, Material cushion, Material frame, Material steel)
        {
            DoubleTrainSeat(parent, center + new Vector3(-1.05f, 0.46f, 0f), true, cushion, frame);
            DoubleTrainSeat(parent, center + new Vector3(1.05f, 0.46f, 0f), false, cushion, frame);
            Cube(parent, "DiningTable", center + new Vector3(0f, 0.78f, 0f), new Vector3(1.10f, 0.10f, 1.45f), steel);
            Cube(parent, "TableLeg", center + new Vector3(0f, 0.40f, 0f), new Vector3(0.14f, 0.70f, 0.14f), frame);
        }

        private static void DoubleTrainSeat(Transform parent, Vector3 position, bool facePositiveX, Material cushion, Material frame)
        {
            Transform seat = Group(parent, "DoubleTrainSeat");
            seat.localPosition = position;
            seat.localRotation = facePositiveX ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
            Cube(seat, "SeatBase", Vector3.zero, new Vector3(0.72f, 0.58f, 1.72f), frame);
            Cube(seat, "SeatCushion", new Vector3(0f, 0.34f, 0f), new Vector3(0.68f, 0.14f, 1.62f), cushion);
            Cube(seat, "SeatBack", new Vector3(-0.31f, 0.78f, 0f), new Vector3(0.14f, 0.86f, 1.62f), cushion);
        }

        private static void BuildLivingVerticalAccess(GameObject root, Material frame, Material steel)
        {
            Transform old = FindDirect(root.transform, "VerticalAccess");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform g = Group(root.transform, "VerticalAccess");

            BuildStairFlight(g, new Vector3(-6.35f, 0.10f, 1.15f), 14, 0.27f, (UpperFloorY - 0.16f) / 14f, true, frame, steel);
            BuildStairFlight(g, new Vector3(-6.35f, -0.16f, -1.15f), 10, 0.27f, (BasementFloorY + 0.18f) / 10f, true, frame, steel);

            Cube(g, "UpperGuardRail", new Vector3(-4.45f, UpperFloorY + 0.55f, 1.92f), new Vector3(4.2f, 1.05f, 0.08f), frame);
            Cube(g, "BasementGuardRail", new Vector3(-5.05f, 0.52f, -1.92f), new Vector3(3.0f, 0.95f, 0.08f), frame);
        }

        private static void BuildStairFlight(Transform parent, Vector3 start, int stepCount, float runPerStep, float risePerStep, bool alongPositiveX, Material frame, Material tread)
        {
            for (int i = 0; i < stepCount; i++)
            {
                float x = (i + 0.5f) * runPerStep * (alongPositiveX ? 1f : -1f);
                float y = (i + 0.5f) * risePerStep;
                Cube(parent, "StairStep", start + new Vector3(x, y, 0f), new Vector3(runPerStep + 0.02f, Mathf.Abs(risePerStep) + 0.04f, 1.38f), i % 2 == 0 ? frame : tread);
            }
        }

        private static void BuildUpperSleepingDeck(GameObject root, int carIndex, Material frame, Material steel, Material living, Material mattress)
        {
            Transform old = FindDirect(root.transform, "UpperSleepingDeck");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform g = Group(root.transform, "UpperSleepingDeck");

            float baseY = UpperFloorY + 0.42f;
            float[] xPositions = carIndex == 2
                ? new[] { -5.55f, -2.00f, 2.00f, 5.55f }
                : new[] { -4.60f, 0f, 4.60f };

            foreach (float x in xPositions)
            {
                UpperBunkPod(g, new Vector3(x, baseY, -2.95f), mattress, frame);
                UpperBunkPod(g, new Vector3(x, baseY, 2.95f), mattress, frame);
            }

            Cabinet(g, "UpperLockerLeft", new Vector3(-6.95f, UpperFloorY + 1.18f, -3.45f), new Vector3(1.10f, 2.10f, 0.62f), frame, steel, true);
            Cabinet(g, "UpperLockerRight", new Vector3(6.95f, UpperFloorY + 1.18f, 3.45f), new Vector3(1.10f, 2.10f, 0.62f), frame, steel, false);

            Cube(g, "UpperCenterGuide", new Vector3(0f, UpperFloorY + 0.04f, 0f), new Vector3(CarLength - 2.1f, 0.035f, 1.55f), living);
        }

        private static void UpperBunkPod(Transform parent, Vector3 position, Material mattress, Material frame)
        {
            Cube(parent, "LowerBunkFrame", position, new Vector3(2.35f, 0.14f, 0.92f), frame);
            Cube(parent, "LowerMattress", position + new Vector3(0f, 0.13f, 0f), new Vector3(2.18f, 0.13f, 0.80f), mattress);
            Cube(parent, "UpperBunkFrame", position + new Vector3(0f, 1.05f, 0f), new Vector3(2.35f, 0.14f, 0.92f), frame);
            Cube(parent, "UpperMattress", position + new Vector3(0f, 1.18f, 0f), new Vector3(2.18f, 0.13f, 0.80f), mattress);
            Cube(parent, "BunkPostA", position + new Vector3(-1.04f, 0.58f, 0f), new Vector3(0.07f, 1.25f, 0.07f), frame);
            Cube(parent, "BunkPostB", position + new Vector3(1.04f, 0.58f, 0f), new Vector3(0.07f, 1.25f, 0.07f), frame);
        }

        private static void BuildBasementDeck(GameObject root, int carIndex, Material frame, Material steel, Material amber, Material screen)
        {
            Transform old = FindDirect(root.transform, "BasementInfrastructure");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform g = Group(root.transform, "BasementInfrastructure");

            // Continuous service corridor remains clear through the center.
            Pipe(g, "UtilityPipeLeft", new Vector3(0f, -0.42f, -3.25f), new Vector3(0.06f, 7.2f, 0.06f), new Vector3(0f, 0f, 90f), steel);
            Pipe(g, "UtilityPipeRight", new Vector3(0f, -0.42f, 3.25f), new Vector3(0.06f, 7.2f, 0.06f), new Vector3(0f, 0f, 90f), steel);
            Cube(g, "CableTrayLeft", new Vector3(0f, -0.72f, -3.60f), new Vector3(CarLength - 1.4f, 0.15f, 0.32f), frame);
            Cube(g, "CableTrayRight", new Vector3(0f, -0.72f, 3.60f), new Vector3(CarLength - 1.4f, 0.15f, 0.32f), frame);

            switch (carIndex)
            {
                case 0:
                    Cabinet(g, "TractionBatteryBank", new Vector3(1.0f, -1.20f, -3.15f), new Vector3(6.0f, 1.35f, 1.10f), frame, steel, true);
                    Cabinet(g, "DriveElectronics", new Vector3(2.5f, -1.20f, 3.15f), new Vector3(4.8f, 1.35f, 1.10f), frame, screen, false);
                    break;
                case 1:
                    Cube(g, "FuelTankA", new Vector3(-3.6f, -1.18f, -2.95f), new Vector3(4.1f, 1.45f, 1.35f), steel);
                    Cube(g, "FuelTankB", new Vector3(2.0f, -1.18f, -2.95f), new Vector3(4.1f, 1.45f, 1.35f), steel);
                    Cabinet(g, "GeneratorManifold", new Vector3(2.8f, -1.18f, 3.10f), new Vector3(4.2f, 1.35f, 1.05f), frame, amber, false);
                    break;
                case 2:
                    Cube(g, "FreshWaterTank", new Vector3(-3.8f, -1.18f, -3.05f), new Vector3(4.2f, 1.45f, 1.30f), steel);
                    Cube(g, "WasteWaterTank", new Vector3(2.0f, -1.18f, -3.05f), new Vector3(4.2f, 1.45f, 1.30f), frame);
                    Cabinet(g, "HVACPlant", new Vector3(2.8f, -1.18f, 3.10f), new Vector3(4.2f, 1.35f, 1.05f), frame, steel, false);
                    break;
                case 3:
                    Cube(g, "Compressor", new Vector3(-3.2f, -1.18f, -3.10f), new Vector3(3.2f, 1.35f, 1.15f), frame);
                    Cabinet(g, "PartsStorage", new Vector3(2.6f, -1.18f, -3.10f), new Vector3(5.0f, 1.35f, 1.05f), frame, steel, true);
                    Cabinet(g, "ToolPowerBus", new Vector3(2.8f, -1.18f, 3.10f), new Vector3(4.0f, 1.35f, 1.05f), frame, amber, false);
                    break;
                default:
                    Cube(g, "NutrientTank", new Vector3(-3.6f, -1.18f, -3.05f), new Vector3(4.0f, 1.45f, 1.30f), steel);
                    Cube(g, "IrrigationReserve", new Vector3(1.9f, -1.18f, -3.05f), new Vector3(4.0f, 1.45f, 1.30f), steel);
                    Cabinet(g, "ClimateServiceRack", new Vector3(2.8f, -1.18f, 3.10f), new Vector3(4.0f, 1.35f, 1.05f), frame, screen, false);
                    break;
            }
        }

        private static void RebuildLevelLighting(GameObject root, int carIndex, Material frame, Material amber)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true).ToArray())
            {
                if (child == root.transform) continue;
                if (child.name == "MultiDeckLevelLighting") Object.DestroyImmediate(child.gameObject);
            }

            Transform g = Group(root.transform, "MultiDeckLevelLighting");
            CreateLevelLight(g, "MainDeckLight", new Vector3(0f, 2.62f, 0f), carIndex == 1 ? 1.35f : 1.15f, carIndex == 4 ? new Color(0.72f, 0.95f, 0.72f) : new Color(0.90f, 0.80f, 0.66f));
            CreateLevelLight(g, "UpperDeckLight", new Vector3(0f, 5.35f, 0f), 0.85f, new Color(0.86f, 0.80f, 0.70f));
            CreateLevelLight(g, "BasementLight", new Vector3(0f, -0.85f, 0f), 0.70f, new Color(0.72f, 0.82f, 0.90f));

            for (int level = 0; level < 3; level++)
            {
                float y = level == 0 ? 2.90f : level == 1 ? 5.85f : -0.28f;
                for (int i = -1; i <= 1; i++)
                {
                    Cube(g, "LightHousing", new Vector3(i * 4.5f, y, 0f), new Vector3(2.6f, 0.07f, 0.28f), frame);
                    Cube(g, "LightStrip", new Vector3(i * 4.5f, y - 0.045f, 0f), new Vector3(2.25f, 0.03f, 0.16f), amber);
                }
            }
        }

        private static void CreateLevelLight(Transform parent, string name, Vector3 position, float intensity, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8.0f;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;
        }

        private static void RebuildMultiDeckConnector()
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath);
            if (asset == null) return;

            Material floor = Mat("Train_Floor");
            Material frame = Mat("Train_Frame");
            Material rubber = Mat("Train_Rubber");

            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Transform old = FindDirect(root.transform, "MultiDeckConnector");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                Transform g = Group(root.transform, "MultiDeckConnector");

                Cube(g, "UpperConnectorFloor", new Vector3(0f, UpperFloorY, 0f), new Vector3(0.85f, 0.18f, 2.0f), floor);
                Cube(g, "UpperConnectorRoof", new Vector3(0f, RoofY - 0.08f, 0f), new Vector3(0.85f, 0.14f, 2.0f), frame);
                Cube(g, "BasementConnectorFloor", new Vector3(0f, BasementFloorY, 0f), new Vector3(0.85f, 0.18f, 1.75f), floor);

                float fullCenter = (RoofY + BasementFloorY) * 0.5f;
                float fullHeight = RoofY - BasementFloorY;
                Cube(g, "BellowsMultiLeft", new Vector3(0f, fullCenter, -1.05f), new Vector3(0.85f, fullHeight, 0.12f), rubber);
                Cube(g, "BellowsMultiRight", new Vector3(0f, fullCenter, 1.05f), new Vector3(0.85f, fullHeight, 0.12f), rubber);

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RefreshSceneAtmosphere()
        {
            if (!System.IO.File.Exists(ScenePath)) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject atmosphere = GameObject.Find("SCENE_ATMOSPHERE");
            if (atmosphere == null)
            {
                atmosphere = new GameObject("SCENE_ATMOSPHERE");
            }

            foreach (ParticleSystem particles in atmosphere.GetComponentsInChildren<ParticleSystem>(true))
            {
                Object.DestroyImmediate(particles.gameObject);
            }

            Material snowMaterial = AssetDatabase.LoadAssetAtPath<Material>(AtmosphereMaterialPath);
            if (snowMaterial != null)
            {
                CreateExteriorSnowStrip(atmosphere.transform, "ExteriorSnow_Left", -10.5f, snowMaterial);
                CreateExteriorSnowStrip(atmosphere.transform, "ExteriorSnow_Right", 10.5f, snowMaterial);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateExteriorSnowStrip(Transform parent, string name, float z, Material material)
        {
            GameObject snow = new GameObject(name);
            snow.transform.SetParent(parent, false);
            snow.transform.position = new Vector3(0f, 5.2f, z);

            ParticleSystem ps = snow.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 5.2f;
            main.startSpeed = 2.0f;
            main.startSize = 0.075f;
            main.maxParticles = 520;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.035f;

            var emission = ps.emission;
            emission.rateOverTime = 55f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(120f, 11f, 4.0f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = 10f;
            velocity.y = -1.25f;
            velocity.z = z < 0f ? -0.12f : 0.12f;

            ParticleSystemRenderer renderer = snow.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Material Mat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/" + name + ".mat");
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
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

        private static void Cabinet(Transform parent, string name, Vector3 position, Vector3 size, Material body, Material trim, bool doorFacesPositiveZ)
        {
            Transform g = Group(parent, name);
            g.localPosition = position;
            Cube(g, "CabinetBody", Vector3.zero, size, body);
            float doorZ = (doorFacesPositiveZ ? 1f : -1f) * size.z * 0.51f;
            Cube(g, "CabinetDoor", new Vector3(0f, 0f, doorZ), new Vector3(size.x * 0.88f, size.y * 0.86f, 0.04f), trim);
        }

        private static void DestroyDirect(Transform root, params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (set.Contains(child.name)) Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void DestroyDeepNamed(Transform root, string name)
        {
            Transform found = FindDeep(root, name);
            if (found != null) Object.DestroyImmediate(found.gameObject);
        }

        private static Transform FindDirect(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name == name) return root.GetChild(i);
            return null;
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
