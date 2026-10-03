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
    /// Resizes the authored five-car train without moving visual construction back into runtime code.
    /// If prefabs do not exist yet, the scene/prefab migration is run first, then the saved assets are resized.
    /// </summary>
    public static class FrozenTrainAuthoredLayoutResize
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const string ConnectorPrefabPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";

        private const float SourceLength = 15f;
        private const float SourceWidth = 7.2f;
        private const float TargetLength = 16.5f;
        private const float TargetWidth = 8.4f;
        private const float TargetSpacing = 17.3f;

        private const float LengthFactor = TargetLength / SourceLength;
        private const float WidthFactor = TargetWidth / SourceWidth;

        private static readonly string[] CarNames =
        {
            "01_Locomotive",
            "02_Generator",
            "03_Living",
            "04_Workshop",
            "05_Greenhouse"
        };

        private static readonly float[] TargetCenters =
        {
            -2f * TargetSpacing,
            -TargetSpacing,
            0f,
            TargetSpacing,
            2f * TargetSpacing
        };

        [MenuItem("Frozen Train/Resize Authored Train (16.5m x 8.4m)", priority = 2)]
        public static void ResizeAuthoredTrain()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TrainPrefabFolder + "/01_Locomotive.prefab") == null)
            {
                FrozenTrainScenePrefabMigration.ConvertToScenePrefabs();
            }

            for (int i = 0; i < CarNames.Length; i++)
            {
                ResizeCarPrefab(TrainPrefabFolder + "/" + CarNames[i] + ".prefab");
            }

            ResizeConnectorPrefab();
            ResizeScenePlacements();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain authored layout resized to 16.5m length x 8.4m width per car. Five-car count preserved.");
        }

        private static void ResizeCarPrefab(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform floor = FindDirectChild(root.transform, "Floor");
                if (floor == null)
                {
                    Debug.LogWarning("FrozenTrain: Floor not found in " + path);
                    return;
                }

                // Idempotent: do not scale the same authored prefab twice.
                if (floor.localScale.x >= TargetLength - 0.05f && floor.localScale.z >= TargetWidth - 0.05f)
                {
                    return;
                }

                Transform[] descendants = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => t != root.transform)
                    .ToArray();

                var originalRootPositions = new Dictionary<Transform, Vector3>(descendants.Length);
                foreach (Transform t in descendants)
                {
                    originalRootPositions[t] = root.transform.InverseTransformPoint(t.position);
                }

                // Spread every authored object through the larger floor plan without stretching props.
                foreach (Transform t in descendants.OrderBy(GetDepth))
                {
                    Vector3 p = originalRootPositions[t];
                    p.x *= LengthFactor;
                    p.z *= WidthFactor;
                    t.position = root.transform.TransformPoint(p);
                }

                // Only structural shell pieces are physically enlarged. Furniture/machinery keep real-world size.
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform child = root.transform.GetChild(i);
                    if (!IsStructuralShell(child.name))
                    {
                        continue;
                    }

                    Vector3 scale = child.localScale;
                    scale.x *= LengthFactor;
                    scale.z *= WidthFactor;
                    child.localScale = scale;
                }

                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                {
                    light.range *= 1.08f;
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ResizeConnectorPrefab()
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPrefabPath);
            if (asset == null)
            {
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPrefabPath);
            try
            {
                Transform floor = FindDirectChild(root.transform, "ConnectorFloor");
                if (floor != null && floor.localScale.z >= 2.45f)
                {
                    return;
                }

                Transform[] descendants = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => t != root.transform)
                    .ToArray();

                var positions = new Dictionary<Transform, Vector3>(descendants.Length);
                foreach (Transform t in descendants)
                {
                    positions[t] = root.transform.InverseTransformPoint(t.position);
                }

                foreach (Transform t in descendants.OrderBy(GetDepth))
                {
                    Vector3 p = positions[t];
                    p.z *= WidthFactor;
                    t.position = root.transform.TransformPoint(p);
                }

                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform child = root.transform.GetChild(i);
                    if (child.name == "ConnectorFloor" ||
                        child.name == "ConnectorCeiling" ||
                        child.name == "BellowsLeft" ||
                        child.name == "BellowsRight" ||
                        child.name == "SlidingDoor")
                    {
                        Vector3 scale = child.localScale;
                        scale.z *= WidthFactor;
                        child.localScale = scale;
                    }
                }

                SlidingDoor door = root.GetComponentInChildren<SlidingDoor>(true);
                if (door != null)
                {
                    door.Configure(new Vector3(0f, 0f, -2.22f), 2.8f);
                }

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ResizeScenePlacements()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (trainRoot == null)
            {
                Debug.LogError("FrozenTrain: TRAIN_INTERIOR_PROTOTYPE not found in FPSPrototype.unity.");
                return;
            }

            for (int i = 0; i < CarNames.Length; i++)
            {
                Transform car = trainRoot.transform.Find(CarNames[i]);
                if (car == null)
                {
                    continue;
                }

                Vector3 p = car.localPosition;
                p.x = TargetCenters[i];
                p.z = 0f;
                car.localPosition = p;
            }

            List<Transform> connectors = new List<Transform>();
            for (int i = 0; i < trainRoot.transform.childCount; i++)
            {
                Transform child = trainRoot.transform.GetChild(i);
                if (child.name == "CarConnector")
                {
                    connectors.Add(child);
                }
            }

            connectors.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
            for (int i = 0; i < connectors.Count && i < CarNames.Length - 1; i++)
            {
                Vector3 p = connectors[i].localPosition;
                p.x = (TargetCenters[i] + TargetCenters[i + 1]) * 0.5f;
                p.z = 0f;
                connectors[i].localPosition = p;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static bool IsStructuralShell(string name)
        {
            return name == "Floor" ||
                   name == "Ceiling" ||
                   name.StartsWith("Wall") ||
                   name.StartsWith("Window") ||
                   name.StartsWith("CabFront") ||
                   name.StartsWith("CabWindshield") ||
                   name.StartsWith("Bulkhead") ||
                   name.StartsWith("RearEnd") ||
                   name.StartsWith("CeilingRib") ||
                   name.StartsWith("BaseTrim") ||
                   name.StartsWith("CeilingDuct") ||
                   name.StartsWith("CeilingLightHousing") ||
                   name.StartsWith("CeilingLightStrip");
        }

        private static Transform FindDirectChild(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        private static int GetDepth(Transform transform)
        {
            int depth = 0;
            Transform current = transform.parent;
            while (current != null)
            {
                depth++;
                current = current.parent;
            }
            return depth;
        }
    }
}
#endif