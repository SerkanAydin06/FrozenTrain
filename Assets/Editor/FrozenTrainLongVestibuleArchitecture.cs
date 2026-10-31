#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final authority for the 24 m-car architecture.
    /// Cars own usable rooms; all normal vertical circulation lives in the four 4 m inter-car vestibules.
    /// Cars 2-5 keep the straight +Z public corridor and one closed functional room on the left.
    /// </summary>
    internal static class FrozenTrainLongVestibuleArchitecture
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string PrototypeMaterialRoot = "Assets/Materials/Prototype";
        private const string MarkerName = "__LongCarFinalV1";

        private const float PartitionZ = 1.78f;
        private const float CorridorMinZ = 1.90f;
        private const float CorridorMaxZ = 4.40f;
        private const float CorridorCenterZ = 3.15f;
        private const float CorridorWidth = 2.50f;
        private const float MainCeilingY = 3.02f;
        private const float WorkDoorWidth = 1.45f;
        private const float WorkDoorHeight = 2.34f;

        private const float UpperStairX = -.82f;
        private const float LowerStairX = .82f;
        // Side-wall ladder locations. The outer wall inner surface is z = -HalfW + .10.
        private const float LadderHoleNearZ = -L.HalfW + .10f;
        private const float LadderHoleFarZ = -L.HalfW + 1.55f;
        private const float LadderZ = -L.HalfW + .30f;
        private const float LandingZ = -L.HalfW + 1.92f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Hazard;
            public Material Fabric;
            public Material Warm;
            public Material Rubber;
            public Material Snow;
        }

        public static void Apply()
        {
            Mats mats = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++)
                FinalizeCar(i, mats);

            RebuildVestibulePrefab(mats);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ---- Cars ---------------------------------------------------------------------------

        private static void FinalizeCar(int index, Mats m)
        {
            string path = PrefabRoot + "/" + L.Cars[index].Name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RemoveDirectChild(root.transform, MarkerName);
                Transform marker = Group(root.transform, MarkerName);

                // No car sacrifices floor area to stairs anymore.
                RemoveDirectChild(root.transform, "VerticalAccess");
                RemoveDirectChild(root.transform, "__CirculationFinalV1");
                RemoveDirectChild(root.transform, "__CirculationFinalV2");
                RemoveDirectChild(root.transform, "__UpperCirculationV3");

                Transform qa = root.transform.Find("__ScreenshotQAV1");
                if (qa != null)
                {
                    DestroyNamedChild(qa, "MainDeckQA");
                    DestroyNamedChild(qa, "UpperDeckQA");
                }

                Transform architecture = root.transform.Find("__RightSideCorridorV2");
                if (architecture != null)
                {
                    DestroyNamedChild(architecture, "ResidentialIdentity");
                    if (index == 0)
                    {
                        DestroyNamedChild(architecture, "RightSidePublicCorridor");
                        DestroyNamedChild(architecture, "ClosedFunctionalCompartment");
                    }
                    else
                    {
                        RebuildLongCorridorDetails(architecture, index, m);
                    }
                }

                RemoveAllNamed(root.transform,
                    "UpperLadderLeft", "UpperLadderRight", "UpperAccessLadder", "TechnicalLadder",
                    "ResidentialAccess", "ResidentialStairwell", "CommonLandingMarker");

                if (index == 0) LayoutLocomotive(root.transform, marker, m);
                else LayoutFunctionalCar(root.transform, marker, index, m);

                BuildUpperResidentialFinishing(root.transform, marker, index, m);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void RebuildLongCorridorDetails(Transform architecture, int index, Mats m)
        {
            Transform corridor = FindFirst(architecture, "RightSidePublicCorridor");
            if (corridor == null) return;

            RemoveAllNamed(corridor,
                "CorridorLuminaire", "CorridorFill", "CorridorPortalHeader", "CorridorRoleBand");

            Material accent = Accent(index, m);
            foreach (float x in new[] { -9.2f, -4.6f, 0f, 4.6f, 9.2f })
            {
                Cube(corridor, "CorridorLuminaire", new Vector3(x, MainCeilingY - .0275f, CorridorCenterZ),
                    new Vector3(2.40f, .055f, .22f), m.Warm, false);
                Point(corridor, "CorridorFill", new Vector3(x, 2.52f, CorridorCenterZ),
                    .82f, 5.0f, new Color(.90f, .82f, .68f));
            }

            foreach (float x in new[] { -11.25f, 11.25f })
            {
                Cube(corridor, "CorridorPortalHeader", new Vector3(x, 2.64f, CorridorCenterZ),
                    new Vector3(.16f, .15f, CorridorWidth + .12f), m.Frame, false);
                Cube(corridor, "CorridorRoleBand", new Vector3(x - Mathf.Sign(x) * .10f, 2.31f, CorridorCenterZ),
                    new Vector3(.045f, .31f, CorridorWidth - .24f), accent, false);
            }

            // Keep exactly one work-room door in the middle of each 24 m functional compartment.
            Transform oldDoor = FindFirst(corridor, "FunctionalCompartmentDoor");
            if (oldDoor != null) oldDoor.localPosition = new Vector3(0f, L.MainTop + WorkDoorHeight * .5f, PartitionZ - .015f);
        }

        private static void LayoutLocomotive(Transform root, Transform marker, Mats m)
        {
            SetPos(root, "MainConsole", -9.25f, 0f);
            Transform[] displays = FindAll(root, "MainDisplay").OrderBy(t => t.localPosition.z).ToArray();
            for (int i = 0; i < displays.Length; i++)
            {
                Vector3 p = displays[i].localPosition;
                p.x = -8.58f;
                displays[i].localPosition = p;
            }

            Transform[] seats = FindAll(root, "DriverSeat").OrderBy(t => t.localPosition.z).ToArray();
            if (seats.Length > 0) seats[0].localPosition = new Vector3(-7.10f, seats[0].localPosition.y, -1.28f);
            if (seats.Length > 1) seats[1].localPosition = new Vector3(-7.10f, seats[1].localPosition.y, 1.28f);

            Transform[] cabinets = FindAll(root, "EquipmentCabinet").OrderBy(t => t.localPosition.x).ToArray();
            Vector2[] targets =
            {
                new Vector2(-1.20f, -3.88f), new Vector2(3.05f, -3.88f), new Vector2(6.75f, 3.88f)
            };
            for (int i = 0; i < cabinets.Length && i < targets.Length; i++)
            {
                Vector3 p = cabinets[i].localPosition;
                p.x = targets[i].x;
                p.z = targets[i].y;
                cabinets[i].localPosition = p;
            }

            SetPos(root, "ManagementTerminal", 8.70f, 3.62f);

            Transform cab = Group(marker, "LongLocomotiveCab");
            Cube(cab, "CabCentreRunner", new Vector3(0f, L.MainTop + .018f, 0f),
                new Vector3(21.7f, .025f, 2.30f), m.Floor, false);
            foreach (float x in new[] { -8.8f, -4.4f, 0f, 4.4f, 8.8f })
                Point(cab, "CabFill", new Vector3(x, 2.48f, 0f), .72f, 4.8f, new Color(.72f, .82f, .94f));
        }

        private static void LayoutFunctionalCar(Transform root, Transform marker, int index, Mats m)
        {
            switch (index)
            {
                case 1: LayoutGenerator(root); break;
                case 2: LayoutLiving(root); break;
                case 3: LayoutWorkshop(root); break;
                case 4: LayoutGreenhouse(root); break;
            }

            Transform detail = Group(marker, "LongFunctionalRoomFinishing");
            Material accent = Accent(index, m);
            Cube(detail, "WorkRoomEntryClearZone", new Vector3(0f, L.MainTop + .012f, .10f),
                new Vector3(3.20f, .020f, 2.80f), m.Floor, false);
            Cube(detail, "WorkRoomThreshold", new Vector3(0f, L.MainTop + .030f, PartitionZ - .16f),
                new Vector3(WorkDoorWidth + .24f, .035f, .12f), accent, false);

            foreach (float x in new[] { -8.8f, -4.4f, 0f, 4.4f, 8.8f })
                Point(detail, "WorkRoomFill", new Vector3(x, 2.48f, -.95f),
                    index == 4 ? .90f : .72f, 5.2f,
                    index == 4 ? new Color(.68f, .90f, .70f) : new Color(.78f, .76f, .70f));

            ClearPublicCorridor(root, index);
        }

        private static void LayoutGenerator(Transform root)
        {
            SetPos(root, "OperatorDesk", -6.6f, -3.72f);
            SetPos(root, "GeneratorConsole", -6.6f, -4.02f);
            SetPos(root, "ControlTable", .6f, -3.68f);
            SetPos(root, "MonitoringRack", 7.2f, -.55f);
        }

        private static void LayoutLiving(Transform root)
        {
            Transform[] bays = FindAll(root, "SeatingBay").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets =
            {
                new Vector3(-7.2f, L.MainTop, -3.05f),
                new Vector3(-2.65f, L.MainTop, -3.05f),
                new Vector3(2.55f, L.MainTop, -3.05f),
                new Vector3(7.00f, L.MainTop, -3.05f),
            };
            for (int i = 0; i < bays.Length && i < targets.Length; i++) bays[i].localPosition = targets[i];

            SetPos(root, "GalleyCounter", 8.65f, -.62f);
            SetPos(root, "GalleyTop", 8.65f, -.62f);
            MoveDetail(root, "__Production_GalleyDetails", 8.65f, -.62f);
            SetPos(root, "MedicalCabinet", 9.70f, -3.88f);
            SetPos(root, "WallHeater", -10.20f, -4.08f);
        }

        private static void LayoutWorkshop(Transform root)
        {
            Transform[] benches = FindAll(root, "Workbench").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets =
            {
                new Vector3(-7.5f, L.MainTop, -3.78f),
                new Vector3(-1.0f, L.MainTop, -3.78f),
                new Vector3(5.6f, L.MainTop, -3.78f),
            };
            for (int i = 0; i < benches.Length && i < targets.Length; i++) benches[i].localPosition = targets[i];

            SetPos(root, "MachineStation", 7.15f, -.58f);
            SetPos(root, "MachineScreen", 7.15f, -1.32f);
            MoveDetail(root, "__Production_MachineStationDetails", 7.15f, -.58f);
            SetPos(root, "ToolCabinet", 9.65f, -3.88f);
        }

        private static void LayoutGreenhouse(Transform root)
        {
            Transform[] beds = FindAll(root, "HydroponicBed").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            var targets = new List<Vector3>();
            foreach (float x in new[] { -9f, -6f, -3f, 0f, 3f, 6f, 9f })
                targets.Add(new Vector3(x, L.MainTop, -3.10f));
            foreach (float x in new[] { -9.2f, -6.2f, -3.1f, 1.6f, 4.7f, 7.8f, 9.8f })
                targets.Add(new Vector3(x, L.MainTop, -.35f));

            for (int i = 0; i < beds.Length && i < targets.Count; i++) beds[i].localPosition = targets[i];
            SetPos(root, "ClimatePanel", 10.05f, -3.82f);
            SetPos(root, "ClimateScreen", 10.05f, -3.55f);
        }

        private static void ClearPublicCorridor(Transform root, int index)
        {
            Transform main = FindFirst(root, L.Cars[index].MainGroupName);
            if (main == null) return;

            int n = 0;
            foreach (Transform child in main.Cast<Transform>().ToArray())
            {
                if (child.localPosition.z < CorridorMinZ - .08f) continue;
                Vector3 p = child.localPosition;
                p.z = (n++ % 2 == 0) ? -.62f : -3.78f;
                child.localPosition = p;
            }
        }

        private static void BuildUpperResidentialFinishing(Transform root, Transform marker, int index, Mats m)
        {
            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper == null) return;

            Transform detail = Group(marker, "UpperResidentialFinishing");
            Material accent = Accent(index, m);
            Cube(detail, "UpperHallRunner", new Vector3(0f, L.UpperTop + .014f, 0f),
                new Vector3(L.CarLength - .80f, .024f, 1.82f), m.Fabric, false);
            foreach (float x in new[] { -9.2f, -4.6f, 0f, 4.6f, 9.2f })
            {
                Cube(detail, "UpperHallNightLight", new Vector3(x, L.UpperTop + .30f, .96f),
                    new Vector3(.34f, .10f, .055f), accent, false);
                Point(detail, "UpperHallFill", new Vector3(x, 5.50f, 0f), .48f, 3.9f, new Color(.88f, .76f, .58f));
            }
        }

        // ---- Shared inter-car vestibule -----------------------------------------------------

        private static void RebuildVestibulePrefab(Mats m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

                root.name = "CarConnector";
                Transform shell = Group(root.transform, "VestibuleShell");
                BuildVestibuleFloors(shell, m);
                BuildVestibuleShell(shell, m);
                BuildMainTransit(shell, m);
                BuildLadderCore(shell, m);
                BuildDeckPortals(shell, m);
                BuildVestibuleLighting(shell, m);

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BuildVestibuleFloors(Transform p, Mats m)
        {
            Cube(p, "LowerFloor", new Vector3(0f, L.LowerY, 0f),
                new Vector3(L.VestibuleLength, L.LowerFloorT, L.CarWidth), m.Floor, true);

            Rect lowerHole = Rect.MinMaxRect(LowerStairX - .54f, LadderHoleNearZ, LowerStairX + .54f, LadderHoleFarZ);
            FloorAroundHole(p, "MainFloor", L.MainY, L.MainFloorT, lowerHole, m.Floor);

            Rect upperHole = Rect.MinMaxRect(UpperStairX - .54f, LadderHoleNearZ, UpperStairX + .54f, LadderHoleFarZ);
            FloorAroundHole(p, "UpperFloor", L.UpperY, L.UpperFloorT, upperHole, m.Floor);

            Cube(p, "Roof", new Vector3(0f, L.RoofY, 0f),
                new Vector3(L.VestibuleLength, L.RoofT, L.CarWidth), m.Wall, true);
            Cube(p, "RoofSnow", new Vector3(0f, L.RoofY + .17f, 0f),
                new Vector3(L.VestibuleLength - .16f, .12f, L.CarWidth - .30f), m.Snow, false);
        }

        private static void FloorAroundHole(Transform p, string prefix, float y, float thickness, Rect hole, Material mat)
        {
            float x0 = -L.VestibuleLength * .5f, x1 = L.VestibuleLength * .5f;
            float z0 = -L.HalfW, z1 = L.HalfW;
            int n = 0;
            if (hole.xMin > x0)
                Cube(p, prefix + n++, new Vector3((x0 + hole.xMin) * .5f, y, 0f),
                    new Vector3(hole.xMin - x0, thickness, L.CarWidth), mat, true);
            if (hole.xMax < x1)
                Cube(p, prefix + n++, new Vector3((hole.xMax + x1) * .5f, y, 0f),
                    new Vector3(x1 - hole.xMax, thickness, L.CarWidth), mat, true);
            if (hole.yMin > z0)
                Cube(p, prefix + n++, new Vector3((hole.xMin + hole.xMax) * .5f, y, (z0 + hole.yMin) * .5f),
                    new Vector3(hole.width, thickness, hole.yMin - z0), mat, true);
            if (hole.yMax < z1)
                Cube(p, prefix + n++, new Vector3((hole.xMin + hole.xMax) * .5f, y, (hole.yMax + z1) * .5f),
                    new Vector3(hole.width, thickness, z1 - hole.yMax), mat, true);
        }

        private static void BuildVestibuleShell(Transform p, Mats m)
        {
            float fullBottom = L.LowerY - L.LowerFloorT * .5f;
            float fullTop = L.RoofY + L.RoofT * .5f;
            float h = fullTop - fullBottom;
            float y = (fullTop + fullBottom) * .5f;
            foreach (float z in new[] { -L.HalfW, L.HalfW })
                Cube(p, z < 0f ? "OuterWallL" : "OuterWallR", new Vector3(0f, y, z),
                    new Vector3(L.VestibuleLength, h, L.WallT), m.Wall, true);

            Cube(p, "VestibuleUnderframe", new Vector3(0f, L.UnderframeBottom + .16f, 0f),
                new Vector3(L.VestibuleLength - .18f, .30f, L.CarWidth - .85f), m.Frame, true);
        }

        private static void BuildMainTransit(Transform p, Mats m)
        {
            // Shared open vestibule: no separate stair room, internal partition,
            // overhead header or additional stair-core door.
            BuildEndSlidingDoor(p, "MainVestibuleDoorFront", -L.VestibuleLength * .5f + .055f,
                L.MainTop + 1.22f, CorridorCenterZ, 2.40f, m, -2.22f);
            BuildEndSlidingDoor(p, "MainVestibuleDoorRear", L.VestibuleLength * .5f - .055f,
                L.MainTop + 1.22f, CorridorCenterZ, 2.40f, m, -2.22f);
        }

        private static void BuildLadderCore(Transform p, Mats m)
        {
            // Both ladders stand upright in the vestibule, never inside a usable car.
            // Their exits are on solid floor beyond the compact ladder openings.
            BuildVerticalLadder(p, "UpperLadder", UpperStairX, L.MainTop, L.UpperTop,
                "E - Üst kata çık", "E - Ana kata in", m);
            BuildVerticalLadder(p, "LowerLadder", LowerStairX, L.LowerTop, L.MainTop,
                "E - Ana kata çık", "E - Teknik kata in", m);
        }

        private static void BuildVerticalLadder(Transform parent, string name, float x,
            float lowY, float highY, string upText, string downText, Mats m)
        {
            Transform ladder = Group(parent, name);
            // Both rails face the open room and are mechanically tied into the -Z wall.
            ladder.localPosition = new Vector3(x, 0f, LadderZ);
            // A compact wall ladder spans one deck only. The former one-metre
            // extension above the landing made the rails look implausibly tall.
            const float railHalfWidth = .255f;
            float railBottom = lowY + .12f;
            float railTop = highY + .14f;

            foreach (float side in new[] { -1f, 1f })
            {
                Cube(ladder, "LadderRail",
                    new Vector3(side * railHalfWidth, (railBottom + railTop) * .5f, 0f),
                    new Vector3(.065f, railTop - railBottom, .065f), m.Frame, true);

                // Small support arms enter the side wall, rather than a floating ladder.
                foreach (float y in new[] { lowY + .63f, highY - .31f })
                    Cube(ladder, "WallMount",
                        new Vector3(side * railHalfWidth, y, -.11f),
                        new Vector3(.07f, .075f, .26f), m.Frame, false);
            }

            int rungCount = Mathf.Max(8, Mathf.CeilToInt((highY - lowY) / .31f));
            for (int i = 0; i < rungCount; i++)
            {
                float ratio = i / (float)(rungCount - 1);
                float y = Mathf.Lerp(lowY + .25f, highY - .23f, ratio);
                Cube(ladder, "LadderRung", new Vector3(0f, y, .06f),
                    new Vector3(.52f, .05f, .07f), m.Steel, true);
            }

            // Exit anchors stand on intact deck outside the narrow hatch cutouts.
            Transform lowerExit = Group(ladder, "LowerExit");
            lowerExit.localPosition = new Vector3(0f, lowY + .06f, LandingZ - LadderZ);
            Transform upperExit = Group(ladder, "UpperExit");
            upperExit.localPosition = new Vector3(0f, highY + .06f, LandingZ - LadderZ);
            ladder.gameObject.AddComponent<DeckLadder>().Configure(
                lowerExit, upperExit, upText, downText);
        }

        private static void BuildDeckPortals(Transform p, Mats m)
        {
            // Upper residential landing serves the two adjacent cars; lower landing serves their technical decks.
            foreach (float sign in new[] { -1f, 1f })
            {
                float x = sign * (L.VestibuleLength * .5f - .055f);
                BuildEndSlidingDoor(p, sign < 0f ? "UpperDoorFront" : "UpperDoorRear", x,
                    L.UpperTop + 1.18f, 0f, 2.18f, m, 1.92f);
                BuildEndSlidingDoor(p, sign < 0f ? "TechnicalDoorFront" : "TechnicalDoorRear", x,
                    L.LowerTop + 1.15f, 0f, 2.12f, m, 1.86f);
            }

        }

        private static void BuildEndSlidingDoor(Transform p, string name, float x, float y, float z,
            float width, Mats m, float openOffset)
        {
            // Use the actual car end-wall opening measurements. The original
            // 2.30m slab and fixed centre did not match upper/lower lintels.
            bool main = name.StartsWith("MainVestibuleDoor", StringComparison.Ordinal);
            bool upper = name.StartsWith("UpperDoor", StringComparison.Ordinal);
            float floor = main ? L.MainTop : upper ? L.UpperTop : L.LowerTop;
            Vector2 opening = main ? L.MainEndOpening : upper ? L.UpperEndOpening : L.LowerEndOpening;
            float lintelBottom = (main ? L.MainY : upper ? L.UpperY : L.LowerY) + opening.y;
            float panelHeight = lintelBottom - floor - .02f;
            float panelWidth = opening.x - .02f;
            if (panelHeight <= 0f) throw new InvalidOperationException("Invalid door opening: " + name);

            Transform door = Group(p, name);
            door.localPosition = new Vector3(x, (floor + lintelBottom) * .5f, z);
            Cube(door, "DoorPanel", Vector3.zero,
                new Vector3(.085f, panelHeight, panelWidth), m.Frame, false);
            door.gameObject.AddComponent<BoxCollider>().size =
                new Vector3(.10f, panelHeight, panelWidth);
            // Slide sideways within the train width, never through the exterior.
            float travel = panelWidth + .025f;
            door.gameObject.AddComponent<SlidingDoor>().Configure(
                new Vector3(0f, 0f, Mathf.Sign(openOffset) * travel), 2.8f);
        }

        private static void BuildVestibuleLighting(Transform p, Mats m)
        {
            foreach (float y in new[] { L.MainY + 2.72f, L.UpperY + 2.43f, L.LowerY + 2.52f })
            {
                Cube(p, "VestibuleLuminaire", new Vector3(0f, y, 3.15f), new Vector3(2.6f, .06f, .22f), m.Warm, false);
                Point(p, "VestibuleLight", new Vector3(0f, y - .18f, 2.75f), .72f, 4.6f, new Color(.88f, .82f, .72f));
            }
            Point(p, "StairCoreLight", new Vector3(0f, 1.55f, -2.25f), .82f, 5.0f, new Color(.72f, .80f, .88f));
        }

        // ---- Helpers ------------------------------------------------------------------------

        private static Mats LoadMaterials() => new Mats
        {
            Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)),
            Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
            Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
            Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
            Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
            Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
            Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f)),
            Rubber = PrototypeMat("Train_Rubber", new Color(.055f, .06f, .065f)),
            Snow = PrototypeMat("Train_Snow", new Color(.76f, .83f, .90f)),
        };

        private static Material Accent(int index, Mats m)
        {
            if (index == 1) return Mat("FT_PRO_Generator", new Color(.34f, .105f, .028f));
            if (index == 3) return Mat("FT_PRO_Workshop", new Color(.34f, .245f, .075f));
            if (index == 4) return Mat("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f));
            return index == 2 ? m.Fabric : m.Hazard;
        }

        private static Material Mat(string name, Color fallback)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            EnsureFolder(MaterialRoot);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader) { name = name };
            material.color = fallback;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", fallback);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material PrototypeMat(string name, Color fallback)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PrototypeMaterialRoot + "/" + name + ".mat");
            return material != null ? material : Mat("FT_VEST_" + name, fallback);
        }

        private static void SetPos(Transform root, string name, float x, float z)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            p.x = x; p.z = z;
            t.localPosition = p;
        }

        private static void MoveDetail(Transform root, string name, float x, float z)
        {
            foreach (Transform t in FindAll(root, name))
            {
                Vector3 p = t.localPosition;
                p.x = x; p.z = z;
                t.localPosition = p;
            }
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) Object.DestroyImmediate(c);
            }
            return go;
        }

        private static void Point(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = Group(parent, name);
            t.localPosition = pos;
            Light l = t.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.intensity = intensity;
            l.range = range;
            l.color = color;
            l.shadows = LightShadows.None;
        }

        private static void RemoveDirectChild(Transform root, string name)
        {
            Transform t = root.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        private static void DestroyNamedChild(Transform root, string name)
        {
            if (root == null) return;
            Transform t = root.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        private static void RemoveAllNamed(Transform root, params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t != null && t != root && set.Contains(t.name)) Object.DestroyImmediate(t.gameObject);
            }
        }

        private static Transform FindFirst(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindFirst(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static IEnumerable<Transform> FindAll(Transform root, string name)
        {
            if (root == null) yield break;
            if (root.name == name) yield return root;
            foreach (Transform child in root)
                foreach (Transform found in FindAll(child, name))
                    yield return found;
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