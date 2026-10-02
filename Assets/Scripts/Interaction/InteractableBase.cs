using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Shared contract for all FPS interactions such as doors, consoles and switches.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour
    {
        public abstract string InteractionText { get; }
        public abstract void Interact(GameObject interactor);
    }
}
