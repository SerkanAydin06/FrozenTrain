#if UNITY_EDITOR
using System;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    public enum TrainMat
    {
        Floor, Wall, Frame, Steel, Rubber, Glass, Mattress, Living, Amber, Screen,
        Generator, Workshop, Greenhouse, Plant, Snow, NearRock, FarRock, Ice, Headlamp
    }

    public enum PropShape
    {
        Box,
        Pipe,
        Machine,
        Cabinet,
        Group
    }

    /// <summary>One authored object in car-local space (X = length, -X = locomotive front, Y = up, Z = width).</summary>
    public sealed class Prop
    {
        public string Name;
        public PropShape Shape;
        public Vector3 Position;
        public Vector3 Size;
        public float Yaw;
        public TrainMat Body;
        public TrainMat Trim;
        public Prop[] Children = Array.Empty<Prop>();
        public TrainSystemControlType? Control;
        public bool Terminal;

        public Prop Bind(TrainSystemControlType control) { Control = control; return this; }
        public Prop AsTerminal() { Terminal = true; return this; }
    }

    public readonly struct WindowSpan
    {
        public readonly float CenterX;
        public readonly float Width;
        public WindowSpan(float centerX, float width) { CenterX = centerX; Width = width; }
    }

    public sealed class CarSpec
    {
        public string Name;
        public string MainGroupName;
        public WindowSpan[] MainWindows = Array.Empty<WindowSpan>();
        // Optional override for the +Z main-deck wall (the public corridor side in cars 2-5).
        public WindowSpan[] CorridorSideWindows;
        public bool PanoramicFront;
        // TechnicalLadder: this car has the lower (technical) deck ladder. Every car has an upper ladder.
        public bool TechnicalLadder;
        public bool UpperLaddersAtFront; // legacy, unused (always false)
        public Color MainLightColor = new Color(.91f, .82f, .68f);
        public float MainLightIntensity = 1.08f;
        public Prop[] Lower = Array.Empty<Prop>();
        public Prop[] Main = Array.Empty<Prop>();
    }

    /// <summary>
    /// Modular train layout. The active prototype uses the locomotive and generator car,
    /// (real-train style): main and technical-deck gangways connect cars, while the
    /// upper accommodation decks remain closed between cars. Vertical access is restricted.
    /// </summary>
    public static class FrozenTrainLayout
    {
        // ---- Texture grid standard (2026-10-31) -------------------------------------------
        // One texture repeat = 1.00 m on every floor and interior wall (square, power-of-two
        // texture, e.g. 1024x1024 or 2048x2048). Every INTERIOR clear dimension is a whole number
        // of tiles, so floor/wall textures never end in a partial tile:
        //   clear length 24 m, clear width 9 m, clear deck height 3 m (identical on all 3 decks).
        // Openings (windows, doors, hatches) start and end on whole metres measured from the inner
        // corner (-InnerHalfL, -InnerHalfW) and from each deck's floor top.
        // Shell walls (0.20 m) and floor slabs (0.20 m) sit OUTSIDE the clear grid.
        public const float Tile = 1f;
        public const float ClearLength = 24f;
        public const float ClearWidth = 9f;
        public const float DeckClearHeight = 3f;
        public const float WallT = .20f;
        public const float SlabT = .20f;

        public const float CarLength = ClearLength + WallT;          // 24.2 m wall centre to centre
        public const float CarWidth = ClearWidth + WallT;            // 9.2 m
        public const float VestibuleLength = .80f;                   // end-wall centres across a joint (0.6 m clear gap)
        public const float CarSpacing = CarLength + VestibuleLength; // 25 m
        public const float HalfL = CarLength * .5f;
        public const float HalfW = CarWidth * .5f;
        public const float InnerHalfL = ClearLength * .5f;           // 12 m: inner end-wall face
        public const float InnerHalfW = ClearWidth * .5f;            // 4.5 m: inner side-wall face
        public const float DeckPitch = DeckClearHeight + SlabT;      // 3.2 m

        // Deck walking surfaces are whole deck pitches apart; main deck floor top = 0.
        public const float MainTop = 0f;
        public const float LowerTop = MainTop - DeckPitch;           // -3.2
        public const float UpperTop = MainTop + DeckPitch;           //  3.2
        public const float RoofBottom = UpperTop + DeckClearHeight;  //  6.2
        public const float LowerFloorT = SlabT, MainFloorT = SlabT, UpperFloorT = SlabT, RoofT = SlabT;
        public const float LowerY = LowerTop - SlabT * .5f;          // slab centres
        public const float MainY = MainTop - SlabT * .5f;
        public const float UpperY = UpperTop - SlabT * .5f;
        public const float RoofY = RoofBottom + SlabT * .5f;

        // Under the car and outside.
        public const float UnderframeBottom = LowerY - LowerFloorT * .5f - .30f;
        public const float RailTopY = UnderframeBottom - .44f;
        public const float GroundY = RailTopY - .14f;

        // Inter-car gangway floor already provides 1.20 m of clear width, but
        // the former 1.00 m end doors made both levels unnecessarily narrow.
        // One shared dimension now governs the gangway floor, wall apertures,
        // and upper/lower sliding door leaves. +Z corridor begins at Z=1.50.
        public const float InterCarClearWidth = 1.20f;
        public static readonly Vector2 MainEndOpening = new Vector2(InterCarClearWidth, 2f);
        public static readonly Vector2 LowerEndOpening = new Vector2(InterCarClearWidth, 2f);
        public const float EndDoorCenterZ = 2.10f; // 1.50..2.70, entirely on corridor side

        // Main-deck windows only (no windows on the lower technical or upper cabin decks).
        public const float WindowBottom = MainTop + 1f;
        public const float WindowTop = MainTop + 2f;

        // Upper residential deck: 3 m wide hall (Z -1.5..1.5) and six 4 m cabin bays per side.
        // Hall walls are centred just outside the hall so the hall faces sit on the grid.
        public const float CorridorHalf = 1.5f + .06f;
        public const float CorridorDoorWidth = 1f;
        public const float CorridorDoorHeight = 2f;
        public static readonly float[] SleepingDividerX = { -8f, -4f, 0f, 4f, 8f };
        public static readonly float[] SleepingPodX = { -10f, -6f, -2f, 2f, 6f, 10f };
        public static readonly float[] CorridorDoorX = { -9.5f, -5.5f, -1.5f, 1.5f, 5.5f, 9.5f };
        public const float SleepingPodZ = 3f;

        // In-car vertical access at the rear (+X) end of every car, beside the rear end door.
        // Upper ladder: on the +Z outer wall at the end of the public corridor, main -> upper deck.
        // Technical ladder (locomotive only): on the -Z outer wall, lower -> main deck.
        // Hatches are exactly 1 x 1 tile in the rear corner.
        public const float AccessLadderX = InnerHalfL - .5f;
        public const float UpperLadderZ = InnerHalfW - .28f;  // rail plane, mounts reach the +Z wall
        public const float TechnicalLadderZ = -UpperLadderZ;
        public const float UpperHoleX0 = InnerHalfL - 1f, UpperHoleX1 = InnerHalfL;
        public const float UpperHoleZ0 = InnerHalfW - 1f, UpperHoleZ1 = InnerHalfW;
        public const float TechnicalHoleX0 = UpperHoleX0, TechnicalHoleX1 = UpperHoleX1;
        public const float TechnicalHoleZ0 = -InnerHalfW, TechnicalHoleZ1 = -(InnerHalfW - 1f);
        // Upper-deck stair landing = the whole rear +Z cabin bay (X 8..12): the hall wall stops here.
        public const float UpperLandingX = 8f;

        /// <summary>
        /// Standard main-deck window rhythm: six 2 m windows on whole-metre edges, mirror-symmetric
        /// about the car centre (X ±1..3, ±5..7, ±9..11), 1 m to 2 m above the floor.
        /// </summary>
        public static WindowSpan[] StandardCorridorWindows() => new[]
        {
            new WindowSpan(-10f, 2f), new WindowSpan(-6f, 2f), new WindowSpan(-2f, 2f),
            new WindowSpan(2f, 2f), new WindowSpan(6f, 2f), new WindowSpan(10f, 2f)
        };

        // Minimal greybox stage: build only the locomotive and one functional car.
        // Remaining car specifications stay below for later expansion; adding them
        // back here automatically restores their prefab generation and connectors.
        public static readonly CarSpec[] Cars = { Locomotive(), Generator() };

        /// <summary>
        /// Detect accidental asymmetric bay editing while leaving rear access
        /// hatches, locomotive cockpit and functional corridors intentionally asymmetric.
        /// Called by the authoring pipeline before modifying any prefabs.
        /// </summary>
        public static void ValidateSymmetricBase()
        {
            ValidateTextureGrid();
            ValidatePairs("Cabin dividers", SleepingDividerX);
            ValidatePairs("Cabin doors", CorridorDoorX);
            ValidatePairs("Cabin bays", SleepingPodX);
            foreach (CarSpec car in Cars)
            {
                if (car.MainWindows.Length > 0)
                    ValidateWindows(car.Name + " left/main windows", car.MainWindows);
                if (car.CorridorSideWindows != null)
                {
                    ValidateWindows(car.Name + " right windows", car.CorridorSideWindows);
                    if (!SameWindowSpans(car.MainWindows, car.CorridorSideWindows))
                        throw new InvalidOperationException(
                            "Asymmetric external window openings in " + car.Name);
                }
            }
        }

        private static void ValidatePairs(string label, float[] positions)
        {
            for (int i = 0; i < positions.Length / 2; i++)
                if (Mathf.Abs(positions[i] + positions[positions.Length - 1 - i]) > .001f)
                    throw new InvalidOperationException(label + " is not mirrored about X=0.");
            if (positions.Length % 2 == 1 && Mathf.Abs(positions[positions.Length / 2]) > .001f)
                throw new InvalidOperationException(label + " centre bay is not at X=0.");
        }

        /// <summary>
        /// Texture-grid standard: clear dimensions and every opening edge must fall on whole tiles,
        /// measured from the inner corner and from the deck floor top. Throws before authoring.
        /// </summary>
        public static void ValidateTextureGrid()
        {
            OnGrid("Clear length", ClearLength);
            OnGrid("Clear width", ClearWidth);
            OnGrid("Deck clear height", DeckClearHeight);
            OnGrid("Window sill", WindowBottom - MainTop);
            OnGrid("Window head", WindowTop - MainTop);
            foreach (CarSpec car in Cars)
                foreach (WindowSpan[] set in new[] { car.MainWindows, car.CorridorSideWindows })
                    if (set != null)
                        foreach (WindowSpan w in set)
                        {
                            OnGrid(car.Name + " window edge", w.CenterX - w.Width * .5f + InnerHalfL);
                            OnGrid(car.Name + " window edge", w.CenterX + w.Width * .5f + InnerHalfL);
                        }
            // The inter-car coupling is an intentional 1.20 m modular-joint
            // exception to the 1 m wall tile grid. Do not reject its edge at
            // Z=2.70: exact alignment to the gangway is more important here.
            foreach (Vector2 opening in new[] { MainEndOpening, LowerEndOpening })
            {
                if (Mathf.Abs(opening.x - InterCarClearWidth) > .001f)
                    throw new InvalidOperationException("Inter-car door width must match gangway clear width.");
                if (EndDoorCenterZ - opening.x * .5f < 1.50f - .001f ||
                    EndDoorCenterZ + opening.x * .5f > InnerHalfW + .001f)
                    throw new InvalidOperationException("Inter-car door projects outside the corridor.");
                OnGrid("End door height", opening.y);
            }
            foreach (float x in CorridorDoorX)
                OnGrid("Cabin door edge", x - CorridorDoorWidth * .5f + InnerHalfL);
            OnGrid("Cabin door height", CorridorDoorHeight);
            foreach (float x in SleepingDividerX) OnGrid("Cabin divider", x + InnerHalfL);
            foreach (float v in new[] { UpperHoleX0 + InnerHalfL, UpperHoleX1 + InnerHalfL,
                         UpperHoleZ0 + InnerHalfW, UpperHoleZ1 + InnerHalfW,
                         TechnicalHoleZ0 + InnerHalfW, TechnicalHoleZ1 + InnerHalfW, UpperLandingX + InnerHalfL })
                OnGrid("Hatch / landing edge", v);
        }

        private static void OnGrid(string label, float metres)
        {
            float tiles = metres / Tile;
            if (Mathf.Abs(tiles - Mathf.Round(tiles)) > .001f)
                throw new InvalidOperationException(label + " (" + metres + " m) is off the " + Tile + " m texture grid.");
        }

        private static void ValidateWindows(string label, WindowSpan[] windows)
        {
            WindowSpan[] ordered = windows.OrderBy(w => w.CenterX).ToArray();
            for (int i = 0; i < ordered.Length / 2; i++)
            {
                WindowSpan left = ordered[i], right = ordered[ordered.Length - 1 - i];
                if (Mathf.Abs(left.CenterX + right.CenterX) > .001f ||
                    Mathf.Abs(left.Width - right.Width) > .001f)
                    throw new InvalidOperationException(label + " has unmatched mirror windows.");
            }
            if (ordered.Length % 2 == 1 &&
                Mathf.Abs(ordered[ordered.Length / 2].CenterX) > .001f)
                throw new InvalidOperationException(label + " has an off-centre window.");
        }

        private static bool SameWindowSpans(WindowSpan[] a, WindowSpan[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (Mathf.Abs(a[i].CenterX - b[i].CenterX) > .001f ||
                    Mathf.Abs(a[i].Width - b[i].Width) > .001f)
                    return false;
            return true;
        }

        public static float CarCenterX(int index) => (index - (Cars.Length - 1) * .5f) * CarSpacing;
        public static float VestibuleCenterX(int leftCarIndex) => (CarCenterX(leftCarIndex) + CarCenterX(leftCarIndex + 1)) * .5f;

        /// <summary>Pipes, power and air that run the full length of every technical deck.</summary>
        public static readonly Prop[] LowerServices =
        {
            Pipe("WaterMain", new Vector3(0f, -1.00f, -4.02f), CarLength - 1.8f, .075f, TrainMat.Steel),
            Pipe("HeatingMain", new Vector3(0f, -1.38f, -4.02f), CarLength - 1.8f, .060f, TrainMat.Amber),
            Pipe("ReturnWater", new Vector3(0f, -1.75f, -4.02f), CarLength - 1.8f, .060f, TrainMat.Rubber),
            Box("MainPowerBus", new Vector3(0f, -.72f, 4.02f), new Vector3(CarLength - 1f, .20f, .38f), TrainMat.Frame),
            Box("DataCableTray", new Vector3(0f, -1.10f, 4.02f), new Vector3(CarLength - 1f, .14f, .32f), TrainMat.Steel),
            Box("VentilationTrunk", new Vector3(0f, -.45f, 3.30f), new Vector3(CarLength - 1.4f, .46f, .62f), TrainMat.Frame),
        };

        private static CarSpec Locomotive() => new CarSpec
        {
            Name = "01_Locomotive",
            MainGroupName = "Locomotive_Interior",
            PanoramicFront = true,
            TechnicalLadder = true, // only the locomotive reaches the technical deck for now
            // Mirror the standard 6-bay window pattern about the car centre
            // on both sides. The front windshield remains locomotive-specific.
            MainWindows = StandardCorridorWindows(),
            Lower = new[]
            {
                OnLower(PropShape.Machine, "TractionMotorA", -7.2f, -2.75f, new Vector3(3.6f, 1.75f, 1.35f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Machine, "TractionMotorB", -2.4f, -2.75f, new Vector3(3.6f, 1.75f, 1.35f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Cabinet, "TractionBatteryBank", 3.1f, -2.9f, new Vector3(4.4f, 1.72f, 1.15f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Cabinet, "DriveInverter", 8.0f, 2.95f, new Vector3(3.8f, 1.72f, 1.05f), TrainMat.Frame, TrainMat.Screen),
            },
            Main = new[]
            {
                OnMain(PropShape.Box, "MainConsole", -9.25f, 0f, new Vector3(1.25f, 1.42f, 5.55f), TrainMat.Frame).Bind(TrainSystemControlType.CyclePace),
                Box("MainDisplay", new Vector3(-8.58f, 1.38f, -1.90f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-8.58f, 1.38f, -.95f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-8.58f, 1.38f, 0f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-8.58f, 1.38f, .95f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-8.58f, 1.38f, 1.90f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                DriverSeat(new Vector3(-7.10f, MainTop, -1.25f)),
                DriverSeat(new Vector3(-7.10f, MainTop, 1.25f)),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", -1.2f, -3.78f, new Vector3(3.0f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", 3.0f, -3.78f, new Vector3(3.2f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", 6.8f, 3.78f, new Vector3(3.0f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                Group("ManagementTerminal", new Vector3(8.75f, MainTop, 3.62f), 0f,
                    Box("TerminalBody", new Vector3(0f, .84f, 0f), new Vector3(1.8f, 1.65f, .55f), TrainMat.Frame),
                    Box("TerminalScreen", new Vector3(0f, 1.01f, -.295f), new Vector3(1.35f, .72f, .035f), TrainMat.Screen),
                    Box("TerminalControls", new Vector3(0f, .32f, -.30f), new Vector3(1.35f, .16f, .40f), TrainMat.Amber)).AsTerminal(),
            },
        };

        private static CarSpec Generator() => new CarSpec
        {
            Name = "02_Generator",
            // Identical openings on both exterior sides (same X positions and
            // widths). The +Z public corridor and -Z function room remain distinct.
            MainWindows = StandardCorridorWindows(),
            CorridorSideWindows = StandardCorridorWindows(),
            MainGroupName = "GeneratorControlDeck",
            Lower = new[]
            {
                OnLower(PropShape.Machine, "MainGeneratorEngine", -6.6f, -2.75f, new Vector3(6.2f, 1.85f, 1.55f), TrainMat.Generator, TrainMat.Frame),
                OnLower(PropShape.Machine, "Alternator", .4f, -2.75f, new Vector3(4.6f, 1.85f, 1.55f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Box, "FuelTankA", -6.8f, 3.05f, new Vector3(4.8f, 1.62f, 1.30f), TrainMat.Steel),
                OnLower(PropShape.Box, "FuelTankB", -1.2f, 3.05f, new Vector3(4.8f, 1.62f, 1.30f), TrainMat.Steel),
                OnLower(PropShape.Cabinet, "GeneratorManifold", 7.9f, 3.10f, new Vector3(2.8f, 1.72f, 1.05f), TrainMat.Frame, TrainMat.Amber),
            },
            Main = new[]
            {
                OnMain(PropShape.Box, "OperatorDesk", -6.3f, -3.55f, new Vector3(4.8f, .90f, .90f), TrainMat.Frame),
                Box("GeneratorConsole", new Vector3(-6.3f, MainTop + 1.25f, -3.80f), new Vector3(2.8f, .72f, .18f), TrainMat.Screen)
                    .Bind(TrainSystemControlType.GeneratorToggle),
                OnMain(PropShape.Box, "MonitoringRack", 6.8f, -1.00f, new Vector3(4.8f, 2.10f, .72f), TrainMat.Steel),
                OnMain(PropShape.Box, "ControlTable", .8f, -3.45f, new Vector3(4.2f, .90f, .90f), TrainMat.Frame),
            },
        };

        private static CarSpec Living() => new CarSpec
        {
            Name = "03_Living",
            CorridorSideWindows = StandardCorridorWindows(),
            MainGroupName = "Living_Interior",
            MainWindows = StandardCorridorWindows(),
            Lower = new[]
            {
                OnLower(PropShape.Box, "FreshWaterTank", -7.0f, -2.95f, new Vector3(4.8f, 1.70f, 1.35f), TrainMat.Steel),
                OnLower(PropShape.Box, "WasteWaterTank", -1.5f, -2.95f, new Vector3(4.8f, 1.70f, 1.35f), TrainMat.Frame),
                OnLower(PropShape.Machine, "HeatingBoiler", 5.3f, -2.9f, new Vector3(3.8f, 1.70f, 1.25f), TrainMat.Frame, TrainMat.Amber),
                OnLower(PropShape.Cabinet, "HVACPlant", -5.0f, 3.05f, new Vector3(4.6f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Cabinet, "WaterTreatmentUnit", 3.1f, 3.05f, new Vector3(4.8f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Screen),
            },
            Main = new[]
            {
                SeatingBay(new Vector3(-7.2f, MainTop, -2.95f)),
                SeatingBay(new Vector3(-2.5f, MainTop, -2.95f)),
                SeatingBay(new Vector3(2.4f, MainTop, -2.95f)),
                SeatingBay(new Vector3(7.0f, MainTop, -2.95f)),
                OnMain(PropShape.Box, "GalleyCounter", 8.7f, -4.00f, new Vector3(3.1f, .88f, .92f), TrainMat.Living),
                Box("GalleyTop", new Vector3(8.7f, MainTop + .91f, -4.00f), new Vector3(3.18f, .06f, .98f), TrainMat.Steel),
                OnMain(PropShape.Cabinet, "MedicalCabinet", 8.1f, -.55f, new Vector3(1.45f, 2.05f, .66f), TrainMat.Frame, TrainMat.Steel)
                    .Bind(TrainSystemControlType.UseMedicine),
                OnMain(PropShape.Box, "WallHeater", -10.2f, -4.08f, new Vector3(1.65f, 1.0f, .34f), TrainMat.Amber)
                    .Bind(TrainSystemControlType.LivingPowerToggle),
            },
        };

        private static CarSpec Workshop() => new CarSpec
        {
            Name = "04_Workshop",
            CorridorSideWindows = StandardCorridorWindows(),
            MainGroupName = "Workshop_Interior",
            Lower = new[]
            {
                OnLower(PropShape.Machine, "Compressor", -7.3f, -2.95f, new Vector3(4.0f, 1.70f, 1.25f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Machine, "HydraulicUnit", -2.2f, -2.95f, new Vector3(4.0f, 1.70f, 1.25f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Cabinet, "WorkshopPowerBus", 4.1f, -3f, new Vector3(3.8f, 1.70f, 1.10f), TrainMat.Frame, TrainMat.Amber),
                OnLower(PropShape.Cabinet, "PartsStorage", 6.2f, 3.05f, new Vector3(6.0f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
            },
            Main = new[]
            {
                Workbench(new Vector3(-7.2f, MainTop, -3.55f), 5.4f, 0f).Bind(TrainSystemControlType.WorkshopMaintenanceBoost),
                Workbench(new Vector3(-.9f, MainTop, -3.55f), 5.8f, 0f),
                Workbench(new Vector3(5.7f, MainTop, -3.55f), 5.2f, 0f),
                OnMain(PropShape.Cabinet, "ToolCabinet", 9.3f, -.60f, new Vector3(2.3f, 2.20f, .68f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Box, "MachineStation", 6.8f, -.55f, new Vector3(2.4f, 1.55f, 1.45f), TrainMat.Frame)
                    .Bind(TrainSystemControlType.WorkshopPowerToggle),
                Box("MachineScreen", new Vector3(6.8f, MainTop + 1.17f, -1.30f), new Vector3(1.25f, .48f, .04f), TrainMat.Screen),
            },
        };

        private static CarSpec Greenhouse()
        {
            var main = new System.Collections.Generic.List<Prop>();
            bool flushBound = false;
            foreach (float z in new[] { -3.10f, -.35f })
                for (int i = -3; i <= 3; i++)
                {
                    Prop bed = HydroponicBed(new Vector3(i * 2.80f, MainTop, z));
                    if (!flushBound) { bed.Bind(TrainSystemControlType.GreenhouseNutrientFlush); flushBound = true; }
                    main.Add(bed);
                }
            main.Add(OnMain(PropShape.Box, "ClimatePanel", 9.4f, -3.80f, new Vector3(1.45f, 1.25f, .40f), TrainMat.Frame));
            main.Add(Box("ClimateScreen", new Vector3(9.4f, MainTop + 1.24f, -3.58f), new Vector3(.96f, .45f, .04f), TrainMat.Screen));

            return new CarSpec
            {
                Name = "05_Greenhouse",
                CorridorSideWindows = StandardCorridorWindows(),
                MainGroupName = "Greenhouse_Interior",
                MainLightColor = new Color(.72f, .95f, .72f),
                MainLightIntensity = 1.22f,
                Lower = new[]
                {
                    OnLower(PropShape.Box, "WaterTank", -7.2f, -2.95f, new Vector3(4.8f, 1.70f, 1.35f), TrainMat.Steel)
                        .Bind(TrainSystemControlType.RefillWaterFromReserve),
                    OnLower(PropShape.Box, "NutrientTank", -1.7f, -2.95f, new Vector3(4.8f, 1.70f, 1.35f), TrainMat.Steel),
                    OnLower(PropShape.Machine, "PumpUnit", 5.5f, -2.95f, new Vector3(3.8f, 1.65f, 1.25f), TrainMat.Frame, TrainMat.Steel)
                        .Bind(TrainSystemControlType.GreenhousePowerToggle),
                    OnLower(PropShape.Cabinet, "IrrigationManifold", -5.2f, 3.05f, new Vector3(5.2f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Screen),
                    OnLower(PropShape.Cabinet, "ClimatePlant", 4.2f, 3.05f, new Vector3(5.2f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
                },
                Main = main.ToArray(),
            };
        }

        // ---- Composite furniture ------------------------------------------------------------

        private static Prop DriverSeat(Vector3 floorPos) => Group("DriverSeat", floorPos, 0f,
            Box("SeatBase", new Vector3(0f, .37f, 0f), new Vector3(.72f, .62f, .72f), TrainMat.Frame),
            Box("SeatCushion", new Vector3(0f, .73f, 0f), new Vector3(.66f, .14f, .66f), TrainMat.Rubber),
            Box("SeatBack", new Vector3(.32f, 1.15f, 0f), new Vector3(.14f, .82f, .66f), TrainMat.Rubber));

        private static Prop SeatingBay(Vector3 floorPos) => Group("SeatingBay", floorPos, 0f,
            DoubleSeat(new Vector3(-1.08f, 0f, 0f), 0f),
            DoubleSeat(new Vector3(1.08f, 0f, 0f), 180f),
            Box("DiningTable", new Vector3(0f, .72f, 0f), new Vector3(1.12f, .06f, 1.50f), TrainMat.Steel),
            Box("TableLeg", new Vector3(0f, .35f, 0f), new Vector3(.14f, .70f, .14f), TrainMat.Frame));

        private static Prop DoubleSeat(Vector3 pos, float yaw) => Group("DoubleTrainSeat", pos, yaw,
            Box("SeatBase", new Vector3(0f, .22f, 0f), new Vector3(.74f, .44f, 1.76f), TrainMat.Frame),
            Box("SeatCushion", new Vector3(0f, .51f, 0f), new Vector3(.70f, .14f, 1.66f), TrainMat.Living),
            Box("SeatBack", new Vector3(-.32f, .98f, 0f), new Vector3(.14f, .90f, 1.66f), TrainMat.Living));

        private static Prop Workbench(Vector3 floorPos, float length, float yaw) => Group("Workbench", floorPos, yaw,
            Box("WorkbenchBase", new Vector3(0f, .44f, 0f), new Vector3(length, .88f, .92f), TrainMat.Frame),
            Box("WorkbenchTop", new Vector3(0f, .92f, 0f), new Vector3(length + .08f, .08f, 1.00f), TrainMat.Steel),
            Box("BackBoard", new Vector3(0f, 1.50f, -.43f), new Vector3(length, 1.00f, .10f), TrainMat.Workshop));

        private static Prop HydroponicBed(Vector3 floorPos)
        {
            var parts = new System.Collections.Generic.List<Prop>
            {
                Box("BedFrame", new Vector3(0f, .40f, 0f), new Vector3(1.90f, .80f, 1.28f), TrainMat.Frame),
                Box("PlantBed", new Vector3(0f, .93f, 0f), new Vector3(1.68f, .25f, 1.08f), TrainMat.Greenhouse),
                Box("GrowLight", new Vector3(0f, 2.49f, 0f), new Vector3(1.75f, .07f, .20f), TrainMat.Amber),
            };
            for (int leaf = -2; leaf <= 2; leaf++)
                parts.Add(Box("Plant", new Vector3(leaf * .28f, 1.19f, 0f), new Vector3(.18f, .38f, .34f), TrainMat.Plant));
            return Group("HydroponicBed", floorPos, 0f, parts.ToArray());
        }

        public static Prop Box(string name, Vector3 pos, Vector3 size, TrainMat mat) =>
            new Prop { Name = name, Shape = PropShape.Box, Position = pos, Size = size, Body = mat };

        public static Prop Pipe(string name, Vector3 pos, float length, float diameter, TrainMat mat) =>
            new Prop { Name = name, Shape = PropShape.Pipe, Position = pos, Size = new Vector3(length, diameter, diameter), Body = mat };

        public static Prop Group(string name, Vector3 pos, float yaw, params Prop[] children) =>
            new Prop { Name = name, Shape = PropShape.Group, Position = pos, Yaw = yaw, Children = children };

        private static Prop OnLower(PropShape shape, string name, float x, float z, Vector3 size, TrainMat body, TrainMat trim = TrainMat.Frame) =>
            new Prop { Name = name, Shape = shape, Position = new Vector3(x, LowerTop + size.y * .5f, z), Size = size, Body = body, Trim = trim };

        private static Prop OnMain(PropShape shape, string name, float x, float z, Vector3 size, TrainMat body, TrainMat trim = TrainMat.Frame) =>
            new Prop { Name = name, Shape = shape, Position = new Vector3(x, MainTop + size.y * .5f, z), Size = size, Body = body, Trim = trim };
    }
}
#endif