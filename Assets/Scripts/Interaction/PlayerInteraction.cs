using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Raycasts from the center of the FPS camera and interacts with nearby objects using E.
    /// A lightweight OnGUI prompt is used for the prototype so no UI prefab is required yet.
    /// </summary>
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 2.8f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private InteractableBase currentTarget;
        private GUIStyle promptStyle;
        private GUIStyle crosshairStyle;

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
            currentTarget = null;

            if (playerCamera == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Ignore))
            {
                currentTarget = hit.collider.GetComponentInParent<InteractableBase>();
            }

            if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentTarget.Interact(gameObject);
            }
        }

        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            EnsureStyles();

            Rect crosshairRect = new Rect(Screen.width * 0.5f - 10f, Screen.height * 0.5f - 12f, 20f, 24f);
            GUI.Label(crosshairRect, "+", crosshairStyle);

            if (currentTarget == null)
            {
                return;
            }

            const float width = 280f;
            const float height = 42f;
            Rect promptRect = new Rect(
                Screen.width * 0.5f - width * 0.5f,
                Screen.height * 0.5f + 28f,
                width,
                height);

            GUI.Label(promptRect, currentTarget.InteractionText, promptStyle);
        }

        private void EnsureStyles()
        {
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
            }

            if (crosshairStyle == null)
            {
                crosshairStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.85f) }
                };
            }
        }
    }
}
