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
    /// Final circulation authority after all legacy visual passes.
    /// - Locomotive main deck is one full private cab with no passenger corridor.
    /// - Cars 2-5 keep the right-side passenger corridor and one central work-room door.
    /// - Exposed upper ladders/hatches are removed and replaced by an enclosed residential stairwell.
    /// - Legacy main-deck QA decorations that intrude into the public corridor are removed.
    /// </summary>
    internal static class FrozenTrainCirculationFinalizer
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__CirculationFinalV1";

        private const float PartitionZ = 1.78f;
        private const float CorridorMinZ = 1.90f;
        private const float CorridorCenterZ = 3.15f;
        private const float WorkDoorWidth = 1.45f;
        private const float StairDoorWidth = 1.22f;
        private const float DoorHeight = 2.30f;
        private const float StairInnerU = 6.35f;
        private const float StairOuterU = 8.58f;
        private const float StairDoorU = 7.45f;
        private const float ShaftZMin = -1.58f;
        private const float ShaftZMax = 1.58f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Hazard;
            public Material Fabric;
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
                    RemoveDirectChild(root.transform, MarkerName);
                    Transform marker = Group(root.transform, MarkerName);

                    RemoveLegacyMainDeckClutter(root.transform);
                    RemoveExposedUpperAccess(root.transform);
                    CloseOldUpperHatches(marker, i, mats);

                    if (i == 0)
                        FinalizeLocomotive(root.transform, marker, mats);
                    else
                        FinalizePassengerCar(root.transform, marker, i, mats);

                    BuildResidentialStairwell(root.transform, marker, i, mats);
                    CleanKnownCorridorIntrusions(root.transform, i);

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
            // Locomotive is a destination, not a through-route. Remove the right-side corridor and work-room wall.
            Transform architecture = root.Find("__RightSideCorridorV2");
            if (architecture != null)
            {
                DestroyNamedChild(architecture, "RightSidePublicCorridor");
                DestroyNamedChild(architecture, "ClosedFunctionalCompartment");
            }

            Transform console = FindFirst(root, "MainConsole");
            if (console != null) console.localPosition = new Vector3(-6.30f, console.localPosition.y, 0f);

            Transform[] seats = FindAll(root, "DriverSeat").OrderBy(t => t.localPosition.z).ToArray();
            if (seats.Length > 0) seats[0].localPosition = new Vector3(-4.25f, seats[0].localPosition.y, -1.30f);
            if (seats.Length > 1) seats[1].localPosition = new Vector3(-4.25f, seats[1].localPosition.y, 1.30f);

            Transform[] cabinets = FindAll(root, "EquipmentCabinet").OrderBy(t => t.localPosition.x).ToArray();
            Vector3[] cabinetTargets =
            {
                new Vector3(1.10f, 0f, -3.92f),
                new Vector3(4.35f, 0f, -3.92f),
                new Vector3(2.55f, 0f, 3.92f),
            };
            for (int i = 0; i < cabinets.Length && i < cabinetTargets.Length; i++)
            {
                Vector3 p = cabinets[i].localPosition;
                p.x = cabinetTargets[i].x;
                p.z = cabinetTargets[i].z;
                cabinets[i].localPosition = p;
            }

            Transform terminal = FindFirst(root, "ManagementTerminal");
            if (terminal != null) terminal.localPosition = new Vector3(6.05f, terminal.localPosition.y, 3.55f);

            Transform cab = Group(marker, "LocomotivePrivateCab");
            Cube(cab, "CabFloorIdentity", new Vector3(0f, L.MainTop + .015f, 0f),
                new Vector3(L.CarLength - .50f, .024f, L.CarWidth - .55f), m.Floor, false);
            Cube(cab, "RearPrivateLanding", new Vector3(7.65f, L.MainTop + .032f, 3.15f),
                new Vector3(1.75f, .028f, 2.35f), m.Fabric, false);
            AddPoint(cab, "CabRearFill", new Vector3(6.6f, 2.45f, 2.2f), .82f, 4.8f, new Color(.72f, .82f, .94f));
        }

        private static void FinalizePassengerCar(Transform root, Transform marker, int index, Mats m)
        {
            Transform architecture = root.Find("__RightSideCorridorV2");
            Transform corridor = architecture != null ? FindFirst(architecture, "RightSidePublicCorridor") : null;
            if (corridor == null) return;

            // Rebuild the opaque partition with exactly two distinct access points:
            // central staff/work-room door + end residential stairwell door.
            foreach (string n in new[] { "CompartmentWallFront", "CompartmentWallRear", "CompartmentDoorHeader", "CompartmentWall" })
                DestroyAllNamed(corridor, n);

            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float stairX = s * StairDoorU;
            BuildPartitionWithOpenings(corridor, stairX, m);

            // The staff/work door produced by MainStreetArchitecture stays centred and remains the only
            // entrance into the work compartment. The second door only enters the sealed stair tower.
            BuildLowerStairDoor(marker, stairX, m);

            // Reserve the stair-tower footprint inside the left zone. No work prop may occupy it.
            ClearStairFootprint(root, index, stairX);

            if (index == 2)
            {
                // The medical cabinet used to sit inside the new stair footprint.
                Transform med = FindFirst(root, "MedicalCabinet");
                if (med != null)
                {
                    Vector3 p = med.localPosition;
                    p.x = 4.65f;
                    p.z = -.55f;
                    med.localPosition = p;
                }
            }
        }

        private static void BuildPartitionWithOpenings(Transform parent, float stairX, Mats m)
        {
            const float xMin = -8.78f, xMax = 8.78f;
            float wallY = L.MainTop + (3.02f - L.MainTop) * .5f;
            float wallH = 3.02f - L.MainTop;

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

                float headerHeight = Mathf.Max(.12f, wallH - DoorHeight);
                Cube(parent, "CompartmentDoorHeader", new Vector3(opening.x, L.MainTop + DoorHeight + headerHeight * .5f, PartitionZ),
                    new Vector3(opening.y, headerHeight, .16f), m.Frame, true);
                cursor = Mathf.Max(cursor, right);
            }

            if (cursor < xMax - .03f)
                Cube(parent, "CompartmentWall", new Vector3((cursor + xMax) * .5f, wallY, PartitionZ),
                    new Vector3(xMax - cursor, wallH, .16f), m.Wall, true);
        }

        // ---- Residential stair tower --------------------------------------------------------

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

        private static void BuildResidentialStairwell(Transform root, Transform marker, int index, Mats m)
        {
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float innerX = s * StairInnerU;
            float outerX = s * StairOuterU;
            float xMin = Mathf.Min(innerX, outerX), xMax = Mathf.Max(innerX, outerX);
            float xCentre = (xMin + xMax) * .5f;
            float xLength = xMax - xMin;
            float stairX = s * StairDoorU;

            Transform shaft = Group(marker, "ResidentialStairwell");
            float shaftBottom = L.MainTop;
            float shaftTop = L.RoofBottom;
            float shaftHeight = shaftTop - shaftBottom;
            float shaftY = shaftBottom + shaftHeight * .5f;

            // Opaque vertical shaft: residents see proper doors, not exposed wall ladders or floor holes.
            Cube(shaft, "ShaftBackWall", new Vector3(xCentre, shaftY, ShaftZMin),
                new Vector3(xLength, shaftHeight, .14f), m.Wall, true);
            BuildFrontShaftWall(shaft, stairX, xMin, xMax, shaftBottom, shaftTop, m);
            BuildUpperShaftBulkhead(shaft, innerX, m);

            // Subtle internal steps communicate a real stair tower while floor-to-floor travel remains
            // interaction-driven so the compact shaft never creates FPS head-clearance or collision traps.
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float z = Mathf.Lerp(1.18f, -.95f, t);
                float y = L.MainTop + .16f + i * .19f;
                Cube(shaft, "StairStep", new Vector3(stairX, y, z),
                    new Vector3(1.02f, .18f, .34f), m.Steel, true);
            }
            Cube(shaft, "StairHandrail", new Vector3(stairX - s * .56f, 1.24f, .10f),
                new Vector3(.06f, 1.92f, 2.45f), m.Frame, false);

            Transform lowerAnchor = Empty(shaft, "LowerLanding",
                index == 0
                    ? new Vector3(s * 5.80f, L.MainTop + .055f, .45f)
                    : new Vector3(stairX, L.MainTop + .055f, CorridorCenterZ));
            lowerAnchor.localRotation = Quaternion.Euler(0f, index == 0 ? (s > 0f ? 180f : 0f) : 180f, 0f);

            Transform upperAnchor = Empty(shaft, "UpperLanding", new Vector3(s * 5.82f, L.UpperTop + .055f, 0f));
            upperAnchor.localRotation = Quaternion.Euler(0f, s > 0f ? -90f : 90f, 0f);

            if (index == 0)
                BuildLocomotiveLowerStairDoor(shaft, stairX, lowerAnchor, upperAnchor, m);
            else
                ConfigureLowerStairDoor(marker, lowerAnchor, upperAnchor);

            BuildUpperStairDoor(shaft, s, innerX, lowerAnchor, upperAnchor, m);

            Cube(shaft, "LowerStairSign", new Vector3(stairX, 2.60f, ShaftZMax + .09f),
                new Vector3(.88f, .24f, .035f), m.Hazard, false);
            Cube(shaft, "UpperStairSign", new Vector3(innerX - s * .09f, L.UpperTop + 1.72f, 0f),
                new Vector3(.035f, .24f, .88f), m.Hazard, false);
        }

        private static void BuildFrontShaftWall(Transform shaft, float stairX, float xMin, float xMax, float bottom, float top, Mats m)
        {
            float half = StairDoorWidth * .5f;
            float left = stairX - half, right = stairX + half;
            float y = (bottom + top) * .5f;
            float height = top - bottom;

            if (left > xMin)
                Cube(shaft, "ShaftFrontWall", new Vector3((xMin + left) * .5f, y, ShaftZMax),
                    new Vector3(left - xMin, height, .14f), m.Wall, true);
            if (right < xMax)
                Cube(shaft, "ShaftFrontWall", new Vector3((right + xMax) * .5f, y, ShaftZMax),
                    new Vector3(xMax - right, height, .14f), m.Wall, true);

            float doorTop = L.MainTop + DoorHeight;
            float headerH = .58f;
            Cube(shaft, "ShaftLowerDoorHeader", new Vector3(stairX, doorTop + headerH * .5f, ShaftZMax),
                new Vector3(StairDoorWidth, headerH, .14f), m.Frame, true);

            float sealedAboveBottom = doorTop + headerH;
            if (top > sealedAboveBottom + .05f)
                Cube(shaft, "ShaftFrontUpperWall", new Vector3(stairX, (sealedAboveBottom + top) * .5f, ShaftZMax),
                    new Vector3(StairDoorWidth, top - sealedAboveBottom, .14f), m.Wall, true);
        }

        private static void BuildUpperShaftBulkhead(Transform shaft, float x, Mats m)
        {
            float zHalf = .62f;
            float y = L.UpperTop + (L.RoofBottom - L.UpperTop) * .5f;
            float h = L.RoofBottom - L.UpperTop;

            float negWidth = (-zHalf) - ShaftZMin;
            float posWidth = ShaftZMax - zHalf;
            Cube(shaft, "UpperShaftWall", new Vector3(x, y, ShaftZMin + negWidth * .5f),
                new Vector3(.14f, h, negWidth), m.Wall, true);
            Cube(shaft, "UpperShaftWall", new Vector3(x, y, zHalf + posWidth * .5f),
                new Vector3(.14f, h, posWidth), m.Wall, true);
            Cube(shaft, "UpperShaftHeader", new Vector3(x, L.UpperTop + 2.30f + .28f, 0f),
                new Vector3(.14f, .56f, 1.24f), m.Frame, true);
        }

        private static void BuildLowerStairDoor(Transform marker, float stairX, Mats m)
        {
            Transform door = Group(marker, "ResidentialAccessDoor");
            door.localPosition = new Vector3(stairX, L.MainTop + DoorHeight * .5f, PartitionZ - .025f);
            Cube(door, "DoorPanel", Vector3.zero, new Vector3(StairDoorWidth - .07f, DoorHeight - .06f, .09f), m.Frame, false);
            Cube(door, "DoorBand", new Vector3(0f, .46f, -.052f), new Vector3(.88f, .14f, .025f), m.Hazard, false);
            Cube(door, "DoorHandle", new Vector3(.40f, -.05f, -.065f), new Vector3(.06f, .30f, .045f), m.Steel, false);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(StairDoorWidth, DoorHeight, .22f);
        }

        private static void ConfigureLowerStairDoor(Transform marker, Transform lower, Transform upper)
        {
            Transform door = FindFirst(marker, "ResidentialAccessDoor");
            if (door == null) return;
            DeckLadder stairs = door.gameObject.AddComponent<DeckLadder>();
            stairs.Configure(lower, upper, "E - Konut katına çık", "E - Ana koridora in");
        }

        private static void BuildLocomotiveLowerStairDoor(Transform shaft, float stairX, Transform lower, Transform upper, Mats m)
        {
            Transform door = Group(shaft, "LocomotiveResidentialAccessDoor");
            door.localPosition = new Vector3(stairX, L.MainTop + DoorHeight * .5f, ShaftZMax - .03f);
            Cube(door, "DoorPanel", Vector3.zero, new Vector3(StairDoorWidth - .07f, DoorHeight - .06f, .09f), m.Frame, false);
            Cube(door, "DoorBand", new Vector3(0f, .46f, -.052f), new Vector3(.88f, .14f, .025f), m.Hazard, false);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(StairDoorWidth, DoorHeight, .22f);
            door.gameObject.AddComponent<DeckLadder>().Configure(lower, upper, "E - Mürettebat katına çık", "E - Lokomotife in");
        }

        private static void BuildUpperStairDoor(Transform shaft, float s, float innerX, Transform lower, Transform upper, Mats m)
        {
            Transform door = Group(shaft, "UpperResidentialStairDoor");
            door.localPosition = new Vector3(innerX - s * .025f, L.UpperTop + 1.15f, 0f);
            Cube(door, "DoorPanel", Vector3.zero, new Vector3(.09f, 2.22f, 1.16f), m.Frame, false);
            Cube(door, "DoorBand", new Vector3(-s * .052f, .45f, 0f), new Vector3(.025f, .14f, .84f), m.Hazard, false);
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(.22f, 2.28f, 1.20f);
            door.gameObject.AddComponent<DeckLadder>().Configure(lower, upper, "E - Konut katına çık", "E - Ana kata in");
        }

        // ---- Cleanup ------------------------------------------------------------------------

        private static void RemoveLegacyMainDeckClutter(Transform root)
        {
            Transform qa = root.Find("__ScreenshotQAV1");
            if (qa != null) DestroyNamedChild(qa, "MainDeckQA");

            // Old art roots were authored for a centre-aisle train and can cross the new right corridor.
            foreach (string n in new[] { "LivingWallFurniture", "GreenhouseOverhead" })
                DestroyAllNamed(root, n);
        }

        private static void CleanKnownCorridorIntrusions(Transform root, int index)
        {
            if (index == 0) return;

            // Anything in the canonical main content group that still sits in the public envelope is moved
            // back into the closed room. Interaction components stay on the same root.
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

        private static void ClearStairFootprint(Transform root, int index, float stairX)
        {
            Transform main = FindFirst(root, L.Cars[index].MainGroupName);
            if (main == null) return;

            foreach (Transform child in main.Cast<Transform>().ToArray())
            {
                Vector3 p = child.localPosition;
                if (Mathf.Abs(p.x - stairX) < 1.25f && p.z > ShaftZMin - .45f && p.z < PartitionZ)
                {
                    p.x -= Mathf.Sign(stairX) * 2.20f;
                    p.z = p.z > -.2f ? -.55f : p.z;
                    child.localPosition = p;
                }
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
