using UnityEngine;
using UnityEngine.InputSystem;

namespace ItsaMeKen
{
    public class OrbitObject : MonoBehaviour
    {
        [Header("Orbit Settings")]
        public Transform targetObject;
        public float orbitDistance = 5f;
        public float orbitSpeed = 10f;
       // public float verticalSpeed = 10f;
        public float minYAngle = -30f;    // Limits for vertical angle (looking up)
        public float maxYAngle = 60f;     // Limits for vertical angle (looking down)

        [Header("Camera")]
        public Transform cameraMainTransform;
        public InputActionReference look;

        private float currentOrbitAngle = 0f;  // Horizontal angle (yaw)
        private float currentVerticalAngle = 0f; // Vertical angle (pitch)
        private Vector2 _lookInput;

        void Awake()
        {
            if (look != null)
            {
                look.action.Enable();
            }
        }

        void Update()
        {
            if (targetObject == null || cameraMainTransform == null || look == null) return;

            // Read the player's input for both horizontal and vertical rotation
            _lookInput = look.action.ReadValue<Vector2>();

            // Update horizontal (X-axis) orbit angle
            currentOrbitAngle += _lookInput.x * orbitSpeed * Time.deltaTime;

            // Update vertical (Y-axis) orbit angle and clamp it within defined limits
            currentVerticalAngle -= -_lookInput.y * orbitSpeed * Time.deltaTime;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minYAngle, maxYAngle);

            // Calculate the horizontal rotation (yaw)
            Quaternion horizontalRotation = Quaternion.Euler(0, currentOrbitAngle, 0);

            // Calculate the vertical rotation (pitch) and apply it
            Quaternion verticalRotation = Quaternion.Euler(currentVerticalAngle, 0, 0);

            // Calculate the final position by applying both rotations
            Vector3 orbitDirection = horizontalRotation * verticalRotation * Vector3.forward;

            // Calculate the desired position for the orbit, maintaining distance from the target
            Vector3 desiredPosition = targetObject.position + orbitDirection * orbitDistance;

            // Set the orbiting object's position
            transform.position = desiredPosition;

            // Make the orbiting object look at the target
            transform.LookAt(targetObject);
        }

        void OnDisable()
        {
            if (look != null)
            {
                look.action.Disable();
            }
        }
    }
}
