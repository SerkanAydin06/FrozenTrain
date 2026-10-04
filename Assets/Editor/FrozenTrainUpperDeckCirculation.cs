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
    /// Final upper-deck circulation pass. The existing builder keeps ownership of the car shell and hatch
    /// openings; this pass replaces the sleeping-room geometry so both ladders arrive in one shared
    /// vestibule, the centre corridor remains public from car to car, and private cabins only open onto it.
    /// </summary>
    internal static class FrozenTrainUpperDeckCirculation
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const float CorridorHalf = 1.20f;
        private const float DoorWidth = 1.08f;
        private const float DoorHeight = 2.08f;
        private const float RoomZoneMinU = -8.45f;
        private const float RoomZoneMaxU = 5.55f;
        private const float RoomDividerU = -1.25f;
        private const float VestibuleExitU = 5.92f;
        private const float VestibuleInnerU = 5.55f;
        private const float VestibuleOuterU = 8.72f;
        private const float LandingExitZ = .62f;

        private static readonly float[] RoomDoorU = { -4.85f, 2.15f };
        private static readonly float[] RoomCentreU = { -4.85f, 2.15f };

        private sealed class Mats
        {
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Fabric;
            public Material Bedding;
            public Material Table;
            public Material Hazard;
            public Material Warm;
            public Material Floor;
        }

        public static void Apply()
        {
            Mats mats = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                RebuildCar(path, i, mats);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RebuildCar(string path, int index, Mats m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;

                Transform oldUpper = FindFirst(root.transform, "UpperSleepingDeck");
                if (oldUpper != null) UnityEngine.Object.DestroyImmediate(oldUpper.gameObject);

                Transform productionMarker = root.transform.Find("__ProductionArtV2");
                if (productionMarker != null)
                {
                    Transform oldAccommodation = productionMarker.Find("UpperAccommodationDetails");
                    if (oldAccommodation != null) UnityEngine.Object.DestroyImmediate(oldAccommodation.gameObject);
                }

                Transform upper = NewGroup(root.transform, "UpperSleepingDeck", Vector3.zero);
                BuildPublicCorridor(upper, s, m);
                BuildCabins(upper, s, index, m);
                BuildSharedVestibule(upper, s, m);
                RetargetLadders(root.transform, s);

                Transform marker = root.transform.Find("__UpperCirculationV3");
                if (marker != null) UnityEngine.Object.DestroyImmediate(marker.gameObject);
                marker = NewGroup(root.transform, "__UpperCirculationV3", Vector3.zero);
                BuildWayfinding(marker, s, m);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildPublicCorridor(Transform upper, float s, Mats m)
        {
            float floorY = L.UpperTop + .012f;
            float ceiling = L.RoofBottom;
            float height = ceiling - L.UpperTop;
            float cy = L.UpperTop + height * .5f;

            Cube(upper, "PublicCorridorRunner", new Vector3(0f, floorY, 0f),
                new Vector3(L.CarLength - .65f, .025f, CorridorHalf * 2f - .18f), m.Floor, false);

            float xA = s * RoomZoneMinU;
            float xB = s * RoomZoneMaxU;
            float xMin = Mathf.Min(xA, xB);
            float xMax = Mathf.Max(xA, xB);
            float[] doors = RoomDoorU.Select(u => s * u).OrderBy(v => v).ToArray();

            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * CorridorHalf;
                BuildCorridorWall(upper, z, xMin, xMax, cy, height, doors, side, s, m);
            }

            Cube(upper, "CorridorCeilingSpine", new Vector3(0f, ceiling - .08f, 0f),
                new Vector3(L.CarLength - .65f, .12f, .32f), m.Frame, false);

            foreach (float x in new[] { -6.4f, -2.2f, 2.2f, 6.4f })
            {
                Cube(upper, "CorridorNightLightL", new Vector3(x, L.UpperTop + .30f, -CorridorHalf + .06f),
                    new Vector3(.30f, .11f, .055f), m.Warm, false);
                Cube(upper, "CorridorNightLightR", new Vector3(x, L.UpperTop + .30f, CorridorHalf - .06f),
                    new Vector3(.30f, .11f, .055f), m.Warm, false);
            }
        }

        private static void BuildCorridorWall(
            Transform parent, float z, float xMin, float xMax, float cy, float height,
            float[] doorCenters, float side, float s, Mats m)
        {
            float halfDoor = DoorWidth * .5f;
            float cursor = xMin;
            foreach (float door in doorCenters)
            {
                float left = door - halfDoor;
                float right = door + halfDoor;
                if (left > cursor)
                    Cube(parent, "CabinCorridorWall", new Vector3((cursor + left) * .5f, cy, z),
                        new Vector3(left - cursor, height, .12f), m.Wall, true);
                cursor = Mathf.Max(cursor, right);

                Cube(parent, "CabinDoorJambA", new Vector3(left, L.UpperTop + DoorHeight * .5f, z),
                    new Vector3(.08f, DoorHeight, .16f), m.Frame, true);
                Cube(parent, "CabinDoorJambB", new Vector3(right, L.UpperTop + DoorHeight * .5f, z),
                    new Vector3(.08f, DoorHeight, .16f), m.Frame, true);
                Cube(parent, "CabinDoorHeader", new Vector3(door, L.UpperTop + DoorHeight + .06f, z),
                    new Vector3(DoorWidth + .12f, .12f, .16f), m.Frame, true);

                float leafOffset = door + s * .78f;
                Cube(parent, "PocketDoorOpen", new Vector3(leafOffset, L.UpperTop + .99f, z + side * .055f),
                    new Vector3(.62f, 1.94f, .055f), m.Wall, false);
                Cube(parent, "DoorHandle", new Vector3(door + s * .43f, L.UpperTop + 1.02f, z - side * .105f),
                    new Vector3(.07f, .22f, .055f), m.Hazard, false);
            }

            if (cursor < xMax)
                Cube(parent, "CabinCorridorWall", new Vector3((cursor + xMax) * .5f, cy, z),
                    new Vector3(xMax - cursor, height, .12f), m.Wall, true);
        }

        private static void BuildCabins(Transform upper, float s, int carIndex, Mats m)
        {
            float roomDepth = L.HalfW - CorridorHalf - .18f;
            float zCentre = CorridorHalf + roomDepth * .5f;
            float height = L.RoofBottom - L.UpperTop;
            float cy = L.UpperTop + height * .5f;

            float dividerX = s * RoomDividerU;
            float vestibuleWallX = s * VestibuleInnerU;

            foreach (float side in new[] { -1f, 1f })
            {
                Cube(upper, "CabinDivider", new Vector3(dividerX, cy, side * zCentre),
                    new Vector3(.12f, height, roomDepth), m.Wall, true);
                Cube(upper, "VestibuleCabinBulkhead", new Vector3(vestibuleWallX, cy, side * zCentre),
                    new Vector3(.12f, height, roomDepth), m.Wall, true);
            }

            for (int room = 0; room < RoomCentreU.Length; room++)
            {
                float x = s * RoomCentreU[room];
                foreach (float side in new[] { -1f, 1f })
                    BuildCabinInterior(upper, x, side, s, carIndex, room, m);
            }
        }

        private static void BuildCabinInterior(Transform upper, float x, float side, float s, int carIndex, int roomIndex, Mats m)
        {
            Transform room = NewGroup(upper, $"Cabin_{carIndex + 1}_{(side < 0f ? "L" : "R")}{roomIndex + 1}", Vector3.zero);
            float outerZ = side * 3.48f;

            Cube(room, "BedFrame", new Vector3(x - s * .42f, L.UpperTop + .22f, outerZ),
                new Vector3(2.34f, .42f, .94f), m.Frame, true);
            Cube(room, "Mattress", new Vector3(x - s * .42f, L.UpperTop + .50f, outerZ),
                new Vector3(2.18f, .14f, .80f), m.Bedding, true);
            Cube(room, "Pillow", new Vector3(x - s * 1.10f, L.UpperTop + .64f, outerZ),
                new Vector3(.46f, .13f, .61f), m.Bedding, false);
            Cube(room, "FoldedBlanket", new Vector3(x + s * .38f, L.UpperTop + .63f, outerZ),
                new Vector3(.70f, .09f, .70f), m.Fabric, false);

            float lockerX = x + s * 2.05f;
            Cube(room, "Locker", new Vector3(lockerX, L.UpperTop + .90f, side * 4.10f),
                new Vector3(.68f, 1.80f, .58f), m.Steel, true);
            Cube(room, "LockerHandle", new Vector3(lockerX - s * .18f, L.UpperTop + .94f, side * 3.79f),
                new Vector3(.05f, .24f, .05f), m.Hazard, false);

            float deskX = x + s * .78f;
            Cube(room, "WallDesk", new Vector3(deskX, L.UpperTop + .76f, side * 4.18f),
                new Vector3(1.20f, .08f, .52f), m.Table, true);
            Cube(room, "WallShelf", new Vector3(x - s * .70f, L.UpperTop + 1.48f, side * 4.25f),
                new Vector3(.90f, .08f, .28f), m.Table, false);
            Cube(room, "ReadingLight", new Vector3(x - s * 1.20f, L.UpperTop + 1.56f, side * 4.08f),
                new Vector3(.24f, .10f, .16f), m.Warm, false);

            // One low luggage box per cabin adds use without blocking the door or public corridor.
            Cube(room, "UnderBedStorage", new Vector3(x + s * .20f, L.UpperTop + .14f, outerZ),
                new Vector3(1.20f, .20f, .76f), m.Steel, false);
        }

        private static void BuildSharedVestibule(Transform upper, float s, Mats m)
        {
            float uCentre = (VestibuleInnerU + VestibuleOuterU) * .5f;
            float length = VestibuleOuterU - VestibuleInnerU;
            float x = s * uCentre;

            Cube(upper, "SharedVestibuleFloorMark", new Vector3(x, L.UpperTop + .018f, 0f),
                new Vector3(length, .028f, L.CarWidth - .55f), m.Floor, false);
            Cube(upper, "VestibuleThreshold", new Vector3(s * VestibuleInnerU, L.UpperTop + .035f, 0f),
                new Vector3(.10f, .05f, L.CarWidth - .50f), m.Hazard, false);
            Cube(upper, "VestibuleHeader", new Vector3(s * VestibuleInnerU, L.RoofBottom - .16f, 0f),
                new Vector3(.16f, .24f, L.CarWidth - .55f), m.Frame, false);

            foreach (float side in new[] { -1f, 1f })
                BuildHatchGuard(upper, s, side, m);
        }

        private static void BuildHatchGuard(Transform upper, float s, float side, Mats m)
        {
            float hx0 = s * L.UpperHoleX0;
            float hx1 = s * L.UpperHoleX1;
            float xMin = Mathf.Min(hx0, hx1);
            float xMax = Mathf.Max(hx0, hx1);
            float zMin = side > 0f ? L.UpperHoleZ0 : -L.UpperHoleZ1;
            float zMax = side > 0f ? L.UpperHoleZ1 : -L.UpperHoleZ0;
            float xC = (xMin + xMax) * .5f;
            float zC = (zMin + zMax) * .5f;
            float y = L.UpperTop + .52f;

            // Three-sided guard: the centre-facing edge stays open as the deliberate ladder approach.
            RailX(upper, "HatchGuardOuterX", xMin, xMax, y, side > 0f ? zMax : zMin, m);
            RailZ(upper, "HatchGuardEndA", xMin, y, zMin, zMax, m);
            RailZ(upper, "HatchGuardEndB", xMax, y, zMin, zMax, m);

            Cube(upper, "HatchHazardX", new Vector3(xC, L.UpperTop + .025f, side > 0f ? zMax : zMin),
                new Vector3(xMax - xMin, .04f, .08f), m.Hazard, false);
            Cube(upper, "HatchHazardA", new Vector3(xMin, L.UpperTop + .025f, zC),
                new Vector3(.08f, .04f, zMax - zMin), m.Hazard, false);
            Cube(upper, "HatchHazardB", new Vector3(xMax, L.UpperTop + .025f, zC),
                new Vector3(.08f, .04f, zMax - zMin), m.Hazard, false);
        }

        private static void RailX(Transform p, string name, float x0, float x1, float y, float z, Mats m)
        {
            Cube(p, name, new Vector3((x0 + x1) * .5f, y, z), new Vector3(x1 - x0, .08f, .08f), m.Steel, true);
            Cube(p, name + "PostA", new Vector3(x0, y * .5f + L.UpperTop * .5f, z), new Vector3(.08f, 1.02f, .08f), m.Steel, true);
            Cube(p, name + "PostB", new Vector3(x1, y * .5f + L.UpperTop * .5f, z), new Vector3(.08f, 1.02f, .08f), m.Steel, true);
        }

        private static void RailZ(Transform p, string name, float x, float y, float z0, float z1, Mats m)
        {
            Cube(p, name, new Vector3(x, y, (z0 + z1) * .5f), new Vector3(.08f, .08f, z1 - z0), m.Steel, true);
            Cube(p, name + "PostA", new Vector3(x, y * .5f + L.UpperTop * .5f, z0), new Vector3(.08f, 1.02f, .08f), m.Steel, true);
            Cube(p, name + "PostB", new Vector3(x, y * .5f + L.UpperTop * .5f, z1), new Vector3(.08f, 1.02f, .08f), m.Steel, true);
        }

        private static void RetargetLadders(Transform root, float s)
        {
            Transform access = FindFirst(root, "VerticalAccess");
            if (access == null) return;

            foreach ((string name, float side) in new[] { ("UpperLadderLeft", -1f), ("UpperLadderRight", 1f) })
            {
                Transform ladder = FindFirst(access, name);
                if (ladder == null) continue;
                Transform upperExit = FindFirst(ladder, "UpperExit");
                if (upperExit == null) continue;

                Vector3 localTarget = new Vector3(s * VestibuleExitU, L.UpperTop + .055f, side * LandingExitZ);
                upperExit.position = root.TransformPoint(localTarget);
                float yaw = s > 0f ? -90f : 90f; // face from the vestibule into the public corridor
                upperExit.rotation = root.rotation * Quaternion.Euler(0f, yaw, 0f);
            }
        }

        private static void BuildWayfinding(Transform marker, float s, Mats m)
        {
            float x = s * 6.05f;
            Cube(marker, "CommonLandingMarker", new Vector3(x, L.UpperTop + .032f, 0f),
                new Vector3(.58f, .025f, 1.50f), m.Hazard, false);
            for (int i = -2; i <= 2; i++)
            {
                float arrowX = x - s * (i + 2) * .18f;
                Cube(marker, "CorridorDirectionTick", new Vector3(arrowX, L.UpperTop + .035f, 0f),
                    new Vector3(.08f, .028f, .62f - Mathf.Abs(i) * .08f), m.Hazard, false);
            }
        }

        private static Mats LoadMaterials() => new Mats
        {
            Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
            Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
            Steel = Mat("FT_PRO_BrushedSteel", new Color(.60f, .64f, .66f)),
            Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
            Bedding = Mat("FT_PRO_Bedding", new Color(.40f, .44f, .46f)),
            Table = Mat("FT_PRO_Table", new Color(.29f, .28f, .25f)),
            Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
            Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .58f, .20f)),
            Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)),
        };

        private static Material Mat(string name, Color fallback)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Art/Materials/TrainProduction/" + name + ".mat");
            if (material != null) return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material temporary = new Material(shader) { name = name + "_Fallback", color = fallback };
            return temporary;
        }

        private static Transform NewGroup(Transform parent, string name, Vector3 localPosition)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
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
    }
}
#endif
