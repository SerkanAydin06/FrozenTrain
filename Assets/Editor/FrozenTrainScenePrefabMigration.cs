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
    /// Internal first-run helper used by FrozenTrainUnifiedAuthoring.
    /// It materializes the legacy procedural source exactly once, then converts the result
    /// into editable scene/prefab assets. It intentionally has no public menu entry.
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

        public static void ConvertToScenePrefabs()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(TrainPrefabFolder);
            EnsureFolder(SharedPrefabFolder);
            EnsureFolder(EnvironmentPrefabFolder);
            EnsureFolder(AtmosphereMaterialFolder);
            EnsureFolder("Assets/Settings");

            FrozenTrainPlayablePrototypeBuilder.BuildPlayablePrototype();

            Scene scene = SceneManager.GetActiveScene();
            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (trainRoot == null)
            {
                Debug.LogError("FrozenTrain: train root not found after base authoring build.");
                return;
            }

            ReparentFunctionalContentIntoCars(trainRoot.transform);
            AddAuthoredDoorDetails(trainRoot.transform);
            AttachSceneComponents(trainRoot);
            ConvertCarsToPrefabs(trainRoot.transform);
            ConvertConnectorsToPrefab(trainRoot.transform);
            ConvertEnvironmentToPrefab();
            ConfigurePlayerAsSceneObject();
            CreateBaseSceneAtmosphere();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = trainRoot;
        }

        // The old procedural builder is retained only as an implementation detail.
        [MenuItem("Frozen Train/Build Playable Prototype", true)]
        private static bool DisableLegacyBuilderMenu() => false;

        private static void ReparentFunctionalContentIntoCars(Transform trainRoot)
        {
            var cars = new List<Transform>();
            foreach (string carName in CarNames)
            {
                Transform car = trainRoot.Find(carName);
                if (car != null) cars.Add(car);
            }

            if (cars.Count != CarNames.Length)
            {
                Debug.LogError("FrozenTrain: expected five car roots before prefab conversion.");
                return;
            }

            var directChildren = new List<Transform>();
            for (int i = 0; i < trainRoot.childCount; i++) directChildren.Add(trainRoot.GetChild(i));

            foreach (Transform child in directChildren)
            {
                if (cars.Contains(child) || child.name == "CarConnector") continue;
                Transform nearest = FindNearestCar(child.position.x, cars);
                if (nearest != null) child.SetParent(nearest, true);
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
                if (door.transform.Find("AuthoredDoorDetails") != null) continue;

                GameObject details = new GameObject("AuthoredDoorDetails");
                details.transform.SetParent(door.transform, false);
                CreateWorldSizedChild(details.transform, door.transform, "CenterSeam", new Vector3(0.066f, 0f, 0f), new Vector3(0.018f, 1.95f, 0.045f), frame);
                CreateWorldSizedChild(details.transform, door.transform, "Handle", new Vector3(0.074f, -0.05f, 0.40f), new Vector3(0.035f, 0.34f, 0.060f), accent);
                CreateWorldSizedChild(details.transform, door.transform, "WarningPlate", new Vector3(0.073f, 0.72f, 0f), new Vector3(0.022f, 0.16f, 0.62f), accent);
                CreateWorldSizedChild(details.transform, door.transform, "LowerKickPlate", new Vector3(0.071f, -0.88f, 0f), new Vector3(0.020f, 0.32f, 1.32f), frame);
            }
        }

        private static void CreateWorldSizedChild(Transform parent, Transform scaledReference, string name, Vector3 worldOffset, Vector3 worldSize, Material material)
        {
            GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            detail.name = name;
            detail.transform.SetParent(parent, false);

            Vector3 scale = scaledReference.localScale;
            detail.transform.localPosition = new Vector3(
                worldOffset.x / Mathf.Max(0.001f, scale.x),
                worldOffset.y / Mathf.Max(0.001f, scale.y),
                worldOffset.z / Mathf.Max(0.001f, scale.z));
            detail.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.001f, scale.x),
                worldSize.y / Mathf.Max(0.001f, scale.y),
                worldSize.z / Mathf.Max(0.001f, scale.z));
            if (material != null) detail.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(detail.GetComponent<Collider>());
        }

        private static void AttachSceneComponents(GameObject trainRoot)
        {
            foreach (Light light in trainRoot.GetComponentsInChildren<Light>(true))
            {
                if (light.name == "InteriorLight" && light.GetComponent<TrainLightDynamics>() == null)
                    light.gameObject.AddComponent<TrainLightDynamics>();
            }
        }

        private static void ConvertCarsToPrefabs(Transform trainRoot)
        {
            foreach (string carName in CarNames)
            {
                Transform car = trainRoot.Find(carName);
                if (car == null) continue;
                SaveAndReplaceWithPrefab(car.gameObject, TrainPrefabFolder + "/" + carName + ".prefab", trainRoot);
            }
        }

        private static void ConvertConnectorsToPrefab(Transform trainRoot)
        {
            var connectors = new List<GameObject>();
            for (int i = 0; i < trainRoot.childCount; i++)
            {
                Transform child = trainRoot.GetChild(i);
                if (child.name == "CarConnector") connectors.Add(child.gameObject);
            }
            if (connectors.Count == 0) return;

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
            if (environment == null) return;
            SaveAndReplaceWithPrefab(environment, EnvironmentPrefabFolder + "/FrozenExterior.prefab", null);
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
            if (player == null) return;

            if (player.GetComponent<ProceduralMetalFootsteps>() == null)
                player.AddComponent<ProceduralMetalFootsteps>();

            Camera camera = player.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.dithering = true;
            }
        }

        private static void CreateBaseSceneAtmosphere()
        {
            GameObject existing = GameObject.Find("SCENE_ATMOSPHERE");
            if (existing != null) Object.DestroyImmediate(existing);
            GameObject root = new GameObject("SCENE_ATMOSPHERE");

            VolumeProfile profile = CreatePostProcessProfile();
            GameObject volumeObject = new GameObject("GlobalPostProcessing");
            volumeObject.transform.SetParent(root.transform, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            volume.sharedProfile = profile;

            GetOrCreateParticleMaterial(AtmosphereMaterialFolder + "/Train_SnowParticles.mat");
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
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.contrast.Override(7f);
            color.saturation.Override(-5f);
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.11f);
            vignette.smoothness.Override(0.58f);
            EditorUtility.SetDirty(profile);
            return profile;
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
                material.SetColor("_BaseColor", new Color(0.92f, 0.96f, 1f, 0.72f));
            EditorUtility.SetDirty(material);
            return material;
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