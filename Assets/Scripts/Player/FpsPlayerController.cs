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
        [SerializeField] private float walkSpeed = 4.0f;
        [SerializeField] private float sprintSpeed = 6.2f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float airControl = 0.30f;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.075f;
        [SerializeField] private float minPitch = -84f;
        [SerializeField] private float maxPitch = 84f;

        [Header("Camera Feel")]
        [SerializeField] private float walkBobFrequency = 7.2f;
        [SerializeField] private float sprintBobFrequency = 9.2f;
        [SerializeField] private float walkBobAmplitude = 0.020f;
        [SerializeField] private float sprintBobAmplitude = 0.030f;
        [SerializeField] private float trainSwayAmplitude = 0.010f;
        [SerializeField] private float trainRollDegrees = 0.22f;
        [SerializeField] private float cameraReturnSharpness = 12f;
        [SerializeField] private float sprintFovBoost = 2.0f;

        [Header("Jump / Gravity")]
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private Vector3 cameraBaseLocalPosition;
        private float verticalVelocity;
        private float pitch;
        private float bobPhase;
        private float baseFov;
        private bool cursorCaptured = true;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>(true);
            }

            if (playerCamera != null)
            {
                cameraBaseLocalPosition = playerCamera.transform.localPosition;
                baseFov = playerCamera.fieldOfView;
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

        private void LateUpdate()
        {
            UpdateCameraFeel();
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
            characterController.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void UpdateCameraFeel()
        {
            if (playerCamera == null || characterController == null)
            {
                return;
            }

            Vector3 planarVelocity = characterController.velocity;
            planarVelocity.y = 0f;
            float speed = planarVelocity.magnitude;
            float speed01 = Mathf.Clamp01(speed / sprintSpeed);
            bool grounded = characterController.isGrounded;
            bool moving = grounded && speed > 0.15f;
            bool sprinting = speed > walkSpeed + 0.45f;

            float targetFrequency = sprinting ? sprintBobFrequency : walkBobFrequency;
            float targetAmplitude = sprinting ? sprintBobAmplitude : walkBobAmplitude;

            if (moving)
            {
                bobPhase += Time.deltaTime * targetFrequency;
            }
            else
            {
                bobPhase = Mathf.Lerp(bobPhase, 0f, 4f * Time.deltaTime);
            }

            float moveBlend = moving ? Mathf.Clamp01(speed / walkSpeed) : 0f;
            float bobY = Mathf.Sin(bobPhase * 2f) * targetAmplitude * moveBlend;
            float bobX = Mathf.Sin(bobPhase) * targetAmplitude * 0.45f * moveBlend;

            float trainTime = Time.time;
            float swayX = Mathf.Sin(trainTime * 1.35f) * trainSwayAmplitude;
            float swayY = Mathf.Sin(trainTime * 2.6f + 0.8f) * trainSwayAmplitude * 0.45f;
            float microY = Mathf.PerlinNoise(trainTime * 3.5f, 0.17f) - 0.5f;
            microY *= trainSwayAmplitude * 0.55f;

            Vector3 targetPosition = cameraBaseLocalPosition + new Vector3(bobX + swayX, bobY + swayY + microY, 0f);
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                targetPosition,
                1f - Mathf.Exp(-cameraReturnSharpness * Time.deltaTime));

            float roll = Mathf.Sin(trainTime * 1.15f + 0.4f) * trainRollDegrees;
            float bobPitch = moving ? Mathf.Sin(bobPhase * 2f) * 0.20f * moveBlend : 0f;
            Quaternion targetRotation = Quaternion.Euler(pitch + bobPitch, 0f, roll);
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                targetRotation,
                1f - Mathf.Exp(-18f * Time.deltaTime));

            float targetFov = baseFov + (sprinting ? sprintFovBoost * speed01 : 0f);
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFov,
                1f - Mathf.Exp(-7f * Time.deltaTime));
        }
    }
}
