#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train, the production-art
    /// pass upgrades materials/details, and the final upper-deck circulation pass guarantees that ladders
    /// arrive in a shared public vestibule rather than a private sleeping cabin.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            FrozenTrainProductionArt.Apply();
            FrozenTrainUpperDeckCirculation.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: canonical 5-car layout + production art + shared upper-deck circulation applied.");
        }
    }
}
#endif
