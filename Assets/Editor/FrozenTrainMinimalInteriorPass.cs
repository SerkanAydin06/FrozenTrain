#if UNITY_EDITOR
using System;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Bare train foundation for rebuilding the interior one element at a time.
    /// Keep only the car shells, structural partitions, windows, doors and vertical ladders.
    /// Exterior train geometry and invisible visibility lighting remain functional.
    ///
    /// Runs LAST in the unified authoring chain so historical dressing passes cannot
    /// reintroduce props. All removed items remain available in source for future phases.
    /// </summary>
    internal static class FrozenTrainMinimalInteriorPass
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = PrefabRoot + "/Shared/CarConnector.prefab";

        public static void Apply()
        {
            int removed = 0;

            foreach (CarSpec car in L.Cars)
            {
                string path = PrefabRoot + "/" + car.Name + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    throw new InvalidOperationException("Missing train car prefab: " + path);

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (root.transform.Find("Shell") == null ||
                        root.transform.Find("UpperSleepingDeck") == null)
                        throw new InvalidOperationException("Cannot simplify car without its structural shell: " + path);

                    int before = CountObjects(root.transform);
                    StripCar(root.transform);
                    removed += before - CountObjects(root.transform);

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null)
                throw new InvalidOperationException("Missing vestibule prefab: " + ConnectorPath);

            GameObject connector = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Transform shell = connector.transform.Find("VestibuleShell");
                if (shell == null ||
                    shell.Find("UpperLadder")?.GetComponent<DeckLadder>() == null ||
                    shell.Find("LowerLadder")?.GetComponent<DeckLadder>() == null ||
                    shell.Find("StairCoreDoor")?.GetComponent<SlidingDoor>() == null)
                    throw new InvalidOperationException("The vestibule ladders or door are missing. Simplification aborted.");

                int before = CountObjects(connector.transform);
                StripVestibule(connector.transform);
                removed += before - CountObjects(connector.transform);

                PrefabUtility.SaveAsPrefabAsset(connector, ConnectorPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(connector);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FrozenTrain] Minimal interiors: removed " + removed +
                      " nonessential objects from five cars and the inter-car vestibule prefab. " +
                      "Shells, windows, plain doors, climbable ladders and basic invisible lighting remain.");
        }

        private static void StripCar(Transform root)
        {
            foreach (Transform child in root.Cast<Transform>().ToArray())
            {
                switch (child.name)
                {
                    case "Shell":
                    case "ExteriorDressing":
                    case "__LongExteriorV1":
                        // Structural floors, walls, windows, roof, wheels and outside trim.
                        break;

                    case "UpperSleepingDeck":
                        // Keep the cabin partitions and door openings, remove every bed.
                        KeepChildren(child, t => t.name == "CorridorWall" ||
                            t.name == "CorridorDoorLintel" || t.name == "RoomDivider");
                        break;

                    case "Lighting":
                        // Existing light sources only: no lamp housings or glowing strips.
                        KeepChildren(child, t => t.GetComponent<Light>() != null);
                        break;

                    case "__RightSideCorridorV2":
                        KeepChildren(child, t => t.name == "RightSidePublicCorridor");
                        Transform corridor = child.Find("RightSidePublicCorridor");
                        if (corridor != null)
                            KeepChildren(corridor, t =>
                                t.name == "CompartmentWallFront" ||
                                t.name == "CompartmentWallRear" ||
                                t.name == "CompartmentDoorHeader" ||
                                t.name == "FunctionalCompartmentDoor");
                        if (child.childCount == 0) UnityEngine.Object.DestroyImmediate(child.gameObject);
                        break;

                    case "__ProductionArtV2":
                        // Exterior work stays; keep only the doorway parts from the
                        // old upstairs art pass, not the furniture or its decorations.
                        KeepChildren(child, t => t.name == "ExteriorProductionDetails" ||
                            t.name == "UpperAccommodationDetails");
                        Transform doors = child.Find("UpperAccommodationDetails");
                        if (doors != null)
                        {
                            KeepChildren(doors, t =>
                                t.name == "PocketDoorLeaf");
                        }
                        break;

                    default:
                        // Entire technical furniture, gameplay consoles, bunks,
                        // shelves, plant beds, posters, floor rugs and QA clutter.
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                        break;
                }
            }

            StripDoorDecorations(root);
            AddInvisibleLighting(root);
        }

        private static void StripVestibule(Transform root)
        {
            foreach (Transform child in root.Cast<Transform>().ToArray())
            {
                if (child.name == "VestibuleShell")
                {
                    // Keep only the shell, walkable deck tiles, actual doors,
                    // partition around the ladder door and two upright ladders.
                    KeepChildren(child, t =>
                    {
                        string n = t.name;
                        return n.StartsWith("LowerFloor", StringComparison.Ordinal) ||
                            n.StartsWith("MainFloor", StringComparison.Ordinal) ||
                            n.StartsWith("UpperFloor", StringComparison.Ordinal) ||
                            n == "Roof" || n == "RoofSnow" ||
                            n == "OuterWallL" || n == "OuterWallR" ||
                            n == "VestibuleUnderframe" ||
                            n == "StairPartitionA" || n == "StairPartitionB" ||
                            n == "StairDoorHeader" || n == "StairCoreDoor" ||
                            n == "MainVestibuleDoorFront" || n == "MainVestibuleDoorRear" ||
                            n == "UpperDoorFront" || n == "UpperDoorRear" ||
                            n == "TechnicalDoorFront" || n == "TechnicalDoorRear" ||
                            n == "UpperLadder" || n == "LowerLadder";
                    });
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }

            StripDoorDecorations(root);

            // No visible fixtures: invisible lights keep the three decks navigable.
            Transform lightGroup = MakeGroup(root, "__MinimalVisibility");
            foreach (float y in new[] { L.LowerTop + 2.25f, L.MainTop + 2.25f, L.UpperTop + 2.25f })
                AddLight(lightGroup, new Vector3(0f, y, 0f), 1.0f, 7f);
        }

        private static void StripDoorDecorations(Transform root)
        {
            // The door panel itself remains clickable; every orange band, handle,
            // sign and ornamental header attached to it is removed.
            foreach (SlidingDoor door in root.GetComponentsInChildren<SlidingDoor>(true))
                KeepChildren(door.transform, part => part.name == "DoorPanel");
        }

        private static void AddInvisibleLighting(Transform root)
        {
            Transform lightGroup = MakeGroup(root, "__MinimalVisibility");
            foreach (float y in new[] { L.LowerTop + 2.25f, L.MainTop + 2.25f, L.UpperTop + 2.25f })
                foreach (float x in new[] { -7.6f, 7.6f })
                    AddLight(lightGroup, new Vector3(x, y, 0f), 1.05f, 10f);
        }

        private static Transform MakeGroup(Transform parent, string name)
        {
            GameObject holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            return holder.transform;
        }

        private static void AddLight(Transform parent, Vector3 position, float intensity, float range)
        {
            GameObject holder = new GameObject("InvisibleLight");
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = position;
            Light light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = intensity;
            light.range = range;
            light.color = new Color(.82f, .87f, .94f);
            light.shadows = LightShadows.None;
        }

        private static void KeepChildren(Transform parent, Func<Transform, bool> keep)
        {
            foreach (Transform child in parent.Cast<Transform>().ToArray())
                if (!keep(child)) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static int CountObjects(Transform root) => root.GetComponentsInChildren<Transform>(true).Length;
    }
}
#endif
