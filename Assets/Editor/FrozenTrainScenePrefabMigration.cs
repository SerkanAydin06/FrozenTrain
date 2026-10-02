#if UNITY_EDITOR
using System.Collections.Generic;
using FrozenTrain.Atmosphere;
using FrozenTrain.Interaction;
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// One-time authoring migration. It uses the old procedural source only to materialize
    /// editable Unity assets, then converts the result to scene objects + prefab instances.
    /// Runtime gameplay never builds train geometry.
    /// </summary>
    public static class FrozenTrainScenePrefabMigration
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const string SharedPrefabFolder = "Assets/Prefabs/Train/Shared";
        private const string EnvironmentPrefabFolder = "Assets/Prefabs/Environment";
        private const string AtmosphereMaterialFolder = "Assets/Materials/Atmosphere";
        private const string PostProfilePath = "Assets/Settings/FrozenTrain_PostProcess.asset";

        private static readonly string[] CarNames =
        {
            "01_Locomotive",
            "02_Generator",
            "03_Living",
            "04_Workshop",
            "05_Greenhouse"
        };

        [MenuItem("Frozen Train/Convert To Scene + Prefabs", priority = 1)]
        public static void ConvertToScenePrefabs()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(TrainPrefabFolder);
            EnsureFolder(SharedPrefabFolder);
            EnsureFolder(EnvironmentPrefabFolder);
            EnsureFolder(AtmosphereMaterialFolder);
            EnsureFolder("Assets/Settings");

            // Materialize the latest clean five-car authoring layout once.
            FrozenTrainPlayablePrototypeBuilder.BuildPlayablePrototype();

            Scene scene = SceneManager.GetActiveScene();
            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (trainRoot == null)
            {
                Debug.LogError("FrozenTrain: TRAIN_INTERIOR_PROTOTYPE not found after authoring build.");
                return;
            }

            ReparentFunctionalContentIntoCars(trainRoot.transform);
            AddAuthoredDoorDetails(trainRoot.transform);
            AddAuthoredWindowFrost(trainRoot.transform);
            AttachSceneComponents(trainRoot);

            ConvertCarsToPrefabs(trainRoot.transform);
            ConvertConnectorsToPrefab(trainRoot.transform);
            ConvertEnvironmentToPrefab();
            CreateSceneAtmosphere();
            ConfigurePlayerAsSceneObject();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = trainRoot;
            Debug.Log(
                "FrozenTrain scene authoring complete. Train visuals are now editable prefab/scene assets. " +
                "Use FPSPrototype.unity and Assets/Prefabs/Train from now on; do not rebuild geometry from code.");
        }

        // Keep the old generator callable only by this migration, but disable its public menu path.
        [MenuItem("Frozen Train/Build Playable Prototype", true)]
        private static bool DisableLegacyBuilderMenu()
        {
            return false;
        }

        private static void ReparentFunctionalContentIntoCars(Transform trainRoot)
        {
            var cars = new List<Transform>();
            foreach (string carName in CarNames)
            {
                Transform car = trainRoot.Find(carName);
                if (car != null)
                {
                    cars.Add(car);
                }
            }

            if (cars.Count != CarNames.Length)
            {
                Debug.LogError("FrozenTrain: expected five car roots before prefab conversion.");
                return;
            }

            var directChildren = new List<Transform>();
            for (int i = 0; i < trainRoot.childCount; i++)
            {
                directChildren.Add(trainRoot.GetChild(i));
            }

            foreach (Transform child in directChildren)
            {
                if (cars.Contains(child) || child.name == "CarConnector")
                {
                    continue;
                }

                Transform nearestCar = FindNearestCar(child.position.x, cars);
                if (nearestCar != null)
                {
                    child.SetParent(nearestCar, true);
                }
            }
        }

        private static Transform FindNearestCar(float worldX, List<Transform> cars)
        {
            Transform best = null;
            float bestDistance = float.MaxValue;
            foreach (Transform car in cars)
            {
                float distance = Mathf.Abs(worldX - car.position.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = car;
                }
            }
            return best;
        }

        private static void AddAuthoredDoorDetails(Transform trainRoot)
        {
            Material frame = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/Train_Frame.mat");
            Material accent = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/Train_Amber.mat");

            SlidingDoor[] doors = trainRoot.GetComponentsInChildren<SlidingDoor>(true);
            foreach (SlidingDoor door in doors)
            {
                if (door.transform.Find("AuthoredDoorDetails") != null)
                {
                    continue;
                }

                GameObject root = new GameObject("AuthoredDoorDetails");
                root.transform.SetParent(door.transform, false);

                CreateWorldSizedChild(root.transform, door.transform, "CenterSeam",
                    new Vector3(0.066f, 0f, 0f), new Vector3(0.018f, 1.95f, 0.045f), frame);
                CreateWorldSizedChild(root.transform, door.transform, "Handle",
                    new Vector3(0.074f, -0.05f, 0.40f), new Vector3(0.035f, 0.34f, 0.060f), accent);
                CreateWorldSizedChild(root.transform, door.transform, "WarningPlate",
                    new Vector3(0.073f, 0.72f, 0f), new Vector3(0.022f, 0.16f, 0.62f), accent);
                CreateWorldSizedChild(root.transform, door.transform, "LowerKickPlate",
                    new Vector3(0.071f, -0.88f, 0f), new Vector3(0.020f, 0.32f, 1.32f), frame);
            }
        }

        private static void CreateWorldSizedChild(
            Transform parent,
            Transform scaledReference,
            string name,
            Vector3 worldSizedLocalOffset,
            Vector3 worldSize,
            Material material)
        {
            GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            detail.name = name;
            detail.transform.SetParent(parent, false);

            Vector3 parentScale = scaledReference.localScale;
            detail.transform.localPosition = new Vector3(
                worldSizedLocalOffset.x / Mathf.Max(0.001f, parentScale.x),
                worldSizedLocalOffset.y / Mathf.Max(0.001f, parentScale.y),
                worldSizedLocalOffset.z / Mathf.Max(0.001f, parentScale.z));
            detail.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.001f, parentScale.x),
                worldSize.y / Mathf.Max(0.001f, parentScale.y),
                worldSize.z / Mathf.Max(0.001f, parentScale.z));
            detail.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(detail.GetComponent<Collider>());
        }

        private static void AddAuthoredWindowFrost(Transform trainRoot)
        {
            Material frost = GetOrCreateTransparentMaterial(
                AtmosphereMaterialFolder + "/Train_Frost.mat",
                new Color(0.78f, 0.90f, 0.96f, 0.26f));

            Renderer[] renderers = trainRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.name != "WindowGlass" && renderer.name != "CabWindshield")
                {
                    continue;
                }

                if (renderer.transform.Find("FrostEdges") != null)
                {
                    continue;
                }

                GameObject frostRoot = new GameObject("FrostEdges");
                frostRoot.transform.SetParent(renderer.transform, false);

                if (renderer.name == "WindowGlass")
                {
                    CreateLocalVisual(frostRoot.transform, "FrostTop", new Vector3(0f, 0.44f, -0.58f), new Vector3(0.96f, 0.10f, 0.26f), frost);
                    CreateLocalVisual(frostRoot.transform, "FrostBottom", new Vector3(0f, -0.44f, -0.58f), new Vector3(0.96f, 0.10f, 0.26f), frost);
                    CreateLocalVisual(frostRoot.transform, "FrostLeft", new Vector3(-0.47f, 0f, -0.58f), new Vector3(0.07f, 0.78f, 0.26f), frost);
                    CreateLocalVisual(frostRoot.transform, "FrostRight", new Vector3(0.47f, 0f, -0.58f), new Vector3(0.07f, 0.78f, 0.26f), frost);
                }
                else
                {
                    CreateLocalVisual(frostRoot.transform, "FrostTop", new Vector3(-0.58f, 0.43f, 0f), new Vector3(0.26f, 0.10f, 0.95f), frost);
                    CreateLocalVisual(frostRoot.transform, "FrostBottom", new Vector3(-0.58f, -0.43f, 0f), new Vector3(0.26f, 0.10f, 0.95f), frost);
                }
            }
        }

        private static void AttachSceneComponents(GameObject trainRoot)
        {
            Light[] lights = trainRoot.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                if (light.name == "InteriorLight" && light.GetComponent<TrainLightDynamics>() == null)
                {
                    light.gameObject.AddComponent<TrainLightDynamics>();
                }
            }
        }

        private static void ConvertCarsToPrefabs(Transform trainRoot)
        {
            foreach (string carName in CarNames)
            {
                Transform carTransform = trainRoot.Find(carName);
                if (carTransform == null)
                {
                    continue;
                }

                SaveAndReplaceWithPrefab(
                    carTransform.gameObject,
                    TrainPrefabFolder + "/" + carName + ".prefab",
                    trainRoot);
            }
        }

        private static void ConvertConnectorsToPrefab(Transform trainRoot)
        {
            var connectors = new List<GameObject>();
            for (int i = 0; i < trainRoot.childCount; i++)
            {
                Transform child = trainRoot.GetChild(i);
                if (child.name == "CarConnector")
                {
                    connectors.Add(child.gameObject);
                }
            }

            if (connectors.Count == 0)
            {
                return;
            }

            string path = SharedPrefabFolder + "/CarConnector.prefab";
            GameObject template = Object.Instantiate(connectors[0]);
            template.name = "CarConnector";
            template.transform.SetParent(null);
            template.transform.position = Vector3.zero;
            template.transform.rotation = Quaternion.identity;
            template.transform.localScale = Vector3.one;
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(template, path);
            Object.DestroyImmediate(template);

            var placements = new List<(Vector3 position, Quaternion rotation, Vector3 scale)>();
            foreach (GameObject connector in connectors)
            {
                placements.Add((connector.transform.localPosition, connector.transform.localRotation, connector.transform.localScale));
                Object.DestroyImmediate(connector);
            }

            foreach (var placement in placements)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, trainRoot);
                instance.name = "CarConnector";
                instance.transform.localPosition = placement.position;
                instance.transform.localRotation = placement.rotation;
                instance.transform.localScale = placement.scale;
            }
        }

        private static void ConvertEnvironmentToPrefab()
        {
            GameObject environment = GameObject.Find("OUTSIDE_ENVIRONMENT");
            if (environment == null)
            {
                return;
            }

            SaveAndReplaceWithPrefab(
                environment,
                EnvironmentPrefabFolder + "/FrozenExterior.prefab",
                null);
        }

        private static GameObject SaveAndReplaceWithPrefab(GameObject source, string path, Transform parent)
        {
            Vector3 localPosition = source.transform.localPosition;
            Quaternion localRotation = source.transform.localRotation;
            Vector3 localScale = source.transform.localScale;
            string sourceName = source.name;

            GameObject template = Object.Instantiate(source);
            template.name = sourceName;
            template.transform.SetParent(null);
            template.transform.position = Vector3.zero;
            template.transform.rotation = Quaternion.identity;
            template.transform.localScale = Vector3.one;

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(template, path);
            Object.DestroyImmediate(template);
            Object.DestroyImmediate(source);

            GameObject instance = parent == null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset)
                : (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);

            instance.name = sourceName;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            return instance;
        }

        private static void ConfigurePlayerAsSceneObject()
        {
            GameObject player = GameObject.Find("FPS_Player");
            if (player == null)
            {
                return;
            }

            if (player.GetComponent<ProceduralMetalFootsteps>() == null)
            {
                player.AddComponent<ProceduralMetalFootsteps>();
            }

            Camera camera = player.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.dithering = true;
            }
        }

        private static void CreateSceneAtmosphere()
        {
            GameObject existing = GameObject.Find("SCENE_ATMOSPHERE");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject root = new GameObject("SCENE_ATMOSPHERE");

            VolumeProfile profile = CreatePostProcessProfile();
            GameObject volumeObject = new GameObject("GlobalPostProcessing");
            volumeObject.transform.SetParent(root.transform, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            volume.sharedProfile = profile;

            Material snowMaterial = GetOrCreateParticleMaterial(
                AtmosphereMaterialFolder + "/Train_SnowParticles.mat");

            GameObject snowObject = new GameObject("ExteriorSnow");
            snowObject.transform.SetParent(root.transform, false);
            snowObject.transform.position = new Vector3(0f, 7f, 0f);

            ParticleSystem particles = snowObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 5.5f;
            main.startSpeed = 11f;
            main.startSize = 0.075f;
            main.maxParticles = 850;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.05f;

            var emission = particles.emission;
            emission.rateOverTime = 90f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(110f, 8f, 34f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = -10f;
            velocity.y = -1.6f;
            velocity.z = 0.6f;

            ParticleSystemRenderer renderer = snowObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = snowMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static VolumeProfile CreatePostProcessProfile()
        {
            AssetDatabase.DeleteAsset(PostProfilePath);
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "FrozenTrain_PostProcess";
            AssetDatabase.CreateAsset(profile, PostProfilePath);

            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.13f);
            bloom.threshold.Override(1.10f);
            bloom.scatter.Override(0.48f);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.05f);
            color.contrast.Override(7f);
            color.saturation.Override(-5f);

            WhiteBalance white = profile.Add<WhiteBalance>(true);
            white.temperature.Override(-5f);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.11f);
            vignette.smoothness.Override(0.58f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static Material GetOrCreateTransparentMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader) { name = "Train_Frost" };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.72f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateParticleMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (material == null)
            {
                material = new Material(shader) { name = "Train_SnowParticles" };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", new Color(0.92f, 0.96f, 1f, 0.72f));
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateLocalVisual(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
