#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final architecture rules layered on top of the five-car authored build.
    /// No public menu item: this is invoked automatically when authored prefabs/scenes are saved.
    /// </summary>
    public static class FrozenTrainArchitectureRevision
    {
        public const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        public const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";

        private const float CarLength = 16.5f;
        private const float CarWidth = 8.4f;
        private const float HalfLength = CarLength * 0.5f;
        private const float HalfWidth = CarWidth * 0.5f;
        private const float BasementFloorY = -2.15f;
        private const float MainFloorY = 0f;
        private const float UpperFloorY = 3.10f;
        private const float RoofY = 6.20f;

        private static readonly string[] CarNames =
        {
            "01_Locomotive",
            "02_Generator",
            "03_Living",
            "04_Workshop",
            "05_Greenhouse"
        };

        public static bool IsCarPath(string path)
        {
            return CarNames.Any(name => path == "Assets/Prefabs/Train/" + name + ".prefab");
        }

        public static void ApplyCar(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;

            int index = System.Array.FindIndex(CarNames, n => path.EndsWith("/" + n + ".prefab"));
            if (index < 0) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Material floor = Mat("Train_Floor");
                Material wall = Mat("Train_Wall");
                Material frame = Mat("Train_Frame");
                Material steel = Mat("Train_Steel");
                Material glass = Mat("Train_WindowGlass");
                Material mattress = Mat("Train_Mattress");
                Material living = Mat("Train_Living");

                RemoveDeepByName(root.transform, "ArchitectureRevisionV2");
                RemoveDeepByName(root.transform, "VerticalAccess");

                Transform architecture = FindDeep(root.transform, "MultiDeckArchitecture");
                if (architecture == null) return;

                ClearLegacyLivingOpenings(architecture);
                EnsureFloorMarker(root.transform);
                RebuildUpperEnds(architecture, wall);

                Transform revision = Group(root.transform, "ArchitectureRevisionV2");

                List<RectHole> mainHoles = new List<RectHole>();
                if (index == 1 || index == 3)
                {
                    mainHoles.Add(new RectHole(-7.10f, -5.72f, -4.08f, -3.00f));
                }
                BuildFloorWithHoles(revision, "MainFloor", MainFloorY, 0.22f, mainHoles, floor);

                RectHole upperHole = index == 2
                    ? new RectHole(-0.72f, 0.72f, 3.00f, 4.08f)
                    : new RectHole(5.72f, 7.10f, 3.00f, 4.08f);
                BuildFloorWithHoles(revision, "UpperFloor", UpperFloorY, 0.18f, new List<RectHole> { upperHole }, floor);

                RemoveDeepByName(root.transform, "UpperLockerRight");
                BuildUpperLadder(revision, index, frame, steel);

                if (index == 1 || index == 3)
                {
                    BuildBasementLadder(revision, frame, steel);
                }

                if (index == 0)
                {
                    RebuildPanoramicCabFront(root.transform, revision, wall, frame, glass);
                }

                if (index == 2)
                {
                    RebuildLivingUpperRooms(root.transform, revision, wall, frame, steel, living, mattress);
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static void ApplyConnector()
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath);
            if (asset == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Material floor = Mat("Train_Floor");
                Material rubber = Mat("Train_Rubber");

                RemoveDeepByName(root.transform, "MultiDeckConnector");
                RemoveDeepByName(root.transform, "TechnicalBasementConnector");

                Transform g = Group(root.transform, "TechnicalBasementConnector");
                Cube(g, "BasementConnectorFloor", new Vector3(0f, BasementFloorY, 0f), new Vector3(0.85f, 0.18f, 1.75f), floor);

                float height = MainFloorY - BasementFloorY;
                float center = BasementFloorY + height * 0.5f;
                Cube(g, "BasementBellowsLeft", new Vector3(0f, center, -0.92f), new Vector3(0.85f, height, 0.12f), rubber);
                Cube(g, "BasementBellowsRight", new Vector3(0f, center, 0.92f), new Vector3(0.85f, height, 0.12f), rubber);

                // Intentionally no upper-deck connector: sleeping decks are private to each car.
                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static void ApplySceneSnow()
        {
            if (!System.IO.File.Exists(ScenePath)) return;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                foreach (ParticleSystem ps in sceneRoot.GetComponentsInChildren<ParticleSystem>(true))
                {
                    string n = ps.gameObject.name.ToLowerInvariant();
                    if (n.Contains("snow") || n.Contains("kar"))
                    {
                        Object.DestroyImmediate(ps.gameObject);
                    }
                }
            }

            GameObject atmosphere = GameObject.Find("SCENE_ATMOSPHERE");
            if (atmosphere == null) atmosphere = new GameObject("SCENE_ATMOSPHERE");

            Material snowMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Atmosphere/Train_SnowParticles.mat");
            if (snowMaterial != null)
            {
                CreateExteriorSnow(atmosphere.transform, "ExteriorSnowFar_Left", -18.5f, snowMaterial);
                CreateExteriorSnow(atmosphere.transform, "ExteriorSnowFar_Right", 18.5f, snowMaterial);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void EnsureFloorMarker(Transform root)
        {
            Transform floor = FindDirect(root, "Floor");
            if (floor == null)
            {
                GameObject marker = new GameObject("Floor");
                marker.transform.SetParent(root, false);
                floor = marker.transform;
            }

            foreach (Component component in floor.GetComponents<Component>())
            {
                if (component is Transform) continue;
                Object.DestroyImmediate(component);
            }
            floor.localPosition = Vector3.zero;
            floor.localRotation = Quaternion.identity;
            floor.localScale = new Vector3(CarLength, 1f, CarWidth);
        }

        private static void ClearLegacyLivingOpenings(Transform architecture)
        {
            for (int i = architecture.childCount - 1; i >= 0; i--)
            {
                Transform child = architecture.GetChild(i);
                if (child.name == "UpperDeckFloor" ||
                    child.name == "LivingFloorOpeningsApplied" ||
                    child.name.StartsWith("MainFloor_") ||
                    child.name.StartsWith("UpperDeckFloor_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RebuildUpperEnds(Transform architecture, Material wall)
        {
            for (int i = architecture.childCount - 1; i >= 0; i--)
            {
                Transform child = architecture.GetChild(i);
                if (child.name.StartsWith("UpperFront") || child.name.StartsWith("UpperRear"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            float height = RoofY - UpperFloorY;
            float centerY = UpperFloorY + height * 0.5f;
            Cube(architecture, "UpperFrontSolidV2", new Vector3(-HalfLength, centerY, 0f), new Vector3(0.20f, height, CarWidth), wall);
            Cube(architecture, "UpperRearSolidV2", new Vector3(HalfLength, centerY, 0f), new Vector3(0.20f, height, CarWidth), wall);
        }

        private static void BuildUpperLadder(Transform parent, int carIndex, Material frame, Material steel)
        {
            Vector3 ladderPosition = carIndex == 2
                ? new Vector3(0f, 1.55f, 3.87f)
                : new Vector3(6.42f, 1.55f, 3.87f);

            Vector3 lowerExit = carIndex == 2
                ? new Vector3(0f, 0.12f, 2.52f)
                : new Vector3(6.38f, 0.12f, 2.52f);
            Vector3 upperExit = new Vector3(lowerExit.x, UpperFloorY + 0.12f, lowerExit.z);

            BuildWallLadder(parent, "UpperWallLadder", ladderPosition, lowerExit, upperExit,
                "E - Üst kata çık", "E - Ana kata in", frame, steel);
        }

        private static void BuildBasementLadder(Transform parent, Material frame, Material steel)
        {
            Vector3 ladderPosition = new Vector3(-6.42f, -1.02f, -3.87f);
            Vector3 lowerExit = new Vector3(-6.35f, BasementFloorY + 0.12f, -2.52f);
            Vector3 upperExit = new Vector3(-6.35f, MainFloorY + 0.12f, -2.52f);

            BuildWallLadder(parent, "TechnicalBasementLadder", ladderPosition, lowerExit, upperExit,
                "E - Teknik kata in", "E - Ana kata çık", frame, steel);
        }

        private static void BuildWallLadder(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 lowerExitPosition,
            Vector3 upperExitPosition,
            string upText,
            string downText,
            Material frame,
            Material rung)
        {
            Transform ladder = Group(parent, name);
            ladder.localPosition = position;

            float height = Mathf.Abs(upperExitPosition.y - lowerExitPosition.y);
            Cube(ladder, "RailLeft", new Vector3(-0.43f, 0f, 0f), new Vector3(0.08f, height, 0.08f), frame);
            Cube(ladder, "RailRight", new Vector3(0.43f, 0f, 0f), new Vector3(0.08f, height, 0.08f), frame);

            int rungCount = Mathf.Max(6, Mathf.RoundToInt(height / 0.28f));
            for (int i = 0; i <= rungCount; i++)
            {
                float t = i / (float)rungCount;
                float y = Mathf.Lerp(-height * 0.5f, height * 0.5f, t);
                Cube(ladder, "Rung", new Vector3(0f, y, -0.02f), new Vector3(0.94f, 0.055f, 0.08f), rung);
            }

            GameObject interaction = Cube(ladder, "LadderInteraction", Vector3.zero, new Vector3(1.25f, height, 0.45f), frame);
            Renderer interactionRenderer = interaction.GetComponent<Renderer>();
            if (interactionRenderer != null) Object.DestroyImmediate(interactionRenderer);
            MeshFilter interactionMesh = interaction.GetComponent<MeshFilter>();
            if (interactionMesh != null) Object.DestroyImmediate(interactionMesh);

            Transform lowerExit = Empty(ladder, "LowerExit", ladder.InverseTransformPoint(parent.TransformPoint(lowerExitPosition)));
            Transform upperExit = Empty(ladder, "UpperExit", ladder.InverseTransformPoint(parent.TransformPoint(upperExitPosition)));

            DeckLadder deckLadder = interaction.AddComponent<DeckLadder>();
            deckLadder.Configure(lowerExit, upperExit, upText, downText);
        }

        private static void RebuildPanoramicCabFront(Transform root, Transform revision, Material wall, Material frame, Material glass)
        {
            RemoveDeepByPrefix(root, "CabFront");
            RemoveDeepByPrefix(root, "CabWindshield");
            RemoveDeepByName(root, "PanoramicCabFront");

            Transform g = Group(revision, "PanoramicCabFront");
            float x = -HalfLength;
            Cube(g, "CabFrontLower", new Vector3(x, 0.58f, 0f), new Vector3(0.20f, 1.16f, CarWidth), wall);
            Cube(g, "CabFrontHeader", new Vector3(x, 2.90f, 0f), new Vector3(0.20f, 0.38f, CarWidth), wall);
            Cube(g, "CabFrontEdgeLeft", new Vector3(x, 1.98f, -4.03f), new Vector3(0.20f, 1.46f, 0.20f), frame);
            Cube(g, "CabFrontEdgeRight", new Vector3(x, 1.98f, 4.03f), new Vector3(0.20f, 1.46f, 0.20f), frame);
            Cube(g, "PanoramicWindshield", new Vector3(x - 0.015f, 1.98f, 0f), new Vector3(0.035f, 1.48f, 7.78f), glass);
        }

        private static void RebuildLivingUpperRooms(Transform root, Transform revision, Material wall, Material frame, Material steel, Material living, Material mattress)
        {
            RemoveDeepByName(root, "UpperSleepingDeck");
            Transform rooms = Group(revision, "LivingUpperRooms");

            float wallY = UpperFloorY + 1.48f;
            float wallHeight = 2.72f;

            // Central corridor with four compact private rooms. The wall ladder lands in the middle.
            BuildCorridorWall(rooms, -1.18f, wallY, wallHeight, wall);
            BuildCorridorWall(rooms, 1.18f, wallY, wallHeight, wall);
            Cube(rooms, "LeftRoomDivider", new Vector3(0f, wallY, -2.68f), new Vector3(0.12f, wallHeight, 2.88f), wall);
            Cube(rooms, "RightRoomDivider", new Vector3(0f, wallY, 2.68f), new Vector3(0.12f, wallHeight, 2.88f), wall);

            BuildPrivateRoomFurniture(rooms, new Vector3(-3.55f, UpperFloorY, -2.78f), false, frame, steel, living, mattress);
            BuildPrivateRoomFurniture(rooms, new Vector3(3.55f, UpperFloorY, -2.78f), true, frame, steel, living, mattress);
            BuildPrivateRoomFurniture(rooms, new Vector3(-3.55f, UpperFloorY, 2.78f), false, frame, steel, living, mattress);
            BuildPrivateRoomFurniture(rooms, new Vector3(3.55f, UpperFloorY, 2.78f), true, frame, steel, living, mattress);
        }

        private static void BuildCorridorWall(Transform parent, float z, float y, float height, Material wall)
        {
            // Door openings at x +/-3.55, plus open ladder landing around x=0 on the +Z wall.
            Cube(parent, "CorridorWallA", new Vector3(-6.35f, y, z), new Vector3(1.75f, height, 0.12f), wall);
            Cube(parent, "CorridorWallB", new Vector3(-1.78f, y, z), new Vector3(2.65f, height, 0.12f), wall);
            Cube(parent, "CorridorWallC", new Vector3(1.78f, y, z), new Vector3(2.65f, height, 0.12f), wall);
            Cube(parent, "CorridorWallD", new Vector3(6.35f, y, z), new Vector3(1.75f, height, 0.12f), wall);
        }

        private static void BuildPrivateRoomFurniture(Transform parent, Vector3 basePosition, bool faceNegativeX, Material frame, Material steel, Material living, Material mattress)
        {
            float direction = faceNegativeX ? -1f : 1f;
            Cube(parent, "PrivateBedFrame", basePosition + new Vector3(direction * 0.15f, 0.46f, 0f), new Vector3(2.35f, 0.18f, 0.92f), frame);
            Cube(parent, "PrivateMattress", basePosition + new Vector3(direction * 0.15f, 0.60f, 0f), new Vector3(2.18f, 0.16f, 0.80f), mattress);
            Cube(parent, "PrivateLocker", basePosition + new Vector3(-direction * 1.15f, 1.02f, 0.25f), new Vector3(0.72f, 1.78f, 0.62f), steel);
            Cube(parent, "PrivateShelf", basePosition + new Vector3(0f, 1.48f, -0.48f), new Vector3(1.55f, 0.12f, 0.36f), living);
        }

        private static void BuildFloorWithHoles(Transform parent, string prefix, float y, float thickness, List<RectHole> holes, Material material)
        {
            List<float> xs = new List<float> { -HalfLength, HalfLength };
            List<float> zs = new List<float> { -HalfWidth, HalfWidth };
            foreach (RectHole hole in holes)
            {
                xs.Add(Mathf.Clamp(hole.minX, -HalfLength, HalfLength));
                xs.Add(Mathf.Clamp(hole.maxX, -HalfLength, HalfLength));
                zs.Add(Mathf.Clamp(hole.minZ, -HalfWidth, HalfWidth));
                zs.Add(Mathf.Clamp(hole.maxZ, -HalfWidth, HalfWidth));
            }
            xs = xs.Distinct().OrderBy(v => v).ToList();
            zs = zs.Distinct().OrderBy(v => v).ToList();

            int piece = 0;
            for (int xi = 0; xi < xs.Count - 1; xi++)
            {
                for (int zi = 0; zi < zs.Count - 1; zi++)
                {
                    float minX = xs[xi];
                    float maxX = xs[xi + 1];
                    float minZ = zs[zi];
                    float maxZ = zs[zi + 1];
                    if (maxX - minX < 0.01f || maxZ - minZ < 0.01f) continue;

                    Vector2 center = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
                    if (holes.Any(h => h.Contains(center))) continue;

                    Cube(parent, prefix + "_" + piece++,
                        new Vector3(center.x, y, center.y),
                        new Vector3(maxX - minX, thickness, maxZ - minZ), material);
                }
            }
        }

        private static void CreateExteriorSnow(Transform parent, string name, float z, Material material)
        {
            GameObject snow = new GameObject(name);
            snow.transform.SetParent(parent, false);
            snow.transform.localPosition = new Vector3(0f, 6.0f, z);

            ParticleSystem ps = snow.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 5.5f;
            main.startSpeed = 0f;
            main.startSize = 0.075f;
            main.maxParticles = 420;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0.025f;

            var emission = ps.emission;
            emission.rateOverTime = 42f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(120f, 10f, 1.5f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = 10f;
            velocity.y = -1.35f;
            velocity.z = 0f;

            ParticleSystemRenderer renderer = snow.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Material Mat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/" + name + ".mat");
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform Empty(Transform parent, string name, Vector3 localPosition)
        {
            Transform t = Group(parent, name);
            t.localPosition = localPosition;
            return t;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static Transform FindDirect(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name == name) return root.GetChild(i);
            return null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static void RemoveDeepByName(Transform root, string name)
        {
            List<Transform> matches = root.GetComponentsInChildren<Transform>(true)
                .Where(t => t != root && t.name == name)
                .OrderByDescending(GetDepth)
                .ToList();
            foreach (Transform match in matches)
            {
                if (match != null) Object.DestroyImmediate(match.gameObject);
            }
        }

        private static void RemoveDeepByPrefix(Transform root, string prefix)
        {
            List<Transform> matches = root.GetComponentsInChildren<Transform>(true)
                .Where(t => t != root && t.name.StartsWith(prefix))
                .OrderByDescending(GetDepth)
                .ToList();
            foreach (Transform match in matches)
            {
                if (match != null) Object.DestroyImmediate(match.gameObject);
            }
        }

        private static int GetDepth(Transform t)
        {
            int depth = 0;
            Transform p = t.parent;
            while (p != null) { depth++; p = p.parent; }
            return depth;
        }

        private readonly struct RectHole
        {
            public readonly float minX;
            public readonly float maxX;
            public readonly float minZ;
            public readonly float maxZ;

            public RectHole(float minX, float maxX, float minZ, float maxZ)
            {
                this.minX = minX;
                this.maxX = maxX;
                this.minZ = minZ;
                this.maxZ = maxZ;
            }

            public bool Contains(Vector2 p)
            {
                return p.x > minX + 0.001f && p.x < maxX - 0.001f && p.y > minZ + 0.001f && p.y < maxZ - 0.001f;
            }
        }
    }
}
#endif
