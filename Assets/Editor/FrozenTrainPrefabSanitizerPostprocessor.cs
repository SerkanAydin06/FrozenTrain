#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Removes first-run single-deck shell/light objects once a car has received
    /// MultiDeckArchitecture. This prevents hidden solid walls from covering authored
    /// windows and prevents legacy point lights from stacking with level lighting.
    /// </summary>
    public sealed class FrozenTrainPrefabSanitizerPostprocessor : AssetPostprocessor
    {
        private static readonly HashSet<string> CarPaths = new HashSet<string>
        {
            "Assets/Prefabs/Train/01_Locomotive.prefab",
            "Assets/Prefabs/Train/02_Generator.prefab",
            "Assets/Prefabs/Train/03_Living.prefab",
            "Assets/Prefabs/Train/04_Workshop.prefab",
            "Assets/Prefabs/Train/05_Greenhouse.prefab"
        };

        private static readonly HashSet<string> LegacyDirectNames = new HashSet<string>
        {
            "WallLeft", "WallRight", "WallLower", "WallUpper", "WallPillar",
            "WindowTopFrame", "WindowBottomFrame", "WindowLeftFrame", "WindowRightFrame", "WindowGlass",
            "CeilingLightHousing", "CeilingLightStrip", "InteriorLight"
        };

        private static bool applying;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (applying) return;

            foreach (string path in importedAssets.Where(CarPaths.Contains))
            {
                Sanitize(path);
            }
        }

        private static void Sanitize(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;

            applying = true;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform architecture = FindDirect(root.transform, "MultiDeckArchitecture");
                if (architecture == null || FindDirect(architecture, "LegacySingleDeckSanitized") != null)
                {
                    return;
                }

                bool changed = false;
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = root.transform.GetChild(i);
                    if (!LegacyDirectNames.Contains(child.name)) continue;
                    Object.DestroyImmediate(child.gameObject);
                    changed = true;
                }

                GameObject marker = new GameObject("LegacySingleDeckSanitized");
                marker.transform.SetParent(architecture, false);
                changed = true;

                if (changed)
                    PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
                applying = false;
            }
        }

        private static Transform FindDirect(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name == name) return root.GetChild(i);
            return null;
        }
    }
}
#endif
