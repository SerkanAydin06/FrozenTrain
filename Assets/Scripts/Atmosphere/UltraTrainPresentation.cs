using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    /// <summary>
    /// Legacy compatibility shim. Visual atmosphere is now authored directly in
    /// FPSPrototype.unity by FrozenTrainBuilder and this component does nothing.
    /// </summary>
    [System.Obsolete("Atmosphere is scene-authored. Do not add this component.")]
    public sealed class UltraTrainPresentation : MonoBehaviour
    {
    }
}
