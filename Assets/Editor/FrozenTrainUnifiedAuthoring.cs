#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder owns the five 24 m cars; final passes
    /// create the right-side public corridor in cars 2-5, replace the old connector with four full
    /// inter-car vestibules containing all normal vertical circulation, then apply screenshot-driven
    /// visual placement and stair/lighting corrections without changing gameplay ownership.
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
            FrozenTrainPost24ScreenshotPolish.Apply();
            FrozenTrainFinalScreenshotFix.Apply();
            FrozenTrainMinimalInteriorPass.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: five empty 24 m cars + four inter-car vestibules, keeping only doors, stairs, structural partitions and minimal visibility lighting.");
        }
    }
}
#endif
