using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FpsPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.2f;
        [SerializeField] private float sprintSpeed = 6.8f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float airControl = 0.35f;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.08f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Jump / Gravity")]
        [SerializeField] private float jumpHeight = 1.15f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float pitch;
        private bool cursorCaptured = true;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>(true);
            }
        }

        private void Start()
        {
            SetCursorCaptured(true);
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return;
            }

            HandleCursorToggle();

            if (cursorCaptured)
            {
                HandleLook();
            }

            HandleMovement();
        }

        public void SetCursorCaptured(bool captured)
        {
            cursorCaptured = captured;
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        private void HandleCursorToggle()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorCaptured(!cursorCaptured);
            }

            if (!cursorCaptured && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorCaptured(true);
            }
        }

        private void HandleLook()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float yawDelta = mouseDelta.x * mouseSensitivity;
            float pitchDelta = mouseDelta.y * mouseSensitivity;

            transform.Rotate(0f, yawDelta, 0f, Space.Self);

            pitch = Mathf.Clamp(pitch - pitchDelta, minPitch, maxPitch);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            Vector2 moveInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;

            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            bool grounded = characterController.isGrounded;
            bool sprinting = grounded &&
                              (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) &&
                              moveInput.y > 0.1f;

            float targetSpeed = sprinting ? sprintSpeed : walkSpeed;
            Vector3 desiredDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            Vector3 targetVelocity = desiredDirection * targetSpeed;

            float control = grounded ? 1f : airControl;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                acceleration * control * Time.deltaTime);

            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }

            if (grounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(motion * Time.deltaTime);
        }
    }
}
