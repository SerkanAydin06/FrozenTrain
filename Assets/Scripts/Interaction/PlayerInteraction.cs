using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Interaction
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 2.8f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private InteractableBase currentTarget;
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

            const float width = 310f;
            const float height = 48f;
            Rect backgroundRect = new Rect(cx - width * 0.5f, cy + 34f, width, height);
            GUI.DrawTexture(backgroundRect, promptBackground);

            Rect keyRect = new Rect(backgroundRect.x + 9f, backgroundRect.y + 8f, 32f, 32f);
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

            promptBackground = SolidTexture(new Color(0.025f, 0.035f, 0.045f, 0.90f));
            keyBackground = SolidTexture(new Color(0.72f, 0.24f, 0.055f, 0.96f));
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
