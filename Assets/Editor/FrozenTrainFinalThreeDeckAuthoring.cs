#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final authored train layout: five 18m x 9.2m cars, three full-size decks.
    /// Lower = technical plant, middle = occupied/work deck, upper = sleeping + through corridor.
    /// </summary>
    public static class FrozenTrainFinalThreeDeckAuthoring
    {
        public const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        public const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        public const float CarLength = 18f;
        public const float CarWidth = 9.2f;
        public const float CarSpacing = 18.8f;

        private const float HalfL = CarLength * 0.5f;
        private const float HalfW = CarWidth * 0.5f;
        private const float LowerY = -3.10f;
        private const float MainY = 0f;
        private const float UpperY = 3.15f;
        private const float RoofY = 6.30f;
        private const float WallT = 0.20f;
        private const float CorridorHalf = 1.05f;

        private static readonly string[] Cars =
        {
            "01_Locomotive", "02_Generator", "03_Living", "04_Workshop", "05_Greenhouse"
        };

        private static readonly float[] Centers =
        {
            -2f * CarSpacing, -CarSpacing, 0f, CarSpacing, 2f * CarSpacing
        };

        public static void ApplyAll()
        {
            for (int i = 0; i < Cars.Length; i++)
                ApplyCar("Assets/Prefabs/Train/" + Cars[i] + ".prefab", i);
            ApplyConnector();
            ApplyScene();
        }

        private static void ApplyCar(string path, int index)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Material floor = Mat("Train_Floor");
                Material wall = Mat("Train_Wall");
                Material frame = Mat("Train_Frame");
                Material steel = Mat("Train_Steel");
                Material rubber = Mat("Train_Rubber");
                Material glass = Mat("Train_WindowGlass");
                Material mattress = Mat("Train_Mattress");
                Material living = Mat("Train_Living");
                Material amber = Mat("Train_Amber");
                Material screen = Mat("Train_Screen");

                Cleanup(root.transform);
                MakeFloorMarker(root.transform);
                Transform g = Group(root.transform, "FinalThreeDeckV4");

                List<Hole> mainHoles = (index == 1 || index == 3)
                    ? new List<Hole> { new Hole(-8.65f, -7.05f, -4.15f, -2.55f) }
                    : new List<Hole>();
                float ladderX0 = index == Cars.Length - 1 ? -8.65f : 7.05f;
                float ladderX1 = index == Cars.Length - 1 ? -7.05f : 8.65f;
                List<Hole> upperHoles = new List<Hole>
                {
                    new Hole(ladderX0, ladderX1, -3.60f, -2.10f),
                    new Hole(ladderX0, ladderX1, 2.10f, 3.60f)
                };

                Cube(g, "LowerFloor", new Vector3(0f, LowerY, 0f), new Vector3(CarLength, .20f, CarWidth), floor);
                BuildFloor(g, "MainFloor", MainY, .22f, mainHoles, floor);
                BuildFloor(g, "UpperFloor", UpperY, .18f, upperHoles, floor);
                Cube(g, "Roof", new Vector3(0f, RoofY, 0f), new Vector3(CarLength, .22f, CarWidth), wall);

                BuildSideShell(g, index, wall, frame, glass);
                BuildEnds(g, index, wall, frame);
                BuildUpperSleeping(g, index, wall, frame, steel, living, mattress);
                BuildLowerTechnical(root.transform, g, index, frame, steel, rubber, amber, screen);
                BuildUpperLadders(g, index, frame, steel);
                if (index == 1 || index == 3) BuildTechnicalLadder(g, frame, steel);
                BuildLights(g, index, frame, amber);

                if (index == 0) BuildPanoramicFront(g, glass);
                if (index == 1) SimplifyGeneratorMain(root.transform, g, frame, steel, screen);
                if (index == 2) RebuildLivingMain(root.transform, g, living, frame, steel, amber);
                if (index == 4) RemoveMainGreenhouseUtilities(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BuildSideShell(Transform g, int index, Material wall, Material frame, Material glass)
        {
            float lowerH = MainY - LowerY;
            float upperH = RoofY - UpperY;
            Cube(g, "LowerSideL", new Vector3(0f, LowerY + lowerH / 2f, -HalfW), new Vector3(CarLength, lowerH, WallT), wall);
            Cube(g, "LowerSideR", new Vector3(0f, LowerY + lowerH / 2f, HalfW), new Vector3(CarLength, lowerH, WallT), wall);
            Cube(g, "UpperSideL", new Vector3(0f, UpperY + upperH / 2f, -HalfW), new Vector3(CarLength, upperH, WallT), wall);
            Cube(g, "UpperSideR", new Vector3(0f, UpperY + upperH / 2f, HalfW), new Vector3(CarLength, upperH, WallT), wall);

            BuildMainSide(g, -HalfW, index, wall, frame, glass);
            BuildMainSide(g, HalfW, index, wall, frame, glass);
        }

        private static void BuildMainSide(Transform g, float z, int index, Material wall, Material frame, Material glass)
        {
            float top = UpperY - .09f;
            if (index != 0 && index != 2)
            {
                Cube(g, "MainSolidSide", new Vector3(0f, top / 2f, z), new Vector3(CarLength, top, WallT), wall);
                return;
            }

            float wb = 1.20f, wt = 2.52f, wy = (wb + wt) / 2f, wh = wt - wb;
            Cube(g, "MainWallLower", new Vector3(0f, wb / 2f, z), new Vector3(CarLength, wb, WallT), wall);
            Cube(g, "MainWallUpper", new Vector3(0f, wt + (top - wt) / 2f, z), new Vector3(CarLength, top - wt, WallT), wall);

            List<(float c, float w)> windows = index == 0
                ? new List<(float, float)> { (-6.4f, 2.25f), (-3.55f, 2.25f) }
                : new List<(float, float)> { (-5.6f, 2.2f), (-2.0f, 2.2f), (2.0f, 2.2f), (5.6f, 2.2f) };

            float cursor = -HalfL;
            foreach (var win in windows)
            {
                float left = win.c - win.w / 2f;
                if (left > cursor)
                    Cube(g, "MainSolidBand", new Vector3((cursor + left) / 2f, wy, z), new Vector3(left - cursor, wh, WallT), wall);
                Window(g, win.c, win.w, z, wb, wt, frame, glass);
                cursor = win.c + win.w / 2f;
            }
            if (cursor < HalfL)
                Cube(g, "MainSolidBand", new Vector3((cursor + HalfL) / 2f, wy, z), new Vector3(HalfL - cursor, wh, WallT), wall);
        }

        private static void Window(Transform g, float x, float w, float z, float b, float t, Material frame, Material glass)
        {
            float y = (b + t) / 2f, h = t - b, inward = z > 0 ? -1f : 1f;
            Cube(g, "WindowTopFrame", new Vector3(x, t, z), new Vector3(w + .16f, .10f, .14f), frame);
            Cube(g, "WindowBottomFrame", new Vector3(x, b, z), new Vector3(w + .16f, .10f, .14f), frame);
            Cube(g, "WindowLeftFrame", new Vector3(x - w / 2f, y, z), new Vector3(.10f, h, .14f), frame);
            Cube(g, "WindowRightFrame", new Vector3(x + w / 2f, y, z), new Vector3(.10f, h, .14f), frame);
            Cube(g, "WindowGlass", new Vector3(x, y, z + inward * .025f), new Vector3(w - .12f, h - .12f, .035f), glass);
        }

        private static void BuildEnds(Transform g, int index, Material wall, Material frame)
        {
            bool first = index == 0, last = index == Cars.Length - 1;
            if (!first) EndOpening(g, -HalfL, MainY, UpperY, 2.15f, 2.60f, wall, frame, "MainFront");
            if (!last) EndOpening(g, HalfL, MainY, UpperY, 2.15f, 2.60f, wall, frame, "MainRear");
            if (first)
            {
                Cube(g, "CabFrontLower", new Vector3(-HalfL, .55f, 0f), new Vector3(WallT, 1.10f, CarWidth), wall);
                Cube(g, "CabFrontHeader", new Vector3(-HalfL, 2.93f, 0f), new Vector3(WallT, .36f, CarWidth), wall);
                Cube(g, "CabFrontEdgeL", new Vector3(-HalfL, 2.02f, -4.43f), new Vector3(WallT, 1.48f, .22f), frame);
                Cube(g, "CabFrontEdgeR", new Vector3(-HalfL, 2.02f, 4.43f), new Vector3(WallT, 1.48f, .22f), frame);
            }
            if (last) SolidEnd(g, HalfL, MainY, UpperY, wall, "MainRearSolid");

            if (first) SolidEnd(g, -HalfL, LowerY, MainY, wall, "LowerFrontSolid");
            else EndOpening(g, -HalfL, LowerY, MainY, 2.15f, 2.48f, wall, frame, "LowerFront");
            if (last) SolidEnd(g, HalfL, LowerY, MainY, wall, "LowerRearSolid");
            else EndOpening(g, HalfL, LowerY, MainY, 2.15f, 2.48f, wall, frame, "LowerRear");

            if (first) SolidEnd(g, -HalfL, UpperY, RoofY, wall, "UpperFrontSolid");
            else EndOpening(g, -HalfL, UpperY, RoofY, 2.20f, 2.55f, wall, frame, "UpperFront");
            if (last) SolidEnd(g, HalfL, UpperY, RoofY, wall, "UpperRearSolid");
            else EndOpening(g, HalfL, UpperY, RoofY, 2.20f, 2.55f, wall, frame, "UpperRear");
        }

        private static void SolidEnd(Transform g, float x, float bottom, float top, Material wall, string name)
        {
            Cube(g, name, new Vector3(x, (bottom + top) / 2f, 0f), new Vector3(WallT, top - bottom, CarWidth), wall);
        }

        private static void EndOpening(Transform g, float x, float bottom, float top, float width, float height, Material wall, Material frame, string prefix)
        {
            float h = top - bottom, cy = (bottom + top) / 2f, side = (CarWidth - width) / 2f, sc = width / 2f + side / 2f;
            Cube(g, prefix + "L", new Vector3(x, cy, -sc), new Vector3(WallT, h, side), wall);
            Cube(g, prefix + "R", new Vector3(x, cy, sc), new Vector3(WallT, h, side), wall);
            float lintel = Mathf.Max(.1f, h - height);
            Cube(g, prefix + "Lintel", new Vector3(x, bottom + height + lintel / 2f, 0f), new Vector3(WallT, lintel, width), frame);
        }

        private static void BuildUpperSleeping(Transform g, int index, Material wall, Material frame, Material steel, Material living, Material mattress)
        {
            Transform u = Group(g, "UpperSleepingDeckV4");
            float wy = UpperY + 1.48f, wh = 2.72f;
            List<float> doorXs = new List<float> { -6f, -2f, 2f, 6f };
            doorXs.Add(index == Cars.Length - 1 ? -7.45f : 7.45f);
            CorridorWall(u, -CorridorHalf, wy, wh, doorXs, wall);
            CorridorWall(u, CorridorHalf, wy, wh, doorXs, wall);

            foreach (float x in new[] { -4f, 0f, 4f })
            {
                Cube(u, "RoomDividerL", new Vector3(x, wy, -2.82f), new Vector3(.12f, wh, 3.50f), wall);
                Cube(u, "RoomDividerR", new Vector3(x, wy, 2.82f), new Vector3(.12f, wh, 3.50f), wall);
            }
            foreach (float x in new[] { -6.2f, -2.2f, 2.2f, 6.2f })
            {
                SleepPod(u, new Vector3(x, UpperY, -3.12f), frame, steel, living, mattress);
                SleepPod(u, new Vector3(x, UpperY, 3.12f), frame, steel, living, mattress);
            }
            Cube(u, "UpperCorridor", new Vector3(0f, UpperY + .035f, 0f), new Vector3(CarLength - .8f, .04f, 1.82f), living);
        }

        private static void CorridorWall(Transform p, float z, float y, float h, List<float> centers, Material wall)
        {
            const float doorW = .92f;
            List<(float min, float max)> holes = centers.Select(c => (min: c - doorW / 2f, max: c + doorW / 2f)).OrderBy(v => v.min).ToList();
            float cursor = -HalfL;
            foreach (var hole in holes)
            {
                if (hole.min > cursor)
                {
                    float w = hole.min - cursor;
                    Cube(p, "CorridorWall", new Vector3(cursor + w / 2f, y, z), new Vector3(w, h, .12f), wall);
                }
                cursor = Mathf.Max(cursor, hole.max);
            }
            if (cursor < HalfL)
            {
                float w = HalfL - cursor;
                Cube(p, "CorridorWall", new Vector3(cursor + w / 2f, y, z), new Vector3(w, h, .12f), wall);
            }
        }

        private static void SleepPod(Transform p, Vector3 b, Material frame, Material steel, Material living, Material mattress)
        {
            Cube(p, "BedFrame", b + new Vector3(0f, .42f, 0f), new Vector3(2.30f, .16f, .92f), frame);
            Cube(p, "Mattress", b + new Vector3(0f, .56f, 0f), new Vector3(2.14f, .14f, .80f), mattress);
            float lockerZ = b.z < 0 ? -4.08f : 4.08f;
            Cube(p, "Locker", new Vector3(b.x + 1.28f, UpperY + 1.02f, lockerZ), new Vector3(.62f, 1.80f, .54f), steel);
            Cube(p, "Shelf", b + new Vector3(-.75f, 1.42f, 0f), new Vector3(.74f, .10f, .34f), living);
        }

        private static void BuildLowerTechnical(Transform root, Transform g, int index, Material frame, Material steel, Material rubber, Material amber, Material screen)
        {
            Transform t = Group(g, "LowerTechnicalDeckV4");
            Pipe(t, "WaterMain", new Vector3(0f, -1.00f, -4.02f), new Vector3(.075f, 8.1f, .075f), steel);
            Pipe(t, "HeatingMain", new Vector3(0f, -1.38f, -4.02f), new Vector3(.060f, 8.1f, .060f), amber);
            Pipe(t, "ReturnWater", new Vector3(0f, -1.75f, -4.02f), new Vector3(.060f, 8.1f, .060f), rubber);
            Cube(t, "MainPowerBus", new Vector3(0f, -.72f, 4.02f), new Vector3(CarLength - 1f, .20f, .38f), frame);
            Cube(t, "DataCableTray", new Vector3(0f, -1.10f, 4.02f), new Vector3(CarLength - 1f, .14f, .32f), steel);
            Cube(t, "VentilationTrunk", new Vector3(0f, -.45f, 3.30f), new Vector3(CarLength - 1.4f, .46f, .62f), frame);

            if (index == 0)
            {
                Machine(t, "TractionMotorA", new Vector3(-4.8f, -1.63f, -2.75f), new Vector3(3.2f, 1.75f, 1.35f), steel, frame);
                Machine(t, "TractionMotorB", new Vector3(0f, -1.63f, -2.75f), new Vector3(3.2f, 1.75f, 1.35f), steel, frame);
                Cabinet(t, "TractionBatteryBank", new Vector3(4.8f, -1.62f, -2.9f), new Vector3(3.4f, 1.72f, 1.15f), frame, steel);
                Cabinet(t, "DriveInverter", new Vector3(4.5f, -1.62f, 2.95f), new Vector3(3.8f, 1.72f, 1.05f), frame, screen);
            }
            else if (index == 1)
            {
                Machine(t, "MainGeneratorEngine", new Vector3(-3.8f, -1.60f, -2.75f), new Vector3(5.2f, 1.85f, 1.55f), steel, frame);
                Machine(t, "Alternator", new Vector3(2f, -1.60f, -2.75f), new Vector3(3.8f, 1.85f, 1.55f), frame, steel);
                Cube(t, "FuelTankA", new Vector3(-3.8f, -1.66f, 3.05f), new Vector3(4.2f, 1.62f, 1.30f), steel);
                Cube(t, "FuelTankB", new Vector3(1.2f, -1.66f, 3.05f), new Vector3(4.2f, 1.62f, 1.30f), steel);
                Cabinet(t, "GeneratorManifold", new Vector3(5.7f, -1.62f, 3.10f), new Vector3(2f, 1.72f, 1.05f), frame, amber);
            }
            else if (index == 2)
            {
                Cube(t, "FreshWaterTank", new Vector3(-4.8f, -1.62f, -2.95f), new Vector3(4.2f, 1.70f, 1.35f), steel);
                Cube(t, "WasteWaterTank", new Vector3(0f, -1.62f, -2.95f), new Vector3(4.2f, 1.70f, 1.35f), frame);
                Machine(t, "HeatingBoiler", new Vector3(4.7f, -1.62f, -2.9f), new Vector3(3.2f, 1.70f, 1.25f), frame, amber);
                Cabinet(t, "HVACPlant", new Vector3(-3f, -1.62f, 3.05f), new Vector3(4f, 1.70f, 1.05f), frame, steel);
                Cabinet(t, "WaterTreatmentUnit", new Vector3(3f, -1.62f, 3.05f), new Vector3(4f, 1.70f, 1.05f), frame, screen);
            }
            else if (index == 3)
            {
                Machine(t, "Compressor", new Vector3(-4.6f, -1.62f, -2.95f), new Vector3(3.4f, 1.70f, 1.25f), frame, steel);
                Machine(t, "HydraulicUnit", new Vector3(0f, -1.62f, -2.95f), new Vector3(3.4f, 1.70f, 1.25f), steel, frame);
                Cabinet(t, "WorkshopPowerBus", new Vector3(4.8f, -1.62f, -3f), new Vector3(3.2f, 1.70f, 1.10f), frame, amber);
                Cabinet(t, "PartsStorage", new Vector3(3.2f, -1.62f, 3.05f), new Vector3(4.8f, 1.70f, 1.05f), frame, steel);
            }
            else
            {
                Remove(root, "WaterTank");
                Remove(root, "PumpUnit");
                Cube(t, "WaterTank", new Vector3(-4.8f, -1.62f, -2.95f), new Vector3(4.2f, 1.70f, 1.35f), steel);
                Cube(t, "NutrientTank", new Vector3(0f, -1.62f, -2.95f), new Vector3(4.2f, 1.70f, 1.35f), steel);
                Machine(t, "PumpUnit", new Vector3(4.8f, -1.62f, -2.95f), new Vector3(3f, 1.65f, 1.25f), frame, steel);
                Cabinet(t, "IrrigationManifold", new Vector3(-3.1f, -1.62f, 3.05f), new Vector3(4.2f, 1.70f, 1.05f), frame, screen);
                Cabinet(t, "ClimatePlant", new Vector3(3.1f, -1.62f, 3.05f), new Vector3(4.2f, 1.70f, 1.05f), frame, steel);
            }
        }

        private static void BuildUpperLadders(Transform g, int index, Material frame, Material steel)
        {
            bool front = index == Cars.Length - 1;
            float wallX = front ? -HalfL + .22f : HalfL - .22f;
            float exitX = front ? -HalfL + 1.20f : HalfL - 1.20f;
            Ladder(g, "UpperLadderLeft", wallX, -2.85f, MainY, UpperY,
                new Vector3(exitX, MainY + .12f, -2.85f), new Vector3(exitX, UpperY + .12f, -2.85f),
                "E - Üst kata çık", "E - Ana kata in", frame, steel);
            Ladder(g, "UpperLadderRight", wallX, 2.85f, MainY, UpperY,
                new Vector3(exitX, MainY + .12f, 2.85f), new Vector3(exitX, UpperY + .12f, 2.85f),
                "E - Üst kata çık", "E - Ana kata in", frame, steel);
        }

        private static void BuildTechnicalLadder(Transform g, Material frame, Material steel)
        {
            Ladder(g, "TechnicalLadder", -HalfL + .22f, -3.45f, LowerY, MainY,
                new Vector3(-HalfL + 1.20f, LowerY + .12f, -3.25f), new Vector3(-HalfL + 1.20f, MainY + .12f, -3.25f),
                "E - Teknik kata in", "E - Ana kata çık", frame, steel);
        }

        private static void Ladder(Transform p, string name, float x, float z, float lowY, float highY, Vector3 lowExit, Vector3 highExit, string upText, string downText, Material frame, Material steel)
        {
            Transform l = Group(p, name);
            l.localPosition = new Vector3(x, (lowY + highY) / 2f, z);
            float h = highY - lowY;
            Cube(l, "RailL", new Vector3(0f, 0f, -.43f), new Vector3(.08f, h, .08f), frame);
            Cube(l, "RailR", new Vector3(0f, 0f, .43f), new Vector3(.08f, h, .08f), frame);
            int count = Mathf.Max(8, Mathf.RoundToInt(h / .28f));
            for (int i = 0; i <= count; i++)
            {
                float y = Mathf.Lerp(-h / 2f, h / 2f, i / (float)count);
                Cube(l, "Rung", new Vector3(-.02f, y, 0f), new Vector3(.08f, .055f, .94f), steel);
            }
            GameObject hit = Cube(l, "LadderInteraction", Vector3.zero, new Vector3(.45f, h, 1.25f), frame);
            Object.DestroyImmediate(hit.GetComponent<Renderer>());
            Object.DestroyImmediate(hit.GetComponent<MeshFilter>());
            Transform a = Empty(l, "LowerExit", l.InverseTransformPoint(p.TransformPoint(lowExit)));
            Transform b = Empty(l, "UpperExit", l.InverseTransformPoint(p.TransformPoint(highExit)));
            DeckLadder dl = hit.AddComponent<DeckLadder>();
            dl.Configure(a, b, upText, downText);
        }

        private static void SimplifyGeneratorMain(Transform root, Transform g, Material frame, Material steel, Material screen)
        {
            Remove(root, "Generator_Interior");
            Transform c = Group(g, "GeneratorControlDeck");
            Cube(c, "OperatorDesk", new Vector3(-2.8f, .78f, -3.55f), new Vector3(4f, 1.35f, .90f), frame);
            Cube(c, "GeneratorConsole", new Vector3(-2.8f, 1.50f, -3.48f), new Vector3(2.2f, .72f, .18f), screen);
            Cube(c, "MonitoringRack", new Vector3(3.2f, 1.20f, 3.60f), new Vector3(4.4f, 2.10f, .72f), steel);
            Cube(c, "ControlTable", new Vector3(3f, .78f, -3.45f), new Vector3(3.3f, 1.30f, .90f), frame);
        }

        private static void RebuildLivingMain(Transform root, Transform g, Material living, Material frame, Material steel, Material amber)
        {
            Remove(root, "Living_Interior");
            Transform l = Group(g, "Living_Interior");
            Bay(l, new Vector3(-4.2f, 0f, -3.05f), living, frame, steel);
            Bay(l, new Vector3(.3f, 0f, -3.05f), living, frame, steel);
            Bay(l, new Vector3(-4.2f, 0f, 3.05f), living, frame, steel);
            Bay(l, new Vector3(.3f, 0f, 3.05f), living, frame, steel);
            Cube(l, "GalleyCounter", new Vector3(6.70f, .82f, -3.45f), new Vector3(2.65f, 1.40f, .92f), living);
            Cube(l, "GalleyTop", new Vector3(6.70f, 1.55f, -3.45f), new Vector3(2.72f, .10f, .98f), steel);
            Cabinet(l, "MedicalCabinet", new Vector3(6.75f, 1.15f, 3.52f), new Vector3(1.95f, 2.05f, .66f), frame, steel);
            Cube(l, "WallHeater", new Vector3(-7.35f, .84f, -4.08f), new Vector3(1.50f, 1.12f, .34f), amber);
        }

        private static void Bay(Transform p, Vector3 c, Material cushion, Material frame, Material steel)
        {
            Seat(p, c + new Vector3(-1.08f, .46f, 0f), true, cushion, frame);
            Seat(p, c + new Vector3(1.08f, .46f, 0f), false, cushion, frame);
            Cube(p, "DiningTable", c + new Vector3(0f, .78f, 0f), new Vector3(1.12f, .10f, 1.50f), steel);
            Cube(p, "TableLeg", c + new Vector3(0f, .40f, 0f), new Vector3(.14f, .70f, .14f), frame);
        }

        private static void Seat(Transform p, Vector3 pos, bool positiveX, Material cushion, Material frame)
        {
            Transform s = Group(p, "DoubleTrainSeat");
            s.localPosition = pos;
            s.localRotation = positiveX ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
            Cube(s, "SeatBase", Vector3.zero, new Vector3(.74f, .58f, 1.76f), frame);
            Cube(s, "SeatCushion", new Vector3(0f, .34f, 0f), new Vector3(.70f, .14f, 1.66f), cushion);
            Cube(s, "SeatBack", new Vector3(-.32f, .80f, 0f), new Vector3(.14f, .90f, 1.66f), cushion);
        }

        private static void RemoveMainGreenhouseUtilities(Transform root)
        {
            Remove(root, "GreenhouseUtilityBay");
        }

        private static void BuildPanoramicFront(Transform g, Material glass)
        {
            Cube(g, "PanoramicWindshield", new Vector3(-HalfL - .015f, 2.02f, 0f), new Vector3(.035f, 1.55f, 8.48f), glass);
        }

        private static void BuildLights(Transform g, int index, Material frame, Material amber)
        {
            Transform l = Group(g, "ThreeDeckLightingV4");
            Light(l, "LowerDeckLight", new Vector3(0f, -.52f, 0f), .78f, new Color(.72f, .82f, .90f));
            Light(l, "MainDeckLight", new Vector3(0f, 2.72f, 0f), index == 4 ? 1.22f : 1.08f, index == 4 ? new Color(.72f, .95f, .72f) : new Color(.91f, .82f, .68f));
            Light(l, "UpperDeckLight", new Vector3(0f, 5.58f, 0f), .82f, new Color(.86f, .80f, .70f));
            foreach (float y in new[] { -.28f, 2.96f, 6.00f })
                foreach (float x in new[] { -5.8f, 0f, 5.8f })
                {
                    Cube(l, "LightHousing", new Vector3(x, y, 0f), new Vector3(2.8f, .07f, .28f), frame);
                    Cube(l, "LightStrip", new Vector3(x, y - .045f, 0f), new Vector3(2.4f, .03f, .16f), amber);
                }
        }

        private static void Light(Transform p, string name, Vector3 pos, float intensity, Color color)
        {
            GameObject go = new GameObject(name); go.transform.SetParent(p, false); go.transform.localPosition = pos;
            UnityEngine.Light light = go.AddComponent<UnityEngine.Light>();
            light.type = LightType.Point; light.range = 9.5f; light.intensity = intensity; light.color = color; light.shadows = LightShadows.None;
        }

        private static void ApplyConnector()
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath);
            if (asset == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Material floor = Mat("Train_Floor"), frame = Mat("Train_Frame"), rubber = Mat("Train_Rubber");
                Remove(root.transform, "MultiDeckConnector"); Remove(root.transform, "TechnicalBasementConnector"); Remove(root.transform, "ThreeDeckConnectorV3"); Remove(root.transform, "ThreeDeckConnectorV4");
                Transform g = Group(root.transform, "ThreeDeckConnectorV4");
                Cube(g, "LowerFloor", new Vector3(0f, LowerY, 0f), new Vector3(.82f, .18f, 2.20f), floor);
                Cube(g, "LowerL", new Vector3(0f, (LowerY + MainY) / 2f, -1.15f), new Vector3(.82f, 3f, .12f), rubber);
                Cube(g, "LowerR", new Vector3(0f, (LowerY + MainY) / 2f, 1.15f), new Vector3(.82f, 3f, .12f), rubber);
                Cube(g, "UpperFloor", new Vector3(0f, UpperY, 0f), new Vector3(.82f, .18f, 2.20f), floor);
                Cube(g, "UpperRoof", new Vector3(0f, RoofY, 0f), new Vector3(.82f, .18f, 2.20f), frame);
                Cube(g, "UpperL", new Vector3(0f, (UpperY + RoofY) / 2f, -1.15f), new Vector3(.82f, 3f, .12f), rubber);
                Cube(g, "UpperR", new Vector3(0f, (UpperY + RoofY) / 2f, 1.15f), new Vector3(.82f, 3f, .12f), rubber);
                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ApplyScene()
        {
            if (!System.IO.File.Exists(ScenePath)) return;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject train = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (train != null)
            {
                for (int i = 0; i < Cars.Length; i++)
                {
                    Transform car = train.transform.Find(Cars[i]); if (car == null) continue;
                    Vector3 p = car.localPosition; p.x = Centers[i]; p.z = 0f; car.localPosition = p;
                }
                List<Transform> connectors = new List<Transform>();
                for (int i = 0; i < train.transform.childCount; i++) if (train.transform.GetChild(i).name == "CarConnector") connectors.Add(train.transform.GetChild(i));
                connectors.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
                for (int i = 0; i < connectors.Count && i < 4; i++)
                {
                    Vector3 p = connectors[i].localPosition; p.x = (Centers[i] + Centers[i + 1]) / 2f; p.z = 0f; connectors[i].localPosition = p;
                }
            }
            Snow(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void Snow(Scene scene)
        {
            foreach (GameObject r in scene.GetRootGameObjects())
                foreach (ParticleSystem ps in r.GetComponentsInChildren<ParticleSystem>(true))
                {
                    string n = ps.name.ToLowerInvariant(); if (n.Contains("snow") || n.Contains("kar")) Object.DestroyImmediate(ps.gameObject);
                }
            GameObject atmosphere = GameObject.Find("SCENE_ATMOSPHERE"); if (atmosphere == null) atmosphere = new GameObject("SCENE_ATMOSPHERE");
            Material m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Atmosphere/Train_SnowParticles.mat"); if (m == null) return;
            SnowEmitter(atmosphere.transform, "ExteriorSnowFar_Left", -20.5f, m); SnowEmitter(atmosphere.transform, "ExteriorSnowFar_Right", 20.5f, m);
        }

        private static void SnowEmitter(Transform p, string name, float z, Material m)
        {
            GameObject go = new GameObject(name); go.transform.SetParent(p, false); go.transform.localPosition = new Vector3(0f, 6.2f, z);
            ParticleSystem ps = go.AddComponent<ParticleSystem>(); var main = ps.main; main.loop = true; main.startLifetime = 5.5f; main.startSpeed = 0f; main.startSize = .075f; main.maxParticles = 420; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.gravityModifier = .025f;
            var e = ps.emission; e.rateOverTime = 42f; var s = ps.shape; s.shapeType = ParticleSystemShapeType.Box; s.scale = new Vector3(135f, 11f, 1.4f); var v = ps.velocityOverLifetime; v.enabled = true; v.x = 10f; v.y = -1.35f; v.z = 0f;
            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = m; r.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void Cleanup(Transform root)
        {
            foreach (string n in new[] { "MultiDeckArchitecture", "ArchitectureRevisionV2", "ArchitectureRevisionV3", "FinalThreeDeckV4", "VerticalAccess", "UpperSleepingDeck", "BasementInfrastructure", "MultiDeckLevelLighting", "ArchitecturalSideLeft", "ArchitecturalSideRight" }) Remove(root, n);
            RemovePrefix(root, "CabFront"); RemovePrefix(root, "CabWindshield");
            HashSet<string> direct = new HashSet<string> { "WallLeft", "WallRight", "WallLower", "WallUpper", "WallPillar", "WindowTopFrame", "WindowBottomFrame", "WindowLeftFrame", "WindowRightFrame", "WindowGlass", "Ceiling", "CeilingRib", "BaseTrimLeft", "BaseTrimRight", "CeilingDuctLeft", "CeilingDuctRight", "CeilingLightHousing", "CeilingLightStrip", "InteriorLight" };
            for (int i = root.childCount - 1; i >= 0; i--) if (direct.Contains(root.GetChild(i).name)) Object.DestroyImmediate(root.GetChild(i).gameObject);
        }

        private static void MakeFloorMarker(Transform root)
        {
            Transform f = FindDirect(root, "Floor"); if (f == null) f = Group(root, "Floor");
            foreach (Component c in f.GetComponents<Component>()) if (!(c is Transform)) Object.DestroyImmediate(c);
            f.localPosition = Vector3.zero; f.localRotation = Quaternion.identity; f.localScale = new Vector3(CarLength, 1f, CarWidth);
        }

        private static void BuildFloor(Transform p, string prefix, float y, float t, List<Hole> holes, Material m)
        {
            List<float> xs = new List<float> { -HalfL, HalfL }, zs = new List<float> { -HalfW, HalfW };
            foreach (Hole h in holes) { xs.Add(h.x0); xs.Add(h.x1); zs.Add(h.z0); zs.Add(h.z1); }
            xs = xs.Distinct().OrderBy(x => x).ToList(); zs = zs.Distinct().OrderBy(z => z).ToList(); int n = 0;
            for (int xi = 0; xi < xs.Count - 1; xi++) for (int zi = 0; zi < zs.Count - 1; zi++)
            {
                float x0 = xs[xi], x1 = xs[xi + 1], z0 = zs[zi], z1 = zs[zi + 1]; Vector2 c = new Vector2((x0 + x1) / 2f, (z0 + z1) / 2f);
                if (holes.Any(h => h.Contains(c))) continue; Cube(p, prefix + n++, new Vector3(c.x, y, c.y), new Vector3(x1 - x0, t, z1 - z0), m);
            }
        }

        private static void Machine(Transform p, string n, Vector3 pos, Vector3 size, Material body, Material trim)
        {
            Transform g = Group(p, n); g.localPosition = pos; Cube(g, "Body", Vector3.zero, size, body); Cube(g, "Trim", new Vector3(0f, size.y * .18f, size.z * .50f), new Vector3(size.x * .84f, size.y * .18f, .08f), trim);
        }

        private static void Cabinet(Transform p, string n, Vector3 pos, Vector3 size, Material body, Material trim)
        {
            Transform g = Group(p, n); g.localPosition = pos; Cube(g, "Body", Vector3.zero, size, body); Cube(g, "Door", new Vector3(0f, 0f, size.z * .51f), new Vector3(size.x * .88f, size.y * .86f, .04f), trim);
        }

        private static void Pipe(Transform p, string n, Vector3 pos, Vector3 scale, Material m)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = n; go.transform.SetParent(p, false); go.transform.localPosition = pos; go.transform.localEulerAngles = new Vector3(0f, 0f, 90f); go.transform.localScale = scale; if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
        }

        private static Material Mat(string n) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/" + n + ".mat");
        private static Transform Group(Transform p, string n) { GameObject go = new GameObject(n); go.transform.SetParent(p, false); return go.transform; }
        private static Transform Empty(Transform p, string n, Vector3 pos) { Transform t = Group(p, n); t.localPosition = pos; return t; }
        private static GameObject Cube(Transform p, string n, Vector3 pos, Vector3 scale, Material m) { GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = n; go.transform.SetParent(p, false); go.transform.localPosition = pos; go.transform.localScale = scale; if (m != null) go.GetComponent<Renderer>().sharedMaterial = m; return go; }
        private static Transform FindDirect(Transform r, string n) { for (int i = 0; i < r.childCount; i++) if (r.GetChild(i).name == n) return r.GetChild(i); return null; }
        private static void Remove(Transform r, string n) { var a = r.GetComponentsInChildren<Transform>(true).Where(t => t != r && t.name == n).OrderByDescending(Depth).ToList(); foreach (Transform t in a) if (t != null) Object.DestroyImmediate(t.gameObject); }
        private static void RemovePrefix(Transform r, string n) { var a = r.GetComponentsInChildren<Transform>(true).Where(t => t != r && t.name.StartsWith(n)).OrderByDescending(Depth).ToList(); foreach (Transform t in a) if (t != null) Object.DestroyImmediate(t.gameObject); }
        private static int Depth(Transform t) { int d = 0; while (t.parent != null) { d++; t = t.parent; } return d; }

        private readonly struct Hole
        {
            public readonly float x0, x1, z0, z1;
            public Hole(float x0, float x1, float z0, float z1) { this.x0 = x0; this.x1 = x1; this.z0 = z0; this.z1 = z1; }
            public bool Contains(Vector2 p) => p.x > x0 + .001f && p.x < x1 - .001f && p.y > z0 + .001f && p.y < z1 - .001f;
        }
    }
}
#endif
