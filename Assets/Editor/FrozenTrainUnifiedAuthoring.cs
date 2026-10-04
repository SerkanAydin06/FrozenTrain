#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. The train is described by FrozenTrainLayout and
    /// rebuilt from scratch by FrozenTrainBuilder; production art is then applied as a visual-only skin pass.
    /// Runtime code only controls gameplay systems.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            FrozenTrainProductionArt.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: canonical 5-car layout + free production-art pass applied.");
        }
    }
}
#endif
