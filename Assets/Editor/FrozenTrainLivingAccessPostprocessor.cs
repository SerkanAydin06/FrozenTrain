#if UNITY_EDITOR
namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Compatibility shim retained so the existing Unity meta GUID remains stable.
    /// Living-car floor openings and vertical access are now authored by
    /// FrozenTrainArchitectureRevision using compact wall ladders.
    /// </summary>
    public sealed class FrozenTrainLivingAccessPostprocessor
    {
    }
}
#endif
