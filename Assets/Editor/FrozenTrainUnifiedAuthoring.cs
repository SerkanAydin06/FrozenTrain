#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Single public authoring entry point. Order matters:
    /// 1. Builder: five separate 24 m car prefabs with end doors and in-car ladders (CarAccess),
    ///    the short main-deck gangway connector, scene and player.
    /// 2. ProductionArtPro: FT_PRO_* materials/textures (used by later passes) and shell/light tuning.
    /// 3. ExteriorScenery: mountains, rocks, snow drifts and rail gauge in the scene.
    /// 4. MainStreet*: +Z public corridor and partition colliders.
    /// 5. LongVestibule (historic name): final per-car room layout.
    /// 6. MinimalInterior (final authority): strips dressing, assigns baseline material slots,
    ///    builds cabin doors/minimal exterior and refreshes scene prefab instances.
    /// </summary>
    public static class FrozenTrainUnifiedAuthoring
    {
        [MenuItem("Frozen Train/Rebuild Authored Train", priority = 1)]
        public static void RebuildAuthoredTrain()
        {
            if (!FrozenTrainBuilder.Build()) return;
            FrozenTrainProductionArtPro.Apply();
            FrozenTrainExteriorScenery.Apply();
            FrozenTrainMainStreetArchitecture.Apply();
            FrozenTrainMainStreetSafety.Apply();
            FrozenTrainLongVestibuleArchitecture.Apply();
            FrozenTrainMinimalInteriorPass.Apply();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain rebuilt: five coupled cars, main-deck gangways, in-car upper ladders and a locomotive technical ladder.");
        }
    }
}
#endif
