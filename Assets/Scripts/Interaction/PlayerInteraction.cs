using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Interaction
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 3.0f;
        [SerializeField] private float aimAssistRadius = 0.16f;
        [SerializeField] private float targetGraceSeconds = 0.28f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[20];
        private InteractableBase currentTarget;
        private Collider currentTargetCollider;
        private float lastTargetSeenTime = -10f;

        private GUIStyle promptStyle;
        private GUIStyle keyStyle;
        private Texture2D promptBackground;
        private Texture2D keyBackground;
        private Texture2D crosshairTexture;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>(true);
            }
        }

        public void Configure(Camera cameraReference, float distance)
        {
            playerCamera = cameraReference;
            interactionDistance = Mathf.Max(0.5f, distance);
        }

        private void Update()
        {
            if (playerCamera == null || Cursor.lockState != CursorLockMode.Locked)
            {
                currentTarget = null;
                currentTargetCollider = null;
                return;
            }

            InteractableBase detected = FindBestTarget(out Collider detectedCollider);
            if (detected != null)
            {
                currentTarget = detected;
                currentTargetCollider = detectedCollider;
                lastTargetSeenTime = Time.unscaledTime;
            }
            else if (currentTarget != null)
            {
                bool graceExpired = Time.unscaledTime - lastTargetSeenTime > targetGraceSeconds;
                bool tooFar = Vector3.Distance(playerCamera.transform.position, currentTarget.transform.position) > interactionDistance + 1.25f;
                if (graceExpired || tooFar || !HasClearSight(currentTargetCollider, currentTarget))
                {
                    currentTarget = null;
                    currentTargetCollider = null;
                }
            }

            if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentTarget.Interact(gameObject);
                lastTargetSeenTime = Time.unscaledTime;
            }
        }

        private InteractableBase FindBestTarget(out Collider bestCollider)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            int count = Physics.SphereCastNonAlloc(
                ray, aimAssistRadius, hitBuffer, interactionDistance,
                interactionMask, QueryTriggerInteraction.Ignore);

            InteractableBase best = null;
            bestCollider = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hitBuffer[i];
                if (hit.collider == null || hit.distance >= bestDistance) continue;

                InteractableBase target = hit.collider.GetComponentInParent<InteractableBase>();
                if (target == null || !HasClearSight(hit.collider, target)) continue;

                bestDistance = hit.distance;
                best = target;
                bestCollider = hit.collider;
            }
            return best;
        }

        // Aim assistance never bypasses a wall. Check the actual surface and all
        // ordinary physics layers, even if a narrower interaction mask is configured.
        private bool HasClearSight(Collider collider, InteractableBase target)
        {
            if (playerCamera == null || collider == null || target == null) return false;
            Vector3 origin = playerCamera.transform.position;
            Vector3 toTarget = collider.ClosestPoint(origin) - origin;
            float distance = toTarget.magnitude;
            if (distance < 0.001f) return true;

            if (!Physics.Raycast(origin, toTarget / distance, out RaycastHit firstHit,
                distance + 0.05f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;

            return firstHit.collider == collider ||
                firstHit.collider.GetComponentInParent<InteractableBase>() == target;
        }

        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            EnsureStyles();

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            GUI.DrawTexture(new Rect(cx - 2f, cy - 2f, 4f, 4f), crosshairTexture);

            if (currentTarget == null)
            {
                return;
            }

            string text = currentTarget.InteractionText;
            if (text.StartsWith("E - "))
            {
                text = text.Substring(4);
            }

            const float width = 340f;
            const float height = 50f;
            Rect backgroundRect = new Rect(cx - width * 0.5f, cy + 38f, width, height);
            GUI.DrawTexture(backgroundRect, promptBackground);

            Rect keyRect = new Rect(backgroundRect.x + 9f, backgroundRect.y + 9f, 32f, 32f);
            GUI.DrawTexture(keyRect, keyBackground);
            GUI.Label(keyRect, "E", keyStyle);

            Rect textRect = new Rect(backgroundRect.x + 50f, backgroundRect.y + 4f, width - 60f, height - 8f);
            GUI.Label(textRect, text, promptStyle);
        }

        private void EnsureStyles()
        {
            if (promptStyle != null)
            {
                return;
            }

            promptBackground = SolidTexture(new Color(0.025f, 0.035f, 0.045f, 0.92f));
            keyBackground = SolidTexture(new Color(0.72f, 0.24f, 0.055f, 0.98f));
            crosshairTexture = SolidTexture(new Color(0.88f, 0.94f, 1f, 0.82f));

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.90f, 0.94f, 0.97f) }
            };

            keyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private static Texture2D SolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
