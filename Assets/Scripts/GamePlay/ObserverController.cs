using System;
using Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay
{
    public class ObserverController : MonoBehaviour
    {
        public float Pitch { get; set; }

        public float mouseSensitivity = 0.1f;
        public float moveSpeed = 8f;
        public float rotationSpeed = 50f;

        private Rigidbody rb;
        private InputHandler input;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<InputHandler>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CameraManager.Instance.Find();

            CameraManager.Instance.SetFollowTarget(transform);

            PlanetGravity.Instance.Subscribe(rb);

            input.InputActions.Player.Look.performed += Look;
            input.InputActions.Player.Look.canceled += Look;
        }

        private void OnDestroy()
        {
            input.InputActions.Player.Look.performed -= Look;
            input.InputActions.Player.Look.canceled -= Look;
        }

        private void Update()
        {
            AlignToSurface();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void Look(InputAction.CallbackContext ctx)
        {
            CameraManager.Instance.LookMove();

            transform.Rotate(Vector3.up * (input.LookInput.x * mouseSensitivity));

            CameraManager.Instance.SetEulerAngles(transform.rotation.eulerAngles.y);
        }

        private void HandleMovement()
        {
            var moveInput = input.MoveInput;

            if (moveInput == Vector2.zero) return;

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime));
        }

        private void AlignToSurface()
        {
            if (!PlanetGravity.Instance) return;

            var targetRotation = Quaternion.FromToRotation(transform.up,
                -PlanetGravity.Instance.GetGravityDirection(transform.position)
                ) * transform.rotation;

            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}