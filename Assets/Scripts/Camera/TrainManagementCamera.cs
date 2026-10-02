using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.CameraSystem
{
    /// <summary>
    /// Free 3D management camera used while the player manages the train.
    /// Controls: WASD move, middle mouse orbit, mouse wheel zoom, Left Shift speed boost.
    /// </summary>
    public sealed class TrainManagementCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float boostMultiplier = 2f;
        [SerializeField] private Vector2 xBounds = new(-120f, 120f);
        [SerializeField] private Vector2 zBounds = new(-45f, 45f);

        [Header("Orbit")]
        [SerializeField] private float orbitSensitivity = 0.18f;
        [SerializeField] private Vector2 pitchLimits = new(18f, 72f);

        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 0.035f;
        [SerializeField] private Vector2 distanceLimits = new(7f, 65f);

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.08f;
        [SerializeField] private float zoomSmoothTime = 0.08f;
        [SerializeField] private float rotationSharpness = 14f;

        private Vector3 desiredFocusPoint = new(0f, 1.5f, 0f);
        private Vector3 currentFocusPoint = new(0f, 1.5f, 0f);
        private Vector3 focusVelocity;

        private float desiredDistance = 36f;
        private float currentDistance = 36f;
        private float distanceVelocity;

        private float desiredYaw = -28f;
        private float currentYaw = -28f;
        private float desiredPitch = 38f;
        private float currentPitch = 38f;

        private void OnEnable()
        {
            SnapToDesiredPose();
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return;
            }

            ReadPanInput();
            ReadOrbitInput();
            ReadZoomInput();
            SmoothAndApplyPose();
        }

        public void Configure(Vector3 focusPoint, float distance, float yaw, float pitch)
        {
            desiredFocusPoint = focusPoint;
            currentFocusPoint = focusPoint;
            desiredDistance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);
            currentDistance = desiredDistance;
            desiredYaw = yaw;
            currentYaw = yaw;
            desiredPitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
            currentPitch = desiredPitch;
            SnapToDesiredPose();
        }

        public void FocusOn(Transform target, float distance = 18f)
        {
            if (target == null)
            {
                return;
            }

            desiredFocusPoint = target.position;
            desiredDistance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);
        }

        private void ReadPanInput()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;

            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            input.Normalize();

            Quaternion yawRotation = Quaternion.Euler(0f, desiredYaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;

            float distanceScale = Mathf.Lerp(0.65f, 1.75f,
                Mathf.InverseLerp(distanceLimits.x, distanceLimits.y, desiredDistance));

            float speed = panSpeed * distanceScale;
            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                speed *= boostMultiplier;
            }

            desiredFocusPoint += (forward * input.y + right * input.x) * (speed * Time.unscaledDeltaTime);
            desiredFocusPoint.x = Mathf.Clamp(desiredFocusPoint.x, xBounds.x, xBounds.y);
            desiredFocusPoint.z = Mathf.Clamp(desiredFocusPoint.z, zBounds.x, zBounds.y);
        }

        private void ReadOrbitInput()
        {
            if (!Mouse.current.middleButton.isPressed)
            {
                return;
            }

            Vector2 delta = Mouse.current.delta.ReadValue();
            desiredYaw += delta.x * orbitSensitivity;
            desiredPitch -= delta.y * orbitSensitivity;
            desiredPitch = Mathf.Clamp(desiredPitch, pitchLimits.x, pitchLimits.y);
        }

        private void ReadZoomInput()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f)
            {
                return;
            }

            float zoomStep = scroll * zoomSensitivity * Mathf.Max(8f, desiredDistance);
            desiredDistance = Mathf.Clamp(desiredDistance - zoomStep, distanceLimits.x, distanceLimits.y);
        }

        private void SmoothAndApplyPose()
        {
            float dt = Time.unscaledDeltaTime;

            currentFocusPoint = Vector3.SmoothDamp(
                currentFocusPoint,
                desiredFocusPoint,
                ref focusVelocity,
                positionSmoothTime,
                Mathf.Infinity,
                dt);

            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                desiredDistance,
                ref distanceVelocity,
                zoomSmoothTime,
                Mathf.Infinity,
                dt);

            float rotationT = 1f - Mathf.Exp(-rotationSharpness * dt);
            currentYaw = Mathf.LerpAngle(currentYaw, desiredYaw, rotationT);
            currentPitch = Mathf.Lerp(currentPitch, desiredPitch, rotationT);

            ApplyPose();
        }

        private void SnapToDesiredPose()
        {
            currentFocusPoint = desiredFocusPoint;
            currentDistance = desiredDistance;
            currentYaw = desiredYaw;
            currentPitch = desiredPitch;
            focusVelocity = Vector3.zero;
            distanceVelocity = 0f;
            ApplyPose();
        }

        private void ApplyPose()
        {
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 position = currentFocusPoint - (rotation * Vector3.forward * currentDistance);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
