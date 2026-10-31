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
    internal static class FrozenTrainMainStreetArchitecture
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__RightSideCorridorV2";
        private const string LegacyMarkerName = "__MainStreetArchitectureV1";

        public const float CorridorMinZ = 1.90f;
        public const float CorridorMaxZ = 4.40f;
        public const float CorridorCenterZ = (CorridorMinZ + CorridorMaxZ) * .5f;
        public const float CorridorWidth = CorridorMaxZ - CorridorMinZ;
        private const float PartitionZ = 1.78f;
        private const float DoorWidth = 1.45f;
        private const float DoorHeight = 2.34f;
        private const float MainFloorY = .126f;
        private const float CeilingY = 3.02f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Hazard;
            public Material Fabric;
            public Material Generator;
            public Material Workshop;
            public Material Green;
            public Material Screen;
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
                    RemoveDirectChild(root.transform, LegacyMarkerName);
                    RemoveDirectChild(root.transform, MarkerName);
                    RemoveLegacyAisleGeometry(root.transform);

                    Transform marker = Group(root.transform, MarkerName);
                    BuildRightSideCorridor(marker, i, mats);
                    RezoneFunctionalCompartment(root.transform, marker, i, mats);
                    KeepUpperDeckPrivate(root.transform, marker, i, mats);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildRightSideCorridor(Transform marker, int carIndex, Mats m)
        {
            Transform corridor = Group(marker, "RightSidePublicCorridor");
            Material accent = Accent(carIndex, m);

            Cube(corridor, "CorridorFloor", new Vector3(0f, MainFloorY, CorridorCenterZ),
                new Vector3(L.CarLength - .30f, .032f, CorridorWidth), m.Floor, false);
            Cube(corridor, "CorridorInnerEdge", new Vector3(0f, MainFloorY + .018f, CorridorMinZ),
                new Vector3(L.CarLength - .38f, .025f, .055f), accent, false);
            Cube(corridor, "CorridorOuterEdge", new Vector3(0f, MainFloorY + .018f, CorridorMaxZ),
                new Vector3(L.CarLength - .38f, .025f, .055f), accent, false);
            Cube(corridor, "CorridorCeilingSpine", new Vector3(0f, CeilingY - .11f, CorridorCenterZ),
                new Vector3(L.CarLength - .40f, .14f, .30f), m.Frame, false);

            foreach (float x in new[] { -6.0f, -2.0f, 2.0f, 6.0f })
            {
                Cube(corridor, "CorridorLuminaire", new Vector3(x, CeilingY - .16f, CorridorCenterZ),
                    new Vector3(2.15f, .055f, .22f), m.Warm, false);
                AddPoint(corridor, "CorridorFill", new Vector3(x, 2.55f, CorridorCenterZ),
                    .82f, 4.8f, new Color(.90f, .82f, .68f));
            }

            BuildOpaquePartition(corridor, carIndex, m);
            foreach (float x in new[] { -8.35f, 8.35f })
            {
                Cube(corridor, "CorridorPortalHeader", new Vector3(x, 2.64f, CorridorCenterZ),
                    new Vector3(.16f, .15f, CorridorWidth + .12f), m.Frame, false);
                Cube(corridor, "CorridorRoleBand", new Vector3(x - Mathf.Sign(x) * .10f, 2.31f, CorridorCenterZ),
                    new Vector3(.045f, .31f, CorridorWidth - .24f), accent, false);
            }
        }

        private static void BuildOpaquePartition(Transform corridor, int carIndex, Mats m)
        {
            float xMin = -L.HalfL + .22f, xMax = L.HalfL - .22f, halfDoor = DoorWidth * .5f;
            float wallY = L.MainTop + (CeilingY - L.MainTop) * .5f;
            float wallH = CeilingY - L.MainTop;

            Cube(corridor, "CompartmentWallFront", new Vector3((xMin - halfDoor) * .5f, wallY, PartitionZ),
                new Vector3((-halfDoor) - xMin, wallH, .16f), m.Wall, true);
            Cube(corridor, "CompartmentWallRear", new Vector3((halfDoor + xMax) * .5f, wallY, PartitionZ),
                new Vector3(xMax - halfDoor, wallH, .16f), m.Wall, true);
            Cube(corridor, "CompartmentDoorHeader", new Vector3(0f, L.MainTop + DoorHeight + (wallH - DoorHeight) * .5f, PartitionZ),
                new Vector3(DoorWidth, wallH - DoorHeight, .16f), m.Frame, true);

            Transform door = Group(corridor, "FunctionalCompartmentDoor");
            door.localPosition = new Vector3(0f, L.MainTop + DoorHeight * .5f, PartitionZ);
            // Leave a consistent 1cm reveal all around the frame.
            Cube(door, "DoorPanel", Vector3.zero,
                new Vector3(DoorWidth - .02f, DoorHeight - .02f, .085f), m.Frame, false);
            Cube(door, "DoorAccent", new Vector3(0f, .52f, -.052f), new Vector3(DoorWidth - .26f, .15f, .025f), Accent(carIndex, m), false);
            Cube(door, "DoorKick", new Vector3(0f, -.78f, -.052f), new Vector3(DoorWidth - .22f, .34f, .026f), m.Steel, false);
            Cube(door, "DoorHandle", new Vector3(.48f, -.04f, -.072f), new Vector3(.07f, .34f, .05f), m.Hazard, false);
            BoxCollider hit = door.gameObject.AddComponent<BoxCollider>();
            hit.size = new Vector3(DoorWidth - .02f, DoorHeight - .02f, .11f);
            door.gameObject.AddComponent<SlidingDoor>().Configure(new Vector3(DoorWidth + .02f, 0f, 0f), 2.6f);
        }

        private static void RezoneFunctionalCompartment(Transform root, Transform marker, int index, Mats m)
        {
            Transform zone = Group(marker, "ClosedFunctionalCompartment");
            Material accent = Accent(index, m);
            float leftWidth = PartitionZ - (-L.HalfW + .20f);
            float leftCenter = (-L.HalfW + .20f + PartitionZ) * .5f;
            Cube(zone, "FunctionalFloorInset", new Vector3(0f, MainFloorY + .010f, leftCenter),
                new Vector3(L.CarLength - .48f, .018f, leftWidth - .14f), m.Floor, false);
            Cube(zone, "FunctionalZoneBand", new Vector3(0f, MainFloorY + .026f, PartitionZ - .20f),
                new Vector3(L.CarLength - .80f, .020f, .055f), accent, false);

            switch (index)
            {
                case 0: RezoneLocomotive(root, zone, m); break;
                case 1: RezoneGenerator(root, zone, m); break;
                case 2: RezoneLiving(root, zone, m); break;
                case 3: RezoneWorkshop(root, zone, m); break;
                case 4: RezoneGreenhouse(root, zone, m); break;
            }

            foreach (float x in new[] { -5.4f, 0f, 5.4f })
                AddPoint(zone, "FunctionalRoomLight", new Vector3(x, 2.48f, -.95f), .88f, 5.4f,
                    index == 4 ? new Color(.68f, .90f, .70f) : new Color(.78f, .76f, .70f));
        }

        private static void RezoneLocomotive(Transform root, Transform zone, Mats m)
        {
            Transform console = FindFirst(root, "MainConsole");
            if (console != null) console.localPosition = new Vector3(-6.30f, console.localPosition.y, -1.65f);

            Transform[] seats = FindAll(root, "DriverSeat").OrderBy(t => t.localPosition.z).ToArray();
            if (seats.Length > 0) seats[0].localPosition = new Vector3(-4.25f, seats[0].localPosition.y, -2.75f);
            if (seats.Length > 1) seats[1].localPosition = new Vector3(-4.25f, seats[1].localPosition.y, -.70f);

            Transform[] cabinets = FindAll(root, "EquipmentCabinet").OrderBy(t => t.localPosition.x).ToArray();
            float[] xs = { .8f, 3.8f, 6.4f };
            for (int i = 0; i < cabinets.Length && i < xs.Length; i++)
                cabinets[i].localPosition = new Vector3(xs[i], cabinets[i].localPosition.y, -3.95f);

            Transform terminal = FindFirst(root, "ManagementTerminal");
            if (terminal != null) terminal.localPosition = new Vector3(6.15f, terminal.localPosition.y, -.45f);
        }

        private static void RezoneGenerator(Transform root, Transform zone, Mats m)
        {
            SetPos(root, "OperatorDesk", -3.25f, -3.72f);
            SetPos(root, "GeneratorConsole", -3.25f, -4.02f);
            SetPos(root, "ControlTable", 2.55f, -3.65f);
            SetPos(root, "MonitoringRack", 4.50f, -.35f);
        }

        private static void RezoneLiving(Transform root, Transform zone, Mats m)
        {
            Transform[] bays = FindAll(root, "SeatingBay").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            Vector3[] targets = {
                new Vector3(-4.35f, L.MainTop, -3.20f), new Vector3(-4.35f, L.MainTop, -.55f),
                new Vector3(.35f, L.MainTop, -3.20f), new Vector3(.35f, L.MainTop, -.55f)
            };
            for (int i = 0; i < bays.Length && i < targets.Length; i++) bays[i].localPosition = targets[i];
            SetPos(root, "GalleyCounter", 5.25f, -3.82f);
            SetPos(root, "GalleyTop", 5.25f, -3.82f);
            MoveDetail(root, "__Production_GalleyDetails", 5.25f, -3.82f);
            Transform med = FindFirst(root, "MedicalCabinet");
            if (med != null) { med.localPosition = new Vector3(6.70f, med.localPosition.y, -.55f); med.localRotation = Quaternion.identity; }
        }

        private static void RezoneWorkshop(Transform root, Transform zone, Mats m)
        {
            Transform[] benches = FindAll(root, "Workbench").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] targets = {
                new Vector3(-4.30f, L.MainTop, -3.75f), new Vector3(1.10f, L.MainTop, -3.75f),
                new Vector3(-1.20f, L.MainTop, -.35f)
            };
            for (int i = 0; i < benches.Length && i < targets.Length; i++) benches[i].localPosition = targets[i];
            SetPos(root, "ToolCabinet", 6.10f, -3.90f);
            SetPos(root, "MachineStation", 5.10f, -.55f);
            SetPos(root, "MachineScreen", 5.10f, -1.25f);
            MoveDetail(root, "__Production_MachineStationDetails", 5.10f, -.55f);
        }

        private static void RezoneGreenhouse(Transform root, Transform zone, Mats m)
        {
            Transform[] beds = FindAll(root, "HydroponicBed").OrderBy(t => t.localPosition.x).ThenBy(t => t.localPosition.z).ToArray();
            int half = Mathf.CeilToInt(beds.Length * .5f);
            for (int i = 0; i < beds.Length; i++)
            {
                int row = i < half ? 0 : 1;
                int col = row == 0 ? i : i - half;
                beds[i].localPosition = new Vector3(-5.0f + col * 2.5f, beds[i].localPosition.y, row == 0 ? -3.10f : -.35f);
            }
            SetPos(root, "ClimatePanel", 6.55f, -3.85f);
            SetPos(root, "ClimateScreen", 6.55f, -3.55f);
        }

        private static void KeepUpperDeckPrivate(Transform root, Transform marker, int carIndex, Mats m)
        {
            if (FindFirst(root, "UpperSleepingDeck") == null) return;
            Transform residential = Group(marker, "ResidentialIdentity");
            Material accent = Accent(carIndex, m);
            foreach (float x in new[] { -8.70f, 8.70f })
            {
                Cube(residential, "ResidentialEndBulkhead", new Vector3(x, 4.72f, 0f), new Vector3(.12f, 2.68f, 2.55f), m.Wall, true);
                Cube(residential, "ResidentialPrivacyBand", new Vector3(x - Mathf.Sign(x) * .07f, 4.88f, 0f), new Vector3(.025f, .28f, 1.90f), accent, false);
            }
        }

        private static void RemoveLegacyAisleGeometry(Transform root)
        {
            string[] names = { "MainAisleGuide", "MainAisleEdgeL", "MainAisleEdgeR", "StreetRunner", "StreetEdgeL", "StreetEdgeR", "StreetCeilingSpine", "SafetyGlass", "FrontageBottomRail", "FrontageTopRail", "FrontageDoorPost", "FrontageDoorHeader", "LivingPortalPost", "LivingPortalLamp", "WorkshopKick" };
            foreach (string n in names)
                foreach (Transform t in FindAll(root, n).ToArray())
                    if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        private static Mats LoadMaterials() => new Mats {
            Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)), Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
            Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)), Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
            Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)), Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
            Generator = Mat("FT_PRO_Generator", new Color(.34f, .105f, .028f)), Workshop = Mat("FT_PRO_Workshop", new Color(.34f, .245f, .075f)),
            Green = Mat("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f)), Screen = Mat("FT_PRO_Screen", new Color(.03f, .45f, .60f)),
            Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f)) };

        private static Material Accent(int index, Mats m) { switch (index) { case 0: return m.Hazard; case 1: return m.Generator; case 2: return m.Fabric; case 3: return m.Workshop; default: return m.Green; } }
        private static Material Mat(string name, Color fallback) { Material m = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat"); if (m != null) return m; Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); return new Material(s) { name = name + "_Fallback", color = fallback }; }
        private static void SetPos(Transform root, string name, float x, float z) { Transform t = FindFirst(root, name); if (t == null) return; Vector3 p = t.localPosition; p.x = x; p.z = z; t.localPosition = p; }
        private static void MoveDetail(Transform root, string name, float x, float z) { Transform t = FindFirst(root, name); if (t == null) return; Vector3 p = t.localPosition; p.x = x; p.z = z; t.localPosition = p; }
        private static Transform Group(Transform parent, string name) { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider) { GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = size; Renderer r = go.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = mat; if (!collider) { Collider c = go.GetComponent<Collider>(); if (c != null) UnityEngine.Object.DestroyImmediate(c); } return go; }
        private static void AddPoint(Transform parent, string name, Vector3 pos, float intensity, float range, Color color) { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos; Light l = go.AddComponent<Light>(); l.type = LightType.Point; l.intensity = intensity; l.range = range; l.color = color; l.shadows = LightShadows.None; }
        private static void RemoveDirectChild(Transform root, string name) { Transform t = root.Find(name); if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject); }
        private static Transform FindFirst(Transform root, string name) { if (root == null) return null; if (root.name == name) return root; foreach (Transform child in root) { Transform f = FindFirst(child, name); if (f != null) return f; } return null; }
        private static IEnumerable<Transform> FindAll(Transform root, string name) { if (root == null) yield break; if (root.name == name) yield return root; foreach (Transform child in root) foreach (Transform f in FindAll(child, name)) yield return f; }
    }
}
#endif
