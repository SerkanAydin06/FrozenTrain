#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train, production art upgrades
    /// the generated assets, upper circulation guarantees public landings/corridors, and the final screenshot
    /// QA pass applies presentation fixes that were verified from the latest automatic review images.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            FrozenTrainProductionArt.Apply();
            FrozenTrainUpperDeckCirculation.Apply();
            FrozenTrainUpperDeckFinalPolish.Apply();
            FrozenTrainScreenshotQARefinement.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: canonical 5-car layout + production art + public upper circulation + screenshot QA refinement applied.");
        }
    }
}
#endif
