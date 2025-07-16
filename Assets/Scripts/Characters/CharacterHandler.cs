#if UNITY_EDITOR
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Characters.InputHandler;

namespace Characters
{
    /// <summary>
    ///     The custom editor for the <see cref="CharacterHandler" /> component.
    /// </summary>
    [CustomEditor(typeof(CharacterHandler), true)]
    public class CharacterControllerEditor : NetworkTransformEditor
    {
        private SerializedProperty _groundMask;
        private SerializedProperty _jumpForce;
        private SerializedProperty _mouseSensitivity;
        private SerializedProperty _rotationSpeed;
        private SerializedProperty _sprintSpeed;
        private SerializedProperty _walkSpeed;

        public override void OnEnable()
        {
            _groundMask = serializedObject.FindProperty(nameof(CharacterHandler.groundMask));
            _jumpForce = serializedObject.FindProperty(nameof(CharacterHandler.jumpForce));
            _walkSpeed = serializedObject.FindProperty(nameof(CharacterHandler.walkSpeed));
            _sprintSpeed = serializedObject.FindProperty(nameof(CharacterHandler.sprintSpeed));
            _rotationSpeed = serializedObject.FindProperty(nameof(CharacterHandler.rotationSpeed));
            _mouseSensitivity =
                serializedObject.FindProperty(nameof(CharacterHandler.mouseSensitivity));
            base.OnEnable();
        }

        private void DisplayCharacterControllerProperties()
        {
            EditorGUILayout.PropertyField(_groundMask);
            EditorGUILayout.PropertyField(_jumpForce);
            EditorGUILayout.PropertyField(_walkSpeed);
            EditorGUILayout.PropertyField(_sprintSpeed);
            EditorGUILayout.PropertyField(_rotationSpeed);
            EditorGUILayout.PropertyField(_mouseSensitivity);
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
        public float sprintSpeed = 7f;
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

        private bool isGrounded;

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

            isGrounded = IsGrounded();

            animator.SetBool(IsGroundHash, isGrounded);

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
        }

        private void Subscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            input.InputActions.Player.Move.performed += MovementAction;
            input.InputActions.Player.Move.canceled += MovementAction;

            input.InputActions.Player.Jump.performed += JumpAction;

            input.OnAttackPressed += ClickedAction;
            input.OnSprintPressed += SprintAction;
            input.OnSpinPressed += SpinAction;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            input.InputActions.Player.Move.performed -= MovementAction;
            input.InputActions.Player.Move.canceled -= MovementAction;

            input.OnAttackPressed -= ClickedAction;
            input.OnSprintPressed -= SprintAction;
            input.OnSpinPressed -= SpinAction;
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
            if (input.SpinPressed) return;
            if (input.AttackPressed) return;

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

        private void MovementAction(InputAction.CallbackContext ctx)
        {
            animator.SetBool(MoveHash, ctx.performed);
        }

        private void JumpAction(InputAction.CallbackContext obj)
        {
            if (!isGrounded) return;

            animator.SetTrigger(JumpHash);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void SprintAction(bool value)
        {
            if (!isGrounded) return;
            
            animator.SetBool(SprintHash, value);

            moveSpeed = value ? sprintSpeed : walkSpeed;
        }

        private void SpinAction(bool value)
        {
            if (!isGrounded) return;
            
            animator.SetBool(SpinHash, value);
        }

        private void ClickedAction(bool value)
        {
            if (!isGrounded) return;
            
            animator.SetBool(AttackHash, value);
        }
    }
}