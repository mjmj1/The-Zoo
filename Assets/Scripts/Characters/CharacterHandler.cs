#if UNITY_EDITOR
using System;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;
using Random = UnityEngine.Random;

namespace Characters
{
    /// <summary>
    ///     The custom editor for the <see cref="CharacterHandler" /> component.
    /// </summary>
    [CustomEditor(typeof(CharacterHandler), true)]
    public class CharacterControllerEditor : NetworkTransformEditor
    {
        private SerializedProperty groundMask;
        private SerializedProperty jumpForce;
        private SerializedProperty mouseSensitivity;
        private SerializedProperty rotationSpeed;
        private SerializedProperty sprintSpeed;
        private SerializedProperty walkSpeed;

        public override void OnEnable()
        {
            groundMask = serializedObject.FindProperty(nameof(CharacterHandler.groundMask));
            jumpForce = serializedObject.FindProperty(nameof(CharacterHandler.jumpForce));
            walkSpeed = serializedObject.FindProperty(nameof(CharacterHandler.walkSpeed));
            sprintSpeed = serializedObject.FindProperty(nameof(CharacterHandler.runSpeed));
            rotationSpeed = serializedObject.FindProperty(nameof(CharacterHandler.rotationSpeed));
            mouseSensitivity =
                serializedObject.FindProperty(nameof(CharacterHandler.mouseSensitivity));
            base.OnEnable();
        }

        private void DisplayCharacterControllerProperties()
        {
            EditorGUILayout.PropertyField(groundMask);
            EditorGUILayout.PropertyField(jumpForce);
            EditorGUILayout.PropertyField(walkSpeed);
            EditorGUILayout.PropertyField(sprintSpeed);
            EditorGUILayout.PropertyField(rotationSpeed);
            EditorGUILayout.PropertyField(mouseSensitivity);
        }

        public override void OnInspectorGUI()
        {
            var characterController = target as CharacterHandler;

            void SetExpanded(bool expanded)
            {
                characterController.controllerPropertiesVisible = expanded;
            }

            if (characterController)
                DrawFoldOutGroup<CharacterHandler>(characterController.GetType(),
                    DisplayCharacterControllerProperties,
                    characterController.controllerPropertiesVisible, SetExpanded);
            base.OnInspectorGUI();
        }
    }
#endif

    public class CharacterHandler : NetworkTransform
    {
#if UNITY_EDITOR
        public bool controllerPropertiesVisible;
#endif
        public LayerMask groundMask;
        public float jumpForce = 3f;
        public float walkSpeed = 4f;
        public float runSpeed = 7f;
        public float rotationSpeed = 50f;
        public float mouseSensitivity = 0.1f;
        public float minPitch = -10f;
        public float maxPitch = 20f;

        private CharacterNetworkAnimator animator;

        private PlanetGravity gravity;
        private InputHandler input;

        private float moveSpeed;
        private Quaternion previousRotation;

        private Rigidbody rb;

        private bool isGround;
        private bool IsGround
        {
            get => isGround;
            set
            {
                isGround = value;
                IsGroundChanged?.Invoke(value);
            }
        }

        public float Pitch { get; private set; }

        private void Start()
        {
            if (!IsOwner) return;

            moveSpeed = walkSpeed;
        }

        private void Update()
        {
            if (!IsOwner) return;

            AlignToSurface();

            HandleLook();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            IsGround = IsGrounded();

            HandleMovement();
        }

        public override void OnNetworkSpawn()
        {
            InitializeComponent();
            InitializeFollowCamera();
            Subscribe();

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Unsubscribe();
            gravity?.Unsubscribe(rb);

            base.OnNetworkDespawn();
        }

        private bool IsGrounded()
        {
            return Physics.SphereCast(transform.position + transform.up * 0.3f, 0.25f,
                -transform.up, out _, 0.2f, groundMask);
        }

        private void OnOnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (OwnerClientId != clientId) return;

            InitializeFollowCamera();
            InitializeGravity();

            if (sceneName != "Lobby") return;

            var pos = Util.GetCirclePositions(Vector3.zero, Random.Range(0, 8), 5f, 8);

            transform.SetPositionAndRotation(pos, Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }

        private void Subscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            animator.Initialize(input);

            input.InputActions.Player.Jump.performed += Jump;
            input.InputActions.Player.Run.performed += Run;
            input.InputActions.Player.Run.canceled += Run;

            IsGroundChanged += OnIsGroundChanged;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            animator.OnDestroying(input);

            input.InputActions.Player.Jump.performed -= Jump;
            input.InputActions.Player.Run.performed -= Run;
            input.InputActions.Player.Run.canceled -= Run;

            IsGroundChanged -= OnIsGroundChanged;
        }

        private void InitializeComponent()
        {
            if (!IsOwner) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            input = GetComponent<InputHandler>();
            animator = GetComponent<CharacterNetworkAnimator>();
        }

        private void InitializeFollowCamera()
        {
            if (!IsOwner) return;

            var cam = FindAnyObjectByType<ThirdPersonCamera>();
            cam?.ConnectToTarget(transform);
        }

        private void InitializeGravity()
        {
            if (!IsOwner) return;

            gravity = FindAnyObjectByType<PlanetGravity>();

            rb.useGravity = !gravity;

            gravity?.Subscribe(rb);
        }

        private void HandleLook()
        {
            var lookInput = input.LookInput;

            Pitch = Mathf.Clamp(Pitch - lookInput.y * mouseSensitivity, minPitch, maxPitch);

            transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));
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
            if (!gravity) return;

            var gravityDirection = (transform.position - gravity.transform.position).normalized;

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void OnIsGroundChanged(bool value)
        {
            if(value) animator.SetTrigger(CharacterNetworkAnimator.LandHash);
        }

        private void Jump(InputAction.CallbackContext obj)
        {
            if (!IsGround) return;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void Run(InputAction.CallbackContext obj)
        {
            if (!IsGround) return;

            if (obj.performed) moveSpeed = runSpeed;
            if (obj.canceled) moveSpeed = walkSpeed;
        }

        public event Action<bool> IsGroundChanged;
    }
}