#if UNITY_EDITOR
namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Stable production-art entry point used by the unified authoring command.
    /// The implementation lives in FrozenTrainProductionArtPro and covers all five cars.
    /// </summary>
    public static class FrozenTrainProductionArt
    {
        public static void Apply()
        {
            FrozenTrainProductionArtPro.Apply();
        }
    }
}
#endif
