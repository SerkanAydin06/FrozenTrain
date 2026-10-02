using FrozenTrain.Interaction;
using UnityEngine;

namespace FrozenTrain.Management
{
    /// <summary>
    /// Physical in-world terminal that opens the same portable management panel.
    /// </summary>
    public sealed class ManagementTerminal : InteractableBase
    {
        public override string InteractionText => "E - Tren Yönetimini Aç";

        public override void Interact(GameObject interactor)
        {
            TrainManagementPanel panel = interactor.GetComponent<TrainManagementPanel>();
            if (panel != null)
            {
                panel.OpenPanel();
            }
        }
    }
}
