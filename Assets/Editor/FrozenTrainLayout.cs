#if UNITY_EDITOR
using System;
using FrozenTrain.Interaction;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    public enum TrainMat
    {
        Floor, Wall, Frame, Steel, Rubber, Glass, Mattress, Living, Amber, Screen,
        Generator, Workshop, Greenhouse, Plant, Snow, NearRock, FarRock
    }

    public enum PropShape
    {
        /// <summary>Single cube; Size is its full extent.</summary>
        Box,
        /// <summary>Cylinder lying along X; Size = (length, diameter, diameter).</summary>
        Pipe,
        /// <summary>Body cube with a trim strip on the face toward the car centreline.</summary>
        Machine,
        /// <summary>Body cube with a door panel on the face toward the car centreline.</summary>
        Cabinet,
        /// <summary>Empty parent; Children are placed relative to it.</summary>
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

        /// <summary>Makes this object an E interaction for a survival control.</summary>
        public Prop Bind(TrainSystemControlType control) { Control = control; return this; }

        /// <summary>Makes this object open the management panel on E.</summary>
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
        public bool PanoramicFront;
        /// <summary>Restricted ladder from the main deck down to the technical deck, at the -X end.</summary>
        public bool TechnicalLadder;
        /// <summary>Upper-deck ladders sit at the -X end instead of +X (used by the last car, which has no rear connector).</summary>
        public bool UpperLaddersAtFront;
        public Color MainLightColor = new Color(.91f, .82f, .68f);
        public float MainLightIntensity = 1.08f;
        public Prop[] Lower = Array.Empty<Prop>();
        public Prop[] Main = Array.Empty<Prop>();
    }

    /// <summary>
    /// Single source of truth for the authored train: dimensions, deck levels and every car's contents.
    /// FrozenTrainBuilder turns this data into prefabs and the scene; nothing else adds train geometry.
    /// </summary>
    public static class FrozenTrainLayout
    {
        // Footprint and spacing.
        public const float CarLength = 18f;
        public const float CarWidth = 9.2f;
        public const float CarSpacing = 18.8f;
        public const float HalfL = CarLength * .5f;
        public const float HalfW = CarWidth * .5f;
        public const float WallT = .20f;

        // Deck levels (floor centre heights) and walking surfaces.
        public const float LowerY = -3.10f;
        public const float MainY = 0f;
        public const float UpperY = 3.15f;
        public const float RoofY = 6.30f;
        public const float LowerFloorT = .20f;
        public const float MainFloorT = .22f;
        public const float UpperFloorT = .18f;
        public const float RoofT = .22f;
        public const float LowerTop = LowerY + LowerFloorT * .5f;
        public const float MainTop = MainY + MainFloorT * .5f;
        public const float UpperTop = UpperY + UpperFloorT * .5f;
        public const float RoofBottom = RoofY - RoofT * .5f;

        // Under the car and outside.
        public const float UnderframeBottom = LowerY - LowerFloorT * .5f - .30f;
        public const float RailTopY = UnderframeBottom - .44f;
        public const float GroundY = RailTopY - .14f;

        // End-wall openings (width, height) per deck.
        public static readonly Vector2 LowerEndOpening = new Vector2(2.15f, 2.48f);
        public static readonly Vector2 MainEndOpening = new Vector2(2.15f, 2.60f);
        public static readonly Vector2 UpperEndOpening = new Vector2(2.20f, 2.55f);

        // Main-deck side windows.
        public const float WindowBottom = 1.20f;
        public const float WindowTop = 2.52f;

        // Upper sleeping deck, authored with ladders at +X; mirrored when a car's ladders are at the front.
        public const float CorridorHalf = 1.05f;
        public const float CorridorDoorWidth = .92f;
        public const float CorridorDoorHeight = 2.05f;
        public static readonly float[] SleepingDividerX = { -4f, 0f, 4f };
        public static readonly float[] SleepingPodX = { -6.2f, -2.2f, 2.2f, 5.4f };
        public static readonly float[] CorridorDoorX = { -6f, -2f, 2f, 6f, 7.45f };
        public const float SleepingPodZ = 3.12f;

        // Ladders and the floor openings they pass through.
        public const float UpperLadderZ = 2.85f;
        public const float UpperHoleX0 = 7.05f, UpperHoleX1 = 8.65f, UpperHoleZ0 = 2.10f, UpperHoleZ1 = 3.60f;
        public const float TechnicalLadderZ = -3.45f;
        public const float TechnicalHoleX0 = -8.65f, TechnicalHoleX1 = -7.05f, TechnicalHoleZ0 = -4.15f, TechnicalHoleZ1 = -2.55f;

        public static readonly CarSpec[] Cars = { Locomotive(), Generator(), Living(), Workshop(), Greenhouse() };

        public static float CarCenterX(int index) => (index - (Cars.Length - 1) * .5f) * CarSpacing;

        /// <summary>Pipes, power and air that run the full length of every technical deck.</summary>
        public static readonly Prop[] LowerServices =
        {
            Pipe("WaterMain", new Vector3(0f, -1.00f, -4.02f), 16.2f, .075f, TrainMat.Steel),
            Pipe("HeatingMain", new Vector3(0f, -1.38f, -4.02f), 16.2f, .060f, TrainMat.Amber),
            Pipe("ReturnWater", new Vector3(0f, -1.75f, -4.02f), 16.2f, .060f, TrainMat.Rubber),
            Box("MainPowerBus", new Vector3(0f, -.72f, 4.02f), new Vector3(CarLength - 1f, .20f, .38f), TrainMat.Frame),
            Box("DataCableTray", new Vector3(0f, -1.10f, 4.02f), new Vector3(CarLength - 1f, .14f, .32f), TrainMat.Steel),
            Box("VentilationTrunk", new Vector3(0f, -.45f, 3.30f), new Vector3(CarLength - 1.4f, .46f, .62f), TrainMat.Frame),
        };

        private static CarSpec Locomotive() => new CarSpec
        {
            Name = "01_Locomotive",
            MainGroupName = "Locomotive_Interior",
            PanoramicFront = true,
            MainWindows = new[] { new WindowSpan(-6.4f, 2.25f), new WindowSpan(-3.55f, 2.25f) },
            Lower = new[]
            {
                OnLower(PropShape.Machine, "TractionMotorA", -4.8f, -2.75f, new Vector3(3.2f, 1.75f, 1.35f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Machine, "TractionMotorB", 0f, -2.75f, new Vector3(3.2f, 1.75f, 1.35f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Cabinet, "TractionBatteryBank", 4.8f, -2.9f, new Vector3(3.4f, 1.72f, 1.15f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Cabinet, "DriveInverter", 4.5f, 2.95f, new Vector3(3.8f, 1.72f, 1.05f), TrainMat.Frame, TrainMat.Screen),
            },
            Main = new[]
            {
                OnMain(PropShape.Box, "MainConsole", -6.35f, 0f, new Vector3(1.15f, 1.42f, 5.30f), TrainMat.Frame).Bind(TrainSystemControlType.CyclePace),
                Box("MainDisplay", new Vector3(-5.74f, 1.38f, -1.90f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-5.74f, 1.38f, -.95f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-5.74f, 1.38f, 0f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-5.74f, 1.38f, .95f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                Box("MainDisplay", new Vector3(-5.74f, 1.38f, 1.90f), new Vector3(.045f, .52f, .70f), TrainMat.Screen),
                DriverSeat(new Vector3(-4.35f, MainTop, -1.20f)),
                DriverSeat(new Vector3(-4.35f, MainTop, 1.20f)),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", 1.15f, -3.72f, new Vector3(3.0f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", 4.65f, -3.72f, new Vector3(2.8f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Cabinet, "EquipmentCabinet", 2.25f, 3.72f, new Vector3(2.4f, 2.30f, .62f), TrainMat.Frame, TrainMat.Steel),
                Group("ManagementTerminal", new Vector3(5.25f, MainTop, 3.62f), 0f,
                    Box("TerminalBody", new Vector3(0f, .84f, 0f), new Vector3(1.8f, 1.65f, .55f), TrainMat.Frame),
                    Box("TerminalScreen", new Vector3(0f, 1.01f, -.295f), new Vector3(1.35f, .72f, .035f), TrainMat.Screen),
                    Box("TerminalControls", new Vector3(0f, .32f, -.30f), new Vector3(1.35f, .16f, .40f), TrainMat.Amber)).AsTerminal(),
            },
        };

        private static CarSpec Generator() => new CarSpec
        {
            Name = "02_Generator",
            MainGroupName = "GeneratorControlDeck",
            TechnicalLadder = true,
            Lower = new[]
            {
                OnLower(PropShape.Machine, "MainGeneratorEngine", -3.8f, -2.75f, new Vector3(5.2f, 1.85f, 1.55f), TrainMat.Generator, TrainMat.Frame),
                OnLower(PropShape.Machine, "Alternator", 2f, -2.75f, new Vector3(3.8f, 1.85f, 1.55f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Box, "FuelTankA", -3.8f, 3.05f, new Vector3(4.2f, 1.62f, 1.30f), TrainMat.Steel),
                OnLower(PropShape.Box, "FuelTankB", 1.2f, 3.05f, new Vector3(4.2f, 1.62f, 1.30f), TrainMat.Steel),
                OnLower(PropShape.Cabinet, "GeneratorManifold", 5.7f, 3.10f, new Vector3(2f, 1.72f, 1.05f), TrainMat.Frame, TrainMat.Amber),
            },
            Main = new[]
            {
                OnMain(PropShape.Box, "OperatorDesk", -2.8f, -3.55f, new Vector3(4f, .90f, .90f), TrainMat.Frame),
                Box("GeneratorConsole", new Vector3(-2.8f, MainTop + 1.25f, -3.80f), new Vector3(2.2f, .72f, .18f), TrainMat.Screen)
                    .Bind(TrainSystemControlType.GeneratorToggle),
                OnMain(PropShape.Box, "MonitoringRack", 3.2f, 3.60f, new Vector3(4.4f, 2.10f, .72f), TrainMat.Steel),
                OnMain(PropShape.Box, "ControlTable", 3f, -3.45f, new Vector3(3.3f, .90f, .90f), TrainMat.Frame),
            },
        };

        private static CarSpec Living() => new CarSpec
        {
            Name = "03_Living",
            MainGroupName = "Living_Interior",
            MainWindows = new[] { new WindowSpan(-5.6f, 2.2f), new WindowSpan(-2.0f, 2.2f), new WindowSpan(2.0f, 2.2f), new WindowSpan(5.6f, 2.2f) },
            Lower = new[]
            {
                OnLower(PropShape.Box, "FreshWaterTank", -4.8f, -2.95f, new Vector3(4.2f, 1.70f, 1.35f), TrainMat.Steel),
                OnLower(PropShape.Box, "WasteWaterTank", 0f, -2.95f, new Vector3(4.2f, 1.70f, 1.35f), TrainMat.Frame),
                OnLower(PropShape.Machine, "HeatingBoiler", 4.7f, -2.9f, new Vector3(3.2f, 1.70f, 1.25f), TrainMat.Frame, TrainMat.Amber),
                OnLower(PropShape.Cabinet, "HVACPlant", -3f, 3.05f, new Vector3(4f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Cabinet, "WaterTreatmentUnit", 3f, 3.05f, new Vector3(4f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Screen),
            },
            Main = new[]
            {
                SeatingBay(new Vector3(-4.2f, MainTop, -3.05f)),
                SeatingBay(new Vector3(.3f, MainTop, -3.05f)),
                SeatingBay(new Vector3(-4.2f, MainTop, 3.05f)),
                SeatingBay(new Vector3(.3f, MainTop, 3.05f)),
                // Counter height stays below the window sill so the view out is clear.
                OnMain(PropShape.Box, "GalleyCounter", 5.2f, -3.45f, new Vector3(2.65f, .88f, .92f), TrainMat.Living),
                Box("GalleyTop", new Vector3(5.2f, MainTop + .91f, -3.45f), new Vector3(2.72f, .06f, .98f), TrainMat.Steel),
                // Between two windows, clear of the ladder landing at +X.
                OnMain(PropShape.Cabinet, "MedicalCabinet", 3.8f, 3.52f, new Vector3(1.3f, 2.05f, .66f), TrainMat.Frame, TrainMat.Steel)
                    .Bind(TrainSystemControlType.UseMedicine),
                OnMain(PropShape.Box, "WallHeater", -7.75f, -4.08f, new Vector3(1.50f, 1.0f, .34f), TrainMat.Amber)
                    .Bind(TrainSystemControlType.LivingPowerToggle),
            },
        };

        private static CarSpec Workshop() => new CarSpec
        {
            Name = "04_Workshop",
            MainGroupName = "Workshop_Interior",
            TechnicalLadder = true,
            Lower = new[]
            {
                OnLower(PropShape.Machine, "Compressor", -4.6f, -2.95f, new Vector3(3.4f, 1.70f, 1.25f), TrainMat.Frame, TrainMat.Steel),
                OnLower(PropShape.Machine, "HydraulicUnit", 0f, -2.95f, new Vector3(3.4f, 1.70f, 1.25f), TrainMat.Steel, TrainMat.Frame),
                OnLower(PropShape.Cabinet, "WorkshopPowerBus", 4.8f, -3f, new Vector3(3.2f, 1.70f, 1.10f), TrainMat.Frame, TrainMat.Amber),
                OnLower(PropShape.Cabinet, "PartsStorage", 3.2f, 3.05f, new Vector3(4.8f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
            },
            Main = new[]
            {
                Workbench(new Vector3(-3.95f, MainTop, -3.25f), 5.1f, 0f).Bind(TrainSystemControlType.WorkshopMaintenanceBoost),
                Workbench(new Vector3(2.20f, MainTop, -3.25f), 5.7f, 0f),
                Workbench(new Vector3(-2.20f, MainTop, 3.25f), 6.0f, 180f),
                OnMain(PropShape.Cabinet, "ToolCabinet", 5.95f, 3.35f, new Vector3(2.1f, 2.20f, .68f), TrainMat.Frame, TrainMat.Steel),
                OnMain(PropShape.Box, "MachineStation", 5.05f, 1.55f, new Vector3(2.0f, 1.55f, 1.35f), TrainMat.Frame)
                    .Bind(TrainSystemControlType.WorkshopPowerToggle),
                Box("MachineScreen", new Vector3(5.05f, MainTop + 1.17f, .85f), new Vector3(1.15f, .48f, .04f), TrainMat.Screen),
            },
        };

        private static CarSpec Greenhouse()
        {
            var main = new System.Collections.Generic.List<Prop>();
            bool flushBound = false;
            foreach (float z in new[] { -2.55f, 2.55f })
                for (int i = -2; i <= 2; i++)
                {
                    Prop bed = HydroponicBed(new Vector3(i * 2.5f, MainTop, z));
                    if (!flushBound) { bed.Bind(TrainSystemControlType.GreenhouseNutrientFlush); flushBound = true; }
                    main.Add(bed);
                }
            main.Add(OnMain(PropShape.Box, "ClimatePanel", 6.55f, -3.62f, new Vector3(1.25f, 1.25f, .40f), TrainMat.Frame));
            main.Add(Box("ClimateScreen", new Vector3(6.55f, MainTop + 1.24f, -3.40f), new Vector3(.86f, .45f, .04f), TrainMat.Screen));

            return new CarSpec
            {
                Name = "05_Greenhouse",
                MainGroupName = "Greenhouse_Interior",
                UpperLaddersAtFront = true,
                MainLightColor = new Color(.72f, .95f, .72f),
                MainLightIntensity = 1.22f,
                Lower = new[]
                {
                    OnLower(PropShape.Box, "WaterTank", -4.8f, -2.95f, new Vector3(4.2f, 1.70f, 1.35f), TrainMat.Steel)
                        .Bind(TrainSystemControlType.RefillWaterFromReserve),
                    OnLower(PropShape.Box, "NutrientTank", 0f, -2.95f, new Vector3(4.2f, 1.70f, 1.35f), TrainMat.Steel),
                    OnLower(PropShape.Machine, "PumpUnit", 4.8f, -2.95f, new Vector3(3f, 1.65f, 1.25f), TrainMat.Frame, TrainMat.Steel)
                        .Bind(TrainSystemControlType.GreenhousePowerToggle),
                    OnLower(PropShape.Cabinet, "IrrigationManifold", -3.1f, 3.05f, new Vector3(4.2f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Screen),
                    OnLower(PropShape.Cabinet, "ClimatePlant", 3.1f, 3.05f, new Vector3(4.2f, 1.70f, 1.05f), TrainMat.Frame, TrainMat.Steel),
                },
                Main = main.ToArray(),
            };
        }

        // ---- Composite furniture ------------------------------------------------------------

        private static Prop DriverSeat(Vector3 floorPos) => Group("DriverSeat", floorPos, 0f,
            Box("SeatBase", new Vector3(0f, .37f, 0f), new Vector3(.72f, .62f, .72f), TrainMat.Frame),
            Box("SeatCushion", new Vector3(0f, .73f, 0f), new Vector3(.66f, .14f, .66f), TrainMat.Rubber),
            Box("SeatBack", new Vector3(.32f, 1.15f, 0f), new Vector3(.14f, .82f, .66f), TrainMat.Rubber));

        /// <summary>Four-person train bay: two facing double seats and a table.</summary>
        private static Prop SeatingBay(Vector3 floorPos) => Group("SeatingBay", floorPos, 0f,
            DoubleSeat(new Vector3(-1.08f, 0f, 0f), 0f),
            DoubleSeat(new Vector3(1.08f, 0f, 0f), 180f),
            Box("DiningTable", new Vector3(0f, .72f, 0f), new Vector3(1.12f, .06f, 1.50f), TrainMat.Steel),
            Box("TableLeg", new Vector3(0f, .35f, 0f), new Vector3(.14f, .70f, .14f), TrainMat.Frame));

        private static Prop DoubleSeat(Vector3 pos, float yaw) => Group("DoubleTrainSeat", pos, yaw,
            Box("SeatBase", new Vector3(0f, .22f, 0f), new Vector3(.74f, .44f, 1.76f), TrainMat.Frame),
            Box("SeatCushion", new Vector3(0f, .51f, 0f), new Vector3(.70f, .14f, 1.66f), TrainMat.Living),
            Box("SeatBack", new Vector3(-.32f, .98f, 0f), new Vector3(.14f, .90f, 1.66f), TrainMat.Living));

        /// <summary>Bench with its back board on the local -Z side; yaw 180 turns it to face -Z.</summary>
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

        // ---- Prop constructors --------------------------------------------------------------

        public static Prop Box(string name, Vector3 pos, Vector3 size, TrainMat mat) =>
            new Prop { Name = name, Shape = PropShape.Box, Position = pos, Size = size, Body = mat };

        public static Prop Pipe(string name, Vector3 pos, float length, float diameter, TrainMat mat) =>
            new Prop { Name = name, Shape = PropShape.Pipe, Position = pos, Size = new Vector3(length, diameter, diameter), Body = mat };

        public static Prop Group(string name, Vector3 pos, float yaw, params Prop[] children) =>
            new Prop { Name = name, Shape = PropShape.Group, Position = pos, Yaw = yaw, Children = children };

        /// <summary>Object standing on the technical deck floor at (x, z).</summary>
        private static Prop OnLower(PropShape shape, string name, float x, float z, Vector3 size, TrainMat body, TrainMat trim = TrainMat.Frame) =>
            new Prop { Name = name, Shape = shape, Position = new Vector3(x, LowerTop + size.y * .5f, z), Size = size, Body = body, Trim = trim };

        /// <summary>Object standing on the main deck floor at (x, z).</summary>
        private static Prop OnMain(PropShape shape, string name, float x, float z, Vector3 size, TrainMat body, TrainMat trim = TrainMat.Frame) =>
            new Prop { Name = name, Shape = shape, Position = new Vector3(x, MainTop + size.y * .5f, z), Size = size, Body = body, Trim = trim };
    }
}
#endif
