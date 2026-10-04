#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final circulation authority after legacy builder/art passes.
    /// Locomotive is a private cab; cars 2-5 keep a clean right-side public corridor.
    /// Residential access is represented by a closed lower stair lobby plus an upper side landing,
    /// never by an exposed ladder or a full-height shaft blocking the residential corridor.
    /// </summary>
    internal static class FrozenTrainCirculationFinalizer
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__CirculationFinalV2";
        private const string OldMarkerName = "__CirculationFinalV1";

        private const float PartitionZ = 1.78f;
        private const float CorridorMinZ = 1.90f;
        private const float CorridorCenterZ = 3.15f;
        private const float WorkDoorWidth = 1.45f;
        private const float StairDoorWidth = 1.26f;
        private const float DoorHeight = 2.28f;
        private const float MainCeilingY = 3.02f;

        private const float StairDoorU = 7.25f;
        private const float StairInnerU = 6.05f;
        private const float StairOuterU = 8.48f;
        private const float UpperLandingFrontZ = -1.34f;
        private const float UpperLandingBackZ = -4.16f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Hazard;
            public Material Fabric;
            public Material Warm;
        }

        public static void Apply()
        {
            Mats mats = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    RemoveDirectChild(root.transform, OldMarkerName);
                    RemoveDirectChild(root.transform, MarkerName);
                    Transform marker = Group(root.transform, MarkerName);

                    RemoveLegacyMainDeckClutter(root.transform);
                    RemoveExposedUpperAccess(root.transform);
                    CloseOldUpperHatches(marker, i, mats);

                    if (i == 0)
                        FinalizeLocomotive(root.transform, marker, mats);
                    else
                        FinalizePassengerCar(root.transform, i, mats);

                    CleanKnownCorridorIntrusions(root.transform, i);
                    PolishFunctionalRoom(root.transform, marker, i, mats);
                    MoveMainDeckLightingOffCorridor(root.transform, i);
                    BuildResidentialAccess(root.transform, marker, i, mats);

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ---- Main deck ----------------------------------------------------------------------

        private static void FinalizeLocomotive(Transform root, Transform marker, Mats m)
        {
            Transform architecture = root.Find("__RightSideCorridorV2");
            if (architecture != null)
            {
                DestroyNamedChild(architecture, "RightSidePublicCorridor");
                DestroyNamedChild(architecture, "ClosedFunctionalCompartment");
            }

            Transform console = FindFirst(root, "MainConsole");
            if (console != null) console.localPosition = new Vector3(-6.28f, console.localPosition.y, 0f);

            Transform[] seats = FindAll(root, "DriverSeat").OrderBy(t => t.localPosition.z).ToArray();
            if (seats.Length > 0) seats[0].localPosition = new Vector3(-4.15f, seats[0].localPosition.y, -1.28f);
            if (seats.Length > 1) seats[1].localPosition = new Vector3(-4.15f, seats[1].localPosition.y, 1.28f);

            Transform[] cabinets = FindAll(root, "EquipmentCabinet").OrderBy(t => t.localPosition.x).ToArray();
            Vector2[] cabinetTargets =
            {
                new Vector2(.9f, -3.92f),
                new Vector2(4.05f, -3.92f),
                new Vector2(2.25f, 3.92f),
            };
            for (int i = 0; i < cabinets.Length && i < cabinetTargets.Length; i++)
            {
                Vector3 p = cabinets[i].localPosition;
                p.x = cabinetTargets[i].x;
                p.z = cabinetTargets[i].y;
                cabinets[i].localPosition = p;
            }

            Transform terminal = FindFirst(root, "ManagementTerminal");
            if (terminal != null) terminal.localPosition = new Vector3(4.95f, terminal.localPosition.y, 3.72f);

            Transform cab = Group(marker, "LocomotivePrivateCab");
            Cube(cab, "CabFloorIdentity", new Vector3(0f, L.MainTop + .015f, 0f),
                new Vector3(L.CarLength - .50f, .024f, L.CarWidth - .55f), m.Floor, false);
            Cube(cab, "CabRearGuide", new Vector3(6.55f, L.MainTop + .030f, 0f),
                new Vector3(2.4f, .024f, 2.35f), m.Fabric, false);
            AddPoint(cab, "CabRearFill", new Vector3(5.8f, 2.45f, 0f), .88f, 5.5f, new Color(.72f, .82f, .94f));
        }

        private static void FinalizePassengerCar(Transform root, int index, Mats m)
        {
            Transform architecture = root.Find("__RightSideCorridorV2");
            Transform corridor = architecture != null ? FindFirst(architecture, "RightSidePublicCorridor") : null;
            if (corridor == null) return;

            foreach (string n in new[] { "CompartmentWallFront", "CompartmentWallRear", "CompartmentDoorHeader", "CompartmentWall" })
                DestroyAllNamed(corridor, n);

            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            BuildPartitionWithOpenings(corridor, s * StairDoorU, m);
        }

        private static void BuildPartitionWithOpenings(Transform parent, float stairX, Mats m)
        {
            const float xMin = -8.78f, xMax = 8.78f;
            float wallY = L.MainTop + (MainCeilingY - L.MainTop) * .5f;
            float wallH = MainCeilingY - L.MainTop;

            var openings = new List<Vector2>
            {
                new Vector2(0f, WorkDoorWidth),
                new Vector2(stairX, StairDoorWidth),
            };
            openings.Sort((a, b) => a.x.CompareTo(b.x));

            float cursor = xMin;
            foreach (Vector2 opening in openings)
            {
                float left = Mathf.Max(xMin, opening.x - opening.y * .5f);
                float right = Mathf.Min(xMax, opening.x + opening.y * .5f);
                if (left > cursor + .03f)
                    Cube(parent, "CompartmentWall", new Vector3((cursor + left) * .5f, wallY, PartitionZ),
                        new Vector3(left - cursor, wallH, .16f), m.Wall, true);

                float headerH = Mathf.Max(.12f, wallH - DoorHeight);
                Cube(parent, "CompartmentDoorHeader", new Vector3(opening.x, L.MainTop + DoorHeight + headerH * .5f, PartitionZ),
                    new Vector3(opening.y, headerH, .16f), m.Frame, true);
                cursor = Mathf.Max(cursor, right);
            }

            if (cursor < xMax - .03f)
                Cube(parent, "CompartmentWall", new Vector3((cursor + xMax) * .5f, wallY, PartitionZ),
                    new Vector3(xMax - cursor, wallH, .16f), m.Wall, true);
        }

        // ---- Residential access -------------------------------------------------------------

        private static void RemoveExposedUpperAccess(Transform root)
        {
            Transform access = FindFirst(root, "VerticalAccess");
            if (access != null)
            {
                foreach (string n in new[] { "UpperLadderLeft", "UpperLadderRight", "UpperAccessLadder" })
                    DestroyAllNamed(access, n);
            }

            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper != null)
            {
                string[] obsolete =
                {
                    "HatchGuardOuterX", "HatchGuardOuterXPostA", "HatchGuardOuterXPostB",
                    "HatchGuardEndA", "HatchGuardEndAPostA", "HatchGuardEndAPostB",
                    "HatchGuardEndB", "HatchGuardEndBPostA", "HatchGuardEndBPostB",
                    "HatchHazardX", "HatchHazardA", "HatchHazardB",
                    "SharedVestibuleFloorMark", "VestibuleCentreRunner", "VestibuleThreshold", "VestibuleHeader"
                };
                foreach (string n in obsolete) DestroyAllNamed(upper, n);
            }

            Transform oldMarker = root.Find("__UpperCirculationV3");
            if (oldMarker != null)
            {
                DestroyAllNamed(oldMarker, "CommonLandingMarker");
                DestroyAllNamed(oldMarker, "CorridorDirectionTick");
            }
        }

        private static void CloseOldUpperHatches(Transform marker, int index, Mats m)
        {
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float x0 = s * L.UpperHoleX0;
            float x1 = s * L.UpperHoleX1;
            float xMin = Mathf.Min(x0, x1), xMax = Mathf.Max(x0, x1);
            float xC = (xMin + xMax) * .5f;
            float xW = xMax - xMin;
            float zW = L.UpperHoleZ1 - L.UpperHoleZ0;
            float zC = (L.UpperHoleZ0 + L.UpperHoleZ1) * .5f;

            Transform closure = Group(marker, "ClosedLegacyUpperHatches");
            Cube(closure, "UpperFloorPatchLeft", new Vector3(xC, L.UpperY, -zC),
                new Vector3(xW, L.UpperFloorT, zW), m.Floor, true);
            Cube(closure, "UpperFloorPatchRight", new Vector3(xC, L.UpperY, zC),
                new Vector3(xW, L.UpperFloorT, zW), m.Floor, true);
        }

        private static void BuildResidentialAccess(Transform root, Transform marker, int index, Mats m)
        {
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float stairX = s * StairDoorU;
            float xA = s * StairInnerU;
            float xB = s * StairOuterU;
            float xMin = Mathf.Min(xA, xB);
            float xMax = Mathf.Max(xA, xB);

            // Keep the lower stair lobby inside the private/work side of the main deck.
            float lowerBackZ = index == 0 ? -4.12f : -1.55f;
            float lowerFrontZ = index == 0 ? -1.62f : 1.60f;
            ClearStairFootprint(root, index, stairX, lowerBackZ, lowerFrontZ);

            Transform access = Group(marker, "ResidentialAccess");
            BuildLowerStairLobby(access, index, stairX, xMin, xMax, lowerBackZ, lowerFrontZ, m);
            Transform lowerDoor = index == 0
                ? BuildLocomotiveLowerAccessDoor(access, stairX, lowerFrontZ, m)
                : BuildPassengerLowerAccessDoor(access, stairX, m);

            Transform upperDoor = BuildUpperLandingNook(access, stairX, xMin, xMax, m);

            float travelYaw = s > 0f ? -90f : 90f;
            Transform lowerAnchor = Empty(access, "LowerLandingAnchor",
                index == 0
                    ? new Vector3(stairX - s * .35f, L.MainTop + .055f, -.85f)
                    : new Vector3(stairX - s * .20f, L.MainTop + .055f, 2.34f));
            lowerAnchor.localRotation = Quaternion.Euler(0f, travelYaw, 0f);

            Transform upperAnchor = Empty(access, "UpperLandingAnchor",
                new Vector3(stairX - s * .28f, L.UpperTop + .055f, -.72f));
            upperAnchor.localRotation = Quaternion.Euler(0f, travelYaw, 0f);

            string up = index == 0 ? "E - Mürettebat katına çık" : "E - Konut katına çık";
            string down = index == 0 ? "E - Lokomotife in" : "E - Ana koridora in";
            lowerDoor.gameObject.AddComponent<DeckLadder>().Configure(lowerAnchor, upperAnchor, up, down);
            upperDoor.gameObject.AddComponent<DeckLadder>().Configure(lowerAnchor, upperAnchor, up, down);
        }

        private static void BuildLowerStairLobby(Transform parent, int index, float stairX, float xMin, float xMax,
            float zBack, float zFront, Mats m)
        {
            Transform lobby = Group(parent, "LowerStairLobby");
            float xCentre = (xMin + xMax) * .5f;
            float xLength = xMax - xMin;
            float h = MainCeilingY - L.MainTop;
            float y = L.MainTop + h * .5f;
            float zCentre = (zBack + zFront) * .5f;
            float zLength = zFront - zBack;

            Cube(lobby, "LobbyFloor", new Vector3(xCentre, L.MainTop + .018f, zCentre),
                new Vector3(xLength, .025f, zLength), m.Floor, false);
            Cube(lobby, "LobbyBackWall", new Vector3(xCentre, y, zBack), new Vector3(xLength, h, .12f), m.Wall, true);
            Cube(lobby, "LobbySideA", new Vector3(xMin, y, zCentre), new Vector3(.12f, h, zLength), m.Wall, true);
            Cube(lobby, "LobbySideB", new Vector3(xMax, y, zCentre), new Vector3(.12f, h, zLength), m.Wall, true);

            if (index == 0)
                BuildWallWithDoorAlongX(lobby, "LocomotiveLobbyFront", xMin, xMax, zFront, stairX, StairDoorWidth, L.MainTop, MainCeilingY, m);

            // Visible compact stair flight behind the access door. Gameplay transition still uses DeckLadder,
            // but the room now reads as a believable stair lobby rather than a black teleport box.
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float z = Mathf.Lerp(zFront - .32f, zBack + .48f, t);
                float stepY = L.MainTop + .13f + i * .29f;
                Cube(lobby, "StairStep", new Vector3(stairX, stepY, z),
                    new Vector3(1.12f, .18f, .34f), m.Steel, true);
            }
            Cube(lobby, "StairRail", new Vector3(stairX - .66f, L.MainTop + 1.28f, zCentre),
                new Vector3(.055f, 1.85f, Mathf.Max(.9f, zLength - .55f)), m.Frame, false);
            AddPoint(lobby, "StairLobbyLight", new Vector3(stairX, 2.56f, zCentre), .72f, 4.0f, new Color(.88f, .72f, .48f));
        }

        private static Transform BuildPassengerLowerAccessDoor(Transform parent, float stairX, Mats m)
        {
            Transform host = Group(parent, "LowerResidentialAccessInteraction");
            Transform door = Group(host, "ResidentialAccessDoor");
            door.localPosition = new Vector3(stairX, L.MainTop + DoorHeight * .5f, PartitionZ - .025f);
            AddDoorVisual(door, false, m);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(StairDoorWidth, DoorHeight, .22f);
            return door;
        }

        private static Transform BuildLocomotiveLowerAccessDoor(Transform parent, float stairX, float zFront, Mats m)
        {
            Transform host = Group(parent, "LocomotiveResidentialAccessInteraction");
            Transform door = Group(host, "LocomotiveResidentialAccessDoor");
            door.localPosition = new Vector3(stairX, L.MainTop + DoorHeight * .5f, zFront + .025f);
            AddDoorVisual(door, false, m);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(StairDoorWidth, DoorHeight, .22f);
            return door;
        }

        private static Transform BuildUpperLandingNook(Transform parent, float stairX, float xMin, float xMax, Mats m)
        {
            Transform nook = Group(parent, "UpperResidentialLandingNook");
            float h = L.RoofBottom - L.UpperTop;
            float y = L.UpperTop + h * .5f;
            float xCentre = (xMin + xMax) * .5f;
            float xLength = xMax - xMin;
            float zCentre = (UpperLandingBackZ + UpperLandingFrontZ) * .5f;
            float zLength = UpperLandingFrontZ - UpperLandingBackZ;

            Cube(nook, "LandingFloor", new Vector3(xCentre, L.UpperTop + .018f, zCentre),
                new Vector3(xLength, .025f, zLength), m.Floor, false);
            Cube(nook, "LandingBackWall", new Vector3(xCentre, y, UpperLandingBackZ),
                new Vector3(xLength, h, .12f), m.Wall, true);
            Cube(nook, "LandingSideA", new Vector3(xMin, y, zCentre), new Vector3(.12f, h, zLength), m.Wall, true);
            Cube(nook, "LandingSideB", new Vector3(xMax, y, zCentre), new Vector3(.12f, h, zLength), m.Wall, true);
            BuildWallWithDoorAlongX(nook, "LandingFront", xMin, xMax, UpperLandingFrontZ, stairX,
                StairDoorWidth, L.UpperTop, L.RoofBottom, m);

            Transform host = Group(nook, "UpperResidentialAccessInteraction");
            Transform door = Group(host, "UpperResidentialStairDoor");
            door.localPosition = new Vector3(stairX, L.UpperTop + DoorHeight * .5f, UpperLandingFrontZ + .025f);
            AddDoorVisual(door, true, m);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(StairDoorWidth, DoorHeight, .22f);

            Cube(nook, "LandingBench", new Vector3(xCentre, L.UpperTop + .28f, -3.62f),
                new Vector3(1.45f, .48f, .42f), m.Fabric, false);
            AddPoint(nook, "LandingWarmLight", new Vector3(stairX, 5.72f, -2.70f), .72f, 3.8f, new Color(.94f, .72f, .46f));
            return door;
        }

        private static void BuildWallWithDoorAlongX(Transform parent, string prefix, float xMin, float xMax, float z,
            float doorX, float doorWidth, float floorY, float ceilingY, Mats m)
        {
            float half = doorWidth * .5f;
            float left = doorX - half;
            float right = doorX + half;
            float h = ceilingY - floorY;
            float y = floorY + h * .5f;

            if (left > xMin)
                Cube(parent, prefix + "L", new Vector3((xMin + left) * .5f, y, z),
                    new Vector3(left - xMin, h, .12f), m.Wall, true);
            if (right < xMax)
                Cube(parent, prefix + "R", new Vector3((right + xMax) * .5f, y, z),
                    new Vector3(xMax - right, h, .12f), m.Wall, true);

            float headerBottom = floorY + DoorHeight;
            float headerH = Mathf.Max(.10f, ceilingY - headerBottom);
            Cube(parent, prefix + "Header", new Vector3(doorX, headerBottom + headerH * .5f, z),
                new Vector3(doorWidth, headerH, .12f), m.Frame, true);
        }

        private static void AddDoorVisual(Transform door, bool upper, Mats m)
        {
            Cube(door, "DoorPanel", Vector3.zero, new Vector3(StairDoorWidth - .07f, DoorHeight - .06f, .08f), m.Frame, false);
            Cube(door, "DoorBand", new Vector3(0f, .48f, -.052f), new Vector3(.88f, .14f, .025f), m.Hazard, false);
            Cube(door, "DoorHandle", new Vector3(.40f, -.05f, -.065f), new Vector3(.06f, .30f, .045f), m.Steel, false);
            Cube(door, upper ? "ResidentialLandingSign" : "ResidentialAccessSign",
                new Vector3(0f, .82f, -.055f), new Vector3(.72f, .16f, .026f), m.Warm, false);
        }

        // ---- Room/corridor cleanup ----------------------------------------------------------

        private static void RemoveLegacyMainDeckClutter(Transform root)
        {
            Transform qa = root.Find("__ScreenshotQAV1");
            if (qa != null) DestroyNamedChild(qa, "MainDeckQA");

            foreach (string n in new[] { "LivingWallFurniture", "GreenhouseOverhead" })
                DestroyAllNamed(root, n);
        }

        private static void MoveMainDeckLightingOffCorridor(Transform root, int index)
        {
            if (index == 0) return;
            Transform lighting = FindFirst(root, "Lighting");
            if (lighting == null) return;

            foreach (Transform t in lighting.GetComponentsInChildren<Transform>(true))
            {
                if ((t.name == "LightHousing" || t.name == "LightStrip") && t.localPosition.y > 2.75f && t.localPosition.y < 3.20f)
                {
                    Vector3 p = t.localPosition;
                    p.z = -2.85f;
                    t.localPosition = p;
                }
                else if (t.name == "MainDeckLight")
                {
                    Vector3 p = t.localPosition;
                    p.z = -1.35f;
                    t.localPosition = p;
                }
            }
        }

        private static void CleanKnownCorridorIntrusions(Transform root, int index)
        {
            if (index == 0) return;
            Transform main = FindFirst(root, L.Cars[index].MainGroupName);
            if (main == null) return;

            int lane = 0;
            foreach (Transform child in main.Cast<Transform>().ToArray())
            {
                if (child.localPosition.z < CorridorMinZ - .08f) continue;
                Vector3 p = child.localPosition;
                p.z = lane++ % 2 == 0 ? -.55f : -3.55f;
                child.localPosition = p;
            }
        }

        private static void PolishFunctionalRoom(Transform root, Transform marker, int index, Mats m)
        {
            Transform polish = Group(marker, "FunctionalRoomPolish");
            if (index > 0)
            {
                Cube(polish, "RoomServiceRunner", new Vector3(0f, L.MainTop + .020f, -1.82f),
                    new Vector3(14.9f, .020f, .74f), m.Floor, false);
                foreach (float x in new[] { -5.2f, 0f, 5.2f })
                    AddPoint(polish, "RoomFill", new Vector3(x, 2.42f, -1.70f), .68f, 4.5f,
                        index == 4 ? new Color(.66f, .88f, .68f) : new Color(.78f, .77f, .72f));
            }

            switch (index)
            {
                case 1:
                    SetPos(root, "OperatorDesk", -3.2f, -3.72f);
                    SetPos(root, "GeneratorConsole", -3.2f, -4.00f);
                    SetPos(root, "ControlTable", 2.15f, -3.70f);
                    SetPos(root, "MonitoringRack", 5.25f, -3.72f);
                    break;
                case 2:
                    ArrangeLiving(root);
                    break;
                case 3:
                    ArrangeWorkshop(root);
                    break;
                case 4:
                    ArrangeGreenhouse(root);
                    break;
            }
        }

        private static void ArrangeLiving(Transform root)
        {
            Transform[] bays = FindAll(root, "SeatingBay").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            Vector3[] targets =
            {
                new Vector3(-4.40f, L.MainTop, -3.12f), new Vector3(-4.40f, L.MainTop, -.38f),
                new Vector3(.25f, L.MainTop, -3.12f), new Vector3(.25f, L.MainTop, -.38f),
            };
            for (int i = 0; i < bays.Length && i < targets.Length; i++) bays[i].localPosition = targets[i];

            SetPos(root, "GalleyCounter", 4.75f, -3.78f);
            SetPos(root, "GalleyTop", 4.75f, -3.78f);
            MoveDetail(root, "__Production_GalleyDetails", 4.75f, -3.78f);

            Transform med = FindFirst(root, "MedicalCabinet");
            if (med != null)
            {
                Vector3 p = med.localPosition;
                p.x = 5.35f;
                p.z = -.48f;
                med.localPosition = p;
                med.localRotation = Quaternion.identity;
            }
        }

        private static void ArrangeWorkshop(Transform root)
        {
            Transform[] benches = FindAll(root, "Workbench").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets =
            {
                new Vector3(-4.25f, L.MainTop, -3.70f),
                new Vector3(1.05f, L.MainTop, -3.70f),
                new Vector3(-1.45f, L.MainTop, -.42f),
            };
            for (int i = 0; i < benches.Length && i < targets.Length; i++) benches[i].localPosition = targets[i];

            SetPos(root, "ToolCabinet", 5.55f, -3.88f);
            SetPos(root, "MachineStation", 4.75f, -.48f);
            SetPos(root, "MachineScreen", 4.75f, -1.18f);
            MoveDetail(root, "__Production_MachineStationDetails", 4.75f, -.48f);
        }

        private static void ArrangeGreenhouse(Transform root)
        {
            Transform[] beds = FindAll(root, "HydroponicBed").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            int half = Mathf.CeilToInt(beds.Length * .5f);
            for (int i = 0; i < beds.Length; i++)
            {
                int row = i < half ? 0 : 1;
                int col = row == 0 ? i : i - half;
                beds[i].localPosition = new Vector3(-5.0f + col * 2.5f, beds[i].localPosition.y, row == 0 ? -3.10f : -.34f);
            }
            SetPos(root, "ClimatePanel", 5.55f, -3.85f);
            SetPos(root, "ClimateScreen", 5.55f, -3.55f);
        }

        private static void ClearStairFootprint(Transform root, int index, float stairX, float zBack, float zFront)
        {
            Transform main = FindFirst(root, L.Cars[index].MainGroupName);
            if (main == null) return;

            foreach (Transform child in main.Cast<Transform>().ToArray())
            {
                Vector3 p = child.localPosition;
                if (Mathf.Abs(p.x - stairX) < 1.35f && p.z > zBack - .30f && p.z < zFront + .20f)
                {
                    p.x -= Mathf.Sign(stairX) * 2.35f;
                    if (index > 0 && p.z > -.15f) p.z = -.55f;
                    child.localPosition = p;
                }
            }
        }

        private static void SetPos(Transform root, string name, float x, float z)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            p.x = x;
            p.z = z;
            t.localPosition = p;
        }

        private static void MoveDetail(Transform root, string name, float x, float z)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            p.x = x;
            p.z = z;
            t.localPosition = p;
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
        };

        private static Material Mat(string name, Color fallback)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat");
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { name = name + "_Fallback", color = fallback };
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform Empty(Transform parent, string name, Vector3 pos)
        {
            Transform t = Group(parent, name);
            t.localPosition = pos;
            return t;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static void AddPoint(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = Empty(parent, name, pos);
            Light light = t.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = intensity;
            light.range = range;
            light.color = color;
            light.shadows = LightShadows.None;
        }

        private static void RemoveDirectChild(Transform root, string name)
        {
            Transform child = root.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static void DestroyNamedChild(Transform root, string name)
        {
            if (root == null) return;
            Transform child = root.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static void DestroyAllNamed(Transform root, string name)
        {
            foreach (Transform t in FindAll(root, name).ToArray())
                if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
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
    }
}
#endif