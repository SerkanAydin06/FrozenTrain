namespace FrozenTrain.Atmosphere
{
    /// <summary>
    /// Legacy compatibility shim. Camera post-processing is serialized directly on the
    /// PlayerCamera by the scene/prefab authoring migration.
    /// </summary>
    [System.Obsolete("Camera post-processing is scene-authored.")]
    public static class UltraCameraPostEnable
    {
    }
}
