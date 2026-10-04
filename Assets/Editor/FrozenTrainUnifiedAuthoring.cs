#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder owns the five 24 m cars; final passes
    /// create the right-side public corridor in cars 2-5 and replace the old connector with four full
    /// inter-car vestibules containing all normal vertical circulation.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            FrozenTrainProductionArt.Apply();
            FrozenTrainScreenshotQARefinement.Apply();
            FrozenTrainExteriorMaterialFix.Apply();
            FrozenTrainScreenshotQASafety.Apply();
            FrozenTrainMainStreetArchitecture.Apply();
            FrozenTrainMainStreetConnector.Apply();
            FrozenTrainMainStreetSafety.Apply();
            FrozenTrainLongVestibuleArchitecture.Apply();
            FrozenTrainLongLightingPass.Apply();
            FrozenTrainLongExteriorPass.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: five 24 m cars + four 4 m inter-car vestibules + all stairs outside usable car rooms applied.");
        }
    }
}
#endif