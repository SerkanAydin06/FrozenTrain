#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. Visual architecture is materialized into prefabs/scenes;
    /// runtime code only controls gameplay systems.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";

        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            EnsureBaseAssets();
            FrozenTrainAuthoredLayoutResize.ResizeAuthoredTrain();

            // Rebuild the human/work main-deck content first.
            FrozenTrainFiveCarDesignAuthoring.ApplyAll();

            // Final authority: 18m x 9.2m, three full-size decks, continuous upper/lower corridors.
            FrozenTrainArchitectureRevision.ApplyAll();

            // Bind interactions after final objects have been created/moved.
            FrozenTrainPhysicalSystemsAuthoring.InstallOrRefresh();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain authored train rebuilt: 18m x 9.2m cars, full lower technical deck, human/work main deck, continuous upper sleeping corridor.");
        }

        private static void EnsureBaseAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TrainPrefabFolder + "/01_Locomotive.prefab") == null)
            {
                FrozenTrainScenePrefabMigration.ConvertToScenePrefabs();
            }
        }
    }
}
#endif
