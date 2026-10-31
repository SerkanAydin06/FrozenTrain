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
                    shell.Find("LowerLadder")?.GetComponent<DeckLadder>() == null)
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
                        // Keep just the load-bearing body, floors, walls, windows, roof and bogies.
                        break;

                    case "ExteriorDressing":
                    case "__LongExteriorV1":
                        // Snow drifts, icicles, duplicate wheels, roof chamfers, raised
                        // exterior bands and nose ornaments are intentionally discarded.
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
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
                        // Strip the former exterior detail layer too: some wide trim
                        // pieces crossed the hull outline or overlapped the interior.
                        KeepChildren(child, t => t.name == "UpperAccommodationDetails");
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
            AddMinimalExterior(root, root.name == L.Cars[0].Name);
            AddInvisibleLighting(root);
        }

        private static void StripVestibule(Transform root)
        {
            foreach (Transform child in root.Cast<Transform>().ToArray())
            {
                if (child.name == "VestibuleShell")
                {
                    // Open vestibule: keep the shell, decks, passage doors and
                    // two ladders, without the former enclosed stair room.
                    KeepChildren(child, t =>
                    {
                        string n = t.name;
                        return n.StartsWith("LowerFloor", StringComparison.Ordinal) ||
                            n.StartsWith("MainFloor", StringComparison.Ordinal) ||
                            n.StartsWith("UpperFloor", StringComparison.Ordinal) ||
                            n == "Roof" ||
                            n == "OuterWallL" || n == "OuterWallR" ||
                            n == "VestibuleUnderframe" ||
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

        private static void AddMinimalExterior(Transform root, bool locomotive)
        {
            // Geometry is entirely outside the structural side-wall face (+/-4.7 m).
            // No duplicate roof ornaments, oversized nose pieces or snow clutter.
            Transform exterior = MakeGroup(root, "__MinimalExteriorV1");
            Material dark = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Art/Materials/TrainProduction/FT_PRO_Frame.mat");
            Material steel = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Art/Materials/TrainProduction/FT_PRO_BrushedSteel.mat");
            Material body = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Art/Materials/TrainProduction/FT_PRO_Exterior.mat");
            Material lens = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Art/Materials/TrainProduction/FT_PRO_WhiteEnamel.mat");
            if (dark == null || steel == null || body == null || lens == null)
                throw new InvalidOperationException(
                    "Exterior materials missing. Run the unified production build before minimal cleanup.");

            foreach (float side in new[] { -1f, 1f })
            {
                // Thin understated sill along each exterior lower side, without
                // intruding through the train side wall or its windows.
                Solid(exterior, "LowerExteriorSill",
                    new Vector3(0f, -2.73f, side * (L.HalfW + .145f)),
                    new Vector3(L.CarLength - .50f, .16f, .075f), dark);

                foreach (float bogieX in new[] { -L.HalfL + 3f, L.HalfL - 3f })
                    foreach (float axleOffset in new[] { -.70f, .70f })
                    {
                        // Actual round train wheels, below the passenger floors.
                        float x = bogieX + axleOffset;
                        float y = L.RailTopY + .44f;
                        float z = side * (L.HalfW + .14f);
                        Wheel(exterior, "Wheel", new Vector3(x, y, z), .94f, .17f, dark);
                        Wheel(exterior, "Hub", new Vector3(x, y, z + side * .105f),
                            .32f, .025f, steel);
                    }
            }

            if (!locomotive) return;

            // Simple closed nose, completely in front of the cab bulkhead (-X).
            Solid(exterior, "LocomotiveLowerNose",
                new Vector3(-L.HalfL - .43f, .29f, 0f),
                new Vector3(.82f, .62f, 7.55f), body);
            foreach (float z in new[] { -3.05f, 3.05f })
            {
                Solid(exterior, "Headlamp",
                    new Vector3(-L.HalfL - .86f, .48f, z),
                    new Vector3(.055f, .26f, .34f), lens);
                GameObject lightObject = new GameObject("HeadlampLight");
                lightObject.transform.SetParent(exterior, false);
                lightObject.transform.localPosition = new Vector3(-L.HalfL - .91f, .48f, z);
                lightObject.transform.localRotation = Quaternion.LookRotation(Vector3.left);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 55f;
                light.spotAngle = 38f;
                light.intensity = 3.2f;
                light.color = new Color(.94f, .96f, 1f);
                light.shadows = LightShadows.None;
            }
        }

        private static void Wheel(Transform parent, string name, Vector3 position,
            float diameter, float thickness, Material material)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = position;
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheel.transform.localScale = new Vector3(diameter, thickness * .5f, diameter);
            wheel.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }

        private static void Solid(Transform parent, string name, Vector3 position,
            Vector3 size, Material material)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = position;
            piece.transform.localScale = size;
            piece.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(piece.GetComponent<Collider>());
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
