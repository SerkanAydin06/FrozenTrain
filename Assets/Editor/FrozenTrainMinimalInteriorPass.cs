#if UNITY_EDITOR
using System;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        private const string BaselineMaterialRoot = "Assets/Art/Materials/TrainBaseline";
        private static Material bodySlot;
        private static Material wallSlot;
        private static Material floorSlot;
        private static Material metalSlot;
        private static Material doorSlot;

        public static void Apply()
        {
            LoadBaselineSlots();
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
                    AssignBaselineMaterials(root.transform);
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
                AssignBaselineMaterials(connector.transform);
                removed += before - CountObjects(connector.transform);

                PrefabUtility.SaveAsPrefabAsset(connector, ConnectorPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(connector);
            }

            AssetDatabase.SaveAssets();
            RefreshAuthoredSceneInstances();
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
                        // Old decorative pocket leaves sit beside the actual opening
                        // and never seal it. Replace them with functional exact-fit doors.
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                        break;

                    default:
                        // Entire technical furniture, gameplay consoles, bunks,
                        // shelves, plant beds, posters, floor rugs and QA clutter.
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                        break;
                }
            }

            StripDoorDecorations(root);
            BuildCabinDoors(root);
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

        private static void BuildCabinDoors(Transform root)
        {
            Transform upper = root.Find("UpperSleepingDeck");
            if (upper == null) return;
            float bottom = L.UpperTop;
            float height = L.CorridorDoorHeight - .02f;
            float width = L.CorridorDoorWidth - .02f;
            int number = 0;
            foreach (float x in L.CorridorDoorX)
                foreach (float side in new[] { -1f, 1f })
                {
                    Transform door = MakeGroup(upper, "CabinDoor_" + (++number).ToString("00"));
                    door.localPosition = new Vector3(x, bottom + L.CorridorDoorHeight * .5f,
                        side * L.CorridorHalf);
                    Solid(door, "DoorPanel", Vector3.zero,
                        new Vector3(width, height, .085f), doorSlot);
                    BoxCollider hit = door.gameObject.AddComponent<BoxCollider>();
                    hit.size = new Vector3(width, height, .10f);
                    // The door parks against its adjacent partition, within
                    // the car footprint and outside the clear opening.
                    float direction = x > L.HalfL - 2f ? -1f : 1f;
                    door.gameObject.AddComponent<SlidingDoor>().Configure(
                        new Vector3(direction * (width + .025f), 0f, 0f), 2.6f);
                }
        }

        private static void LoadBaselineSlots()
        {
            if (!AssetDatabase.IsValidFolder(BaselineMaterialRoot))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
                    AssetDatabase.CreateFolder("Assets/Art", "Materials");
                AssetDatabase.CreateFolder("Assets/Art/Materials", "TrainBaseline");
            }

            // These are intentionally neutral URP/Lit PBR material slots with NO
            // textures. A future texture pass can edit each material without
            // changing any geometry or replacing references.
            bodySlot = BaselineSlot("01_Body", new Color(.40f, .43f, .47f));
            wallSlot = BaselineSlot("02_Walls", new Color(.55f, .57f, .60f));
            floorSlot = BaselineSlot("03_Floors", new Color(.35f, .37f, .40f));
            metalSlot = BaselineSlot("04_Structure", new Color(.28f, .30f, .33f));
            doorSlot = BaselineSlot("05_Doors", new Color(.48f, .50f, .53f));
        }

        private static Material BaselineSlot(string name, Color tint)
        {
            string path = BaselineMaterialRoot + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing; // Preserve user-assigned future PBR maps.
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit is unavailable.");
            Material material = new Material(shader) { name = name };
            material.color = tint;
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", .22f);
            material.SetTexture("_BaseMap", null);
            material.SetTexture("_BumpMap", null);
            material.DisableKeyword("_NORMALMAP");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void AssignBaselineMaterials(Transform root)
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                string source = r.sharedMaterial != null ? r.sharedMaterial.name : "";
                if (source.Contains("Glass") || r.name.Contains("Window")) continue;
                string n = r.name;
                if (r.GetComponentInParent<SlidingDoor>() != null ||
                    n == "DoorPanel") r.sharedMaterial = doorSlot;
                else if (n.Contains("Floor") || n.Contains("Deck"))
                    r.sharedMaterial = floorSlot;
                else if (n.Contains("Rail") || n.Contains("Rung") ||
                         n.Contains("Wheel") || n.Contains("Hub") ||
                         n.Contains("Underframe") || n.Contains("Bogie") ||
                         n.Contains("Bracket") || n.Contains("Mount") ||
                         n.Contains("Sill"))
                    r.sharedMaterial = metalSlot;
                else if (n.Contains("Wall") || n.Contains("Divider") ||
                         n.Contains("Lintel") || n.Contains("Header") ||
                         n.Contains("Roof") || n.Contains("Partition"))
                    r.sharedMaterial = wallSlot;
                else r.sharedMaterial = bodySlot;
            }
        }

        private static void RefreshAuthoredSceneInstances()
        {
            // Builder places its pre-art prefabs into the open scene BEFORE all
            // art passes change the prefab assets. Replace those stale instances
            // with the final cleaned prefabs so a single Rebuild updates the scene.
            GameObject sceneTrain = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (sceneTrain == null) return;
            Transform parent = sceneTrain.transform;
            Transform[] existing = parent.Cast<Transform>().ToArray();
            for (int i = 0; i < existing.Length; i++)
            {
                Transform old = existing[i];
                string sourcePath = null;
                foreach (CarSpec car in L.Cars)
                    if (old.name == car.Name || old.name == car.Name + "(Clone)")
                    {
                        sourcePath = PrefabRoot + "/" + car.Name + ".prefab";
                        break;
                    }
                if (sourcePath == null && old.name.StartsWith("CarConnector", StringComparison.Ordinal))
                    sourcePath = ConnectorPath;
                if (sourcePath == null) continue;
                GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourcePrefab == null) throw new InvalidOperationException("Missing scene prefab " + sourcePath);

                Vector3 position = old.localPosition;
                Quaternion rotation = old.localRotation;
                Vector3 scale = old.localScale;
                int sibling = old.GetSiblingIndex();
                UnityEngine.Object.DestroyImmediate(old.gameObject);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, parent);
                instance.transform.SetSiblingIndex(sibling);
                instance.transform.localPosition = position;
                instance.transform.localRotation = rotation;
                instance.transform.localScale = scale;
            }
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
