using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Compact wall-ladder interaction for vertical train access.
    /// Decorative rail/rung colliders forward interaction to this component, and destination
    /// positions are resolved onto a nearby solid landing so the player never respawns over a hatch.
    /// </summary>
    public sealed class DeckLadder : InteractableBase
    {
        [SerializeField] private Transform lowerExit;
        [SerializeField] private Transform upperExit;
        [SerializeField] private string lowerToUpperText = "E - Üst kata çık";
        [SerializeField] private string upperToLowerText = "E - Ana kata in";

        private Transform player;
        private bool forwardersBound;

        public override string InteractionText
        {
            get
            {
                Transform actor = ResolvePlayer();
                if (actor == null || lowerExit == null || upperExit == null)
                {
                    return lowerToUpperText;
                }

                float midpoint = (lowerExit.position.y + upperExit.position.y) * 0.5f;
                return actor.position.y < midpoint ? lowerToUpperText : upperToLowerText;
            }
        }

        private void Awake()
        {
            BindColliderForwarders();
        }

        private void OnEnable()
        {
            BindColliderForwarders();
        }

        public void Configure(Transform lower, Transform upper, string upText, string downText)
        {
            lowerExit = lower;
            upperExit = upper;
            lowerToUpperText = upText;
            upperToLowerText = downText;
            BindColliderForwarders();
        }

        public override void Interact(GameObject interactor)
        {
            if (interactor == null || lowerExit == null || upperExit == null)
            {
                return;
            }

            CharacterController controller = interactor.GetComponent<CharacterController>();
            GameObject actor = controller != null ? controller.gameObject : interactor;
            player = actor.transform;

            float midpoint = (lowerExit.position.y + upperExit.position.y) * 0.5f;
            bool goingUp = actor.transform.position.y < midpoint;
            Transform anchor = goingUp ? upperExit : lowerExit;
            Vector3 destination = ResolveSafeLanding(anchor.position);

            bool controllerWasEnabled = controller != null && controller.enabled;
            if (controllerWasEnabled)
            {
                controller.enabled = false;
            }

            actor.transform.SetPositionAndRotation(
                destination,
                Quaternion.Euler(0f, anchor.eulerAngles.y, 0f));
            Physics.SyncTransforms();

            if (controllerWasEnabled)
            {
                controller.enabled = true;
            }
        }

        private Transform ResolvePlayer()
        {
            if (player != null)
            {
                return player;
            }

            GameObject found = GameObject.Find("FPS_Player");
            if (found != null)
            {
                player = found.transform;
            }
            return player;
        }

        /// <summary>
        /// Prefer the authored exit anchor when it already sits on a valid shared landing. Older authored
        /// exits may still be close to a hatch, so only then search nearby surfaces toward the public aisle.
        /// </summary>
        private Vector3 ResolveSafeLanding(Vector3 anchor)
        {
            if (TryWalkableLanding(anchor, anchor.y, out Vector3 direct))
            {
                return direct;
            }

            float centreDirection = Mathf.Abs(anchor.z) > 0.05f ? -Mathf.Sign(anchor.z) : 1f;
            Vector3 towardCorridor = new Vector3(0f, 0f, centreDirection);
            Vector3 awayFromLadder = Vector3.zero;
            if (transform.parent != null)
            {
                awayFromLadder = anchor - transform.parent.position;
                awayFromLadder.y = 0f;
                if (awayFromLadder.sqrMagnitude > 0.001f)
                {
                    awayFromLadder.Normalize();
                }
            }

            Vector3 side = Vector3.Cross(Vector3.up, towardCorridor).normalized;
            Vector3[] offsets =
            {
                towardCorridor * 1.35f,
                towardCorridor * .95f,
                awayFromLadder * .80f,
                awayFromLadder * 1.25f,
                towardCorridor * 1.35f + side * .45f,
                towardCorridor * 1.35f - side * .45f,
                side * .85f,
                -side * .85f,
                Vector3.zero,
            };

            foreach (Vector3 offset in offsets)
            {
                Vector3 candidate = anchor + offset;
                if (TryWalkableLanding(candidate, anchor.y, out Vector3 resolved))
                {
                    return resolved;
                }
            }

            return anchor + towardCorridor * 1.20f + Vector3.up * .035f;
        }

        private static bool TryWalkableLanding(Vector3 candidate, float expectedY, out Vector3 landing)
        {
            Vector3 rayOrigin = candidate + Vector3.up * .70f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.45f, ~0, QueryTriggerInteraction.Ignore)
                && hit.normal.y >= .72f
                && Mathf.Abs(hit.point.y - expectedY) <= .42f)
            {
                landing = hit.point + Vector3.up * .035f;
                return true;
            }

            landing = default;
            return false;
        }

        /// <summary>
        /// Rungs and rails use their own primitive colliders. If those colliders are hit before the hidden
        /// interaction box, forward them to the same ladder action instead of losing the E prompt.
        /// Runtime-only binding avoids serializing helper MonoBehaviours into generated prefabs.
        /// </summary>
        private void BindColliderForwarders()
        {
            if (!Application.isPlaying || forwardersBound || transform.parent == null)
            {
                return;
            }

            // Bind only this ladder's rails and rungs, never nearby doors or
            // the vestibule shell's colliders.
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                if (col == null || col.gameObject == gameObject)
                {
                    continue;
                }

                DeckLadderForwarder proxy = col.GetComponent<DeckLadderForwarder>();
                if (proxy == null)
                {
                    proxy = col.gameObject.AddComponent<DeckLadderForwarder>();
                }
                proxy.Configure(this);
            }
            forwardersBound = true;
        }
    }

    /// <summary>Runtime-only forwarding component added to ladder rail/rung colliders.</summary>
    internal sealed class DeckLadderForwarder : InteractableBase
    {
        private DeckLadder target;

        public override string InteractionText => target != null ? target.InteractionText : "E - Merdiveni kullan";

        public void Configure(DeckLadder ladder)
        {
            target = ladder;
        }

        public override void Interact(GameObject interactor)
        {
            if (target != null)
            {
                target.Interact(interactor);
            }
        }
    }
}
