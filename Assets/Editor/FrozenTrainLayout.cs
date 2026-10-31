#if UNITY_EDITOR
using System;
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
    /// Canonical five-car train layout. Five separate 24 m cars are coupled by short main-deck gangways
    /// (real-train style): only the main-deck corridor doors connect cars; lower and upper decks are
    /// closed at every car end. Each car owns its own vertical access.
    /// </summary>
    public static class FrozenTrainLayout
    {
        // 24 m cars coupled by a 1 m gap (0.8 m clear between end-wall outer faces) holding the gangway.
        public const float CarLength = 24f;
        public const float VestibuleLength = 1f; // coupling gap / gangway length, centre to centre
        public const float CarWidth = 9.2f;
        public const float CarSpacing = CarLength + VestibuleLength; // 25 m
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

        // Only the main deck has end-wall openings (width, height above MainY), centred in the +Z
        // public corridor beside the access ladder. Lower and upper decks are closed at car ends.
        public static readonly Vector2 MainEndOpening = new Vector2(1.30f, 2.60f);
        public const float EndDoorCenterZ = 2.55f; // opening Z 1.90..3.20; door parks toward +Z

        public const float WindowBottom = 1.20f;
        public const float WindowTop = 2.52f;

        // Upper residential deck. Five cabin bays per side use the longer 24 m footprint.
        public const float CorridorHalf = 1.05f;
        public const float CorridorDoorWidth = .96f;
        public const float CorridorDoorHeight = 2.05f;
        public static readonly float[] SleepingDividerX = { -7.20f, -2.40f, 2.40f, 7.20f };
        public static readonly float[] SleepingPodX = { -9.35f, -4.70f, 0f, 4.70f, 9.35f };
        public static readonly float[] CorridorDoorX = { -9.35f, -4.70f, 0f, 4.70f, 9.35f };
        public const float SleepingPodZ = 3.12f;

        // In-car vertical access at the rear (+X) end of every car, beside the rear end door.
        // Upper ladder: on the +Z outer wall at the end of the public corridor, main -> upper deck.
        // Technical ladder (locomotive only): on the -Z outer wall, lower -> main deck.
        public const float AccessLadderX = HalfL - .95f;
        public const float UpperLadderZ = HalfW - .38f;      // rail plane, mounts reach the +Z wall
        public const float TechnicalLadderZ = -(HalfW - .38f);
        public const float UpperHoleX0 = HalfL - 1.50f, UpperHoleX1 = HalfL - .40f;
        public const float UpperHoleZ0 = HalfW - 1.05f, UpperHoleZ1 = HalfW - .10f;
        public const float TechnicalHoleX0 = UpperHoleX0, TechnicalHoleX1 = UpperHoleX1;
        public const float TechnicalHoleZ0 = -(HalfW - .10f), TechnicalHoleZ1 = -(HalfW - 1.05f);
        // Upper-deck stair landing: the +Z end cabin is shortened so the hatch opens onto the hall.
        public const float UpperLandingX = HalfL - 1.75f;

        /// <summary>
        /// Standard public-corridor window rhythm: six equal 2.2 m windows, mirror-symmetric about the
        /// car centre, between the end walls. Shared by every car that has the +Z public corridor.
        /// </summary>
        public static WindowSpan[] StandardCorridorWindows() => new[]
        {
            new WindowSpan(-9.2f, 2.2f), new WindowSpan(-5.5f, 2.2f), new WindowSpan(-1.8f, 2.2f),
            new WindowSpan(1.8f, 2.2f), new WindowSpan(5.5f, 2.2f), new WindowSpan(9.2f, 2.2f)
        };

        public static readonly CarSpec[] Cars = { Locomotive(), Generator(), Living(), Workshop(), Greenhouse() };

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
            MainWindows = new[]
            {
                new WindowSpan(-9.2f, 2.25f), new WindowSpan(-6.2f, 2.25f), new WindowSpan(-3.2f, 2.25f)
            },
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
            MainWindows = new[]
            {
                new WindowSpan(-9.2f, 2.2f), new WindowSpan(-5.5f, 2.2f), new WindowSpan(-1.8f, 2.2f),
                new WindowSpan(1.8f, 2.2f), new WindowSpan(5.5f, 2.2f), new WindowSpan(9.2f, 2.2f)
            },
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