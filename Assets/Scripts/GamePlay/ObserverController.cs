using Players;
using UnityEngine;

namespace GamePlay
{
    public class ObserverController : MonoBehaviour, ICameraTarget
    {
        public float Pitch { get; set; }

        public float mouseSensitivity = 0.1f;
        public float minPitch = -10f;
        public float maxPitch = 20f;
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
        }

        private void Update()
        {
            AlignToSurface();
        }

        private void FixedUpdate()
        {
            HandleMovement();
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