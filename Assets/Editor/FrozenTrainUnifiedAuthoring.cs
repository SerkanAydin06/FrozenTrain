#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train and production/QA passes
    /// finish the generated content; final architecture then converts the main deck to one straight right-side
    /// public corridor with one opaque closed functional room on the left of every car.
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
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: right-side two-person transit corridor + opaque single-door functional compartments + private upper residential decks applied.");
        }
    }
}
#endif
