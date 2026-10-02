using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Simple train sliding door for the FPS prototype.
    /// </summary>
    public sealed class SlidingDoor : InteractableBase
    {
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, -1.55f);
        [SerializeField] private float moveSpeed = 2.8f;

        private Vector3 closedLocalPosition;
        private Vector3 targetLocalPosition;
        private bool isOpen;

        public override string InteractionText => isOpen ? "E - Kapat" : "E - Aç";

        private void Awake()
        {
            closedLocalPosition = transform.localPosition;
            targetLocalPosition = closedLocalPosition;
        }

        public void Configure(Vector3 localOpenOffset, float speed)
        {
            openOffset = localOpenOffset;
            moveSpeed = Mathf.Max(0.1f, speed);
        }

        private void Update()
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetLocalPosition,
                moveSpeed * Time.deltaTime);
        }

        public override void Interact(GameObject interactor)
        {
            isOpen = !isOpen;
            targetLocalPosition = closedLocalPosition + (isOpen ? openOffset : Vector3.zero);
        }
    }
}
