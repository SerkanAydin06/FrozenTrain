#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Internal footprint normalizer used before final prefab authoring.
    /// It spreads existing authored content into the 18m x 9.2m footprint without stretching props.
    /// </summary>
    public static class FrozenTrainAuthoredLayoutResize
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const float TargetLength = 18.0f;
        private const float TargetWidth = 9.2f;
        private const float TargetSpacing = 18.8f;

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

        public static void ResizeAuthoredTrain()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TrainPrefabFolder + "/01_Locomotive.prefab") == null)
                FrozenTrainScenePrefabMigration.ConvertToScenePrefabs();

            foreach (string carName in CarNames)
                ResizeCarPrefab(TrainPrefabFolder + "/" + carName + ".prefab");

            ResizeScenePlacements();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ResizeCarPrefab(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform floor = FindDirectChild(root.transform, "Floor");
                if (floor == null)
                {
                    Debug.LogWarning("FrozenTrain: Floor marker not found in " + path);
                    return;
                }

                float currentLength = Mathf.Max(0.01f, Mathf.Abs(floor.localScale.x));
                float currentWidth = Mathf.Max(0.01f, Mathf.Abs(floor.localScale.z));
                if (Mathf.Abs(currentLength - TargetLength) < 0.05f && Mathf.Abs(currentWidth - TargetWidth) < 0.05f)
                    return;

                float lengthFactor = TargetLength / currentLength;
                float widthFactor = TargetWidth / currentWidth;

                Transform[] descendants = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => t != root.transform && t != floor)
                    .ToArray();
                Dictionary<Transform, Vector3> positions = descendants.ToDictionary(t => t, t => root.transform.InverseTransformPoint(t.position));

                foreach (Transform t in descendants.OrderBy(GetDepth))
                {
                    Vector3 p = positions[t];
                    p.x *= lengthFactor;
                    p.z *= widthFactor;
                    t.position = root.transform.TransformPoint(p);
                }

                floor.localPosition = Vector3.zero;
                floor.localRotation = Quaternion.identity;
                floor.localScale = new Vector3(TargetLength, floor.localScale.y, TargetWidth);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ResizeScenePlacements()
        {
            if (!System.IO.File.Exists(ScenePath)) return;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (trainRoot == null) return;

            for (int i = 0; i < CarNames.Length; i++)
            {
                Transform car = trainRoot.transform.Find(CarNames[i]);
                if (car == null) continue;
                Vector3 p = car.localPosition;
                p.x = TargetCenters[i];
                p.z = 0f;
                car.localPosition = p;
            }

            List<Transform> connectors = new List<Transform>();
            for (int i = 0; i < trainRoot.transform.childCount; i++)
            {
                Transform child = trainRoot.transform.GetChild(i);
                if (child.name == "CarConnector") connectors.Add(child);
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

        private static Transform FindDirectChild(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name == name) return root.GetChild(i);
            return null;
        }

        private static int GetDepth(Transform transform)
        {
            int depth = 0;
            Transform current = transform.parent;
            while (current != null) { depth++; current = current.parent; }
            return depth;
        }
    }
}
#endif
