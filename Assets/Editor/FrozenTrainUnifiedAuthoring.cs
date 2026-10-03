#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The train is described by FrozenTrainLayout and
    /// rebuilt from scratch by FrozenTrainBuilder; runtime code only controls gameplay systems.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt from FrozenTrainLayout: 5 cars, 18m x 9.2m, technical / main / sleeping decks.");
        }
    }
}
#endif
