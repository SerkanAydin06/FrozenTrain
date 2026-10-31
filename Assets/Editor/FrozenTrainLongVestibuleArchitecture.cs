#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final room layout pass for the 24 m cars (historic class name kept for GUID stability).
    /// Cars 2-5 keep the straight +Z public corridor and one closed functional room on the left.
    /// Vertical access and end doors are owned by FrozenTrainBuilder ("CarAccess"); the shared
    /// connector is the builder's short main-deck gangway and is no longer rebuilt here.
    /// </summary>
    internal static class FrozenTrainLongVestibuleArchitecture
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
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
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
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
            if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        private static void DestroyNamedChild(Transform root, string name)
        {
            if (root == null) return;
            Transform t = root.Find(name);
            if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        private static void RemoveAllNamed(Transform root, params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t != null && t != root && set.Contains(t.name)) UnityEngine.Object.DestroyImmediate(t.gameObject);
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