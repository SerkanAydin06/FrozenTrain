#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train and production/QA passes
    /// finish generated content. Final passes repair persistent exterior materials and enforce the locomotive
    /// private-cab exception, clean right-side corridors in cars 2-5, and side-mounted residential access.
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
            FrozenTrainExteriorMaterialFix.Apply();
            FrozenTrainScreenshotQASafety.Apply();
            FrozenTrainMainStreetArchitecture.Apply();
            FrozenTrainMainStreetConnector.Apply();
            FrozenTrainMainStreetSafety.Apply();
            FrozenTrainCirculationFinalizer.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: private locomotive + clean right corridors + side residential landings + persistent URP exterior materials applied.");
        }
    }
}
#endif
