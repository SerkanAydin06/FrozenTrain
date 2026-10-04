#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The canonical builder creates the train, production art upgrades
    /// the generated assets, legacy upper circulation/QA passes finish existing content, then Main Street is
    /// applied as the final scalable architecture: lower infrastructure, main public transit, upper residential.
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
            Debug.Log("FrozenTrain rebuilt: 5-car production train + Main Street public spine + widened connectors + private residential upper decks applied.");
        }
    }
}
#endif
