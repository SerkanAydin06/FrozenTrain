#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train and production/QA passes
    /// finish generated content. The final circulation authority then enforces the locomotive/private-cab
    /// exception, clean right-side passenger corridors in cars 2-5, and enclosed residential stairwells.
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
            FrozenTrainScreenshotQASafety.Apply();
            FrozenTrainMainStreetArchitecture.Apply();
            FrozenTrainMainStreetConnector.Apply();
            FrozenTrainMainStreetSafety.Apply();
            FrozenTrainCirculationFinalizer.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: full private locomotive + clean right-side corridors in cars 2-5 + enclosed residential stairwells applied.");
        }
    }
}
#endif
