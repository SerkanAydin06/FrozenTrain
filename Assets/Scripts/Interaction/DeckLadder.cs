using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Compact wall-ladder interaction for vertical train access.
    /// Keeps ladders space-efficient while making upper/basement decks reliably usable
    /// with the existing FPS CharacterController.
    /// </summary>
    public sealed class DeckLadder : InteractableBase
    {
        [SerializeField] private Transform lowerExit;
        [SerializeField] private Transform upperExit;
        [SerializeField] private string lowerToUpperText = "E - Üst kata çık";
        [SerializeField] private string upperToLowerText = "E - Ana kata in";

        private Transform player;

        public override string InteractionText
        {
            get
            {
                Transform actor = player;
                if (actor == null)
                {
                    GameObject found = GameObject.Find("FPS_Player");
                    if (found != null)
                    {
                        actor = found.transform;
                        player = actor;
                    }
                }

                if (actor == null || lowerExit == null || upperExit == null)
                {
                    return lowerToUpperText;
                }

                float midpoint = (lowerExit.position.y + upperExit.position.y) * 0.5f;
                return actor.position.y < midpoint ? lowerToUpperText : upperToLowerText;
            }
        }

        public void Configure(Transform lower, Transform upper, string upText, string downText)
        {
            lowerExit = lower;
            upperExit = upper;
            lowerToUpperText = upText;
            upperToLowerText = downText;
        }

        public override void Interact(GameObject interactor)
        {
            if (interactor == null || lowerExit == null || upperExit == null)
            {
                return;
            }

            player = interactor.transform;
            float midpoint = (lowerExit.position.y + upperExit.position.y) * 0.5f;
            Transform destination = interactor.transform.position.y < midpoint ? upperExit : lowerExit;

            CharacterController controller = interactor.GetComponent<CharacterController>();
            bool controllerWasEnabled = controller != null && controller.enabled;
            if (controllerWasEnabled)
            {
                controller.enabled = false;
            }

            interactor.transform.position = destination.position;
            interactor.transform.rotation = Quaternion.Euler(0f, destination.eulerAngles.y, 0f);

            if (controllerWasEnabled)
            {
                controller.enabled = true;
            }
        }
    }
}
