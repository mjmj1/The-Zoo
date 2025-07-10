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

        private PlayerEntity _entity;

        private PlanetGravity _gravity;
        private InputHandler _input;

        private float _moveSpeed;
        private Transform _planet;
        private Quaternion _previousRotation;

        private Rigidbody _rb;

        private bool _isGrounded;

        public float Pitch { get; private set; }

        private void Start()
        {
            if (!IsOwner) return;

            _moveSpeed = walkSpeed;
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

            _isGrounded = IsGrounded();

            _entity.SetBool(IsGroundHash, _isGrounded);

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
            _gravity?.Unsubscribe(_rb);

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

            _input.InputActions.Player.Move.performed += MovementAction;
            _input.InputActions.Player.Move.canceled += MovementAction;

            _input.InputActions.Player.Jump.performed += JumpAction;

            _input.OnAttackPressed += ClickedAction;
            _input.OnSprintPressed += SprintAction;
            _input.OnSpinPressed += SpinAction;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            _input.InputActions.Player.Move.performed -= MovementAction;
            _input.InputActions.Player.Move.canceled -= MovementAction;

            _input.OnAttackPressed -= ClickedAction;
            _input.OnSprintPressed -= SprintAction;
            _input.OnSpinPressed -= SpinAction;
        }

        private void InitializeComponent()
        {
            if (!IsOwner) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<InputHandler>();
            _entity = GetComponent<PlayerEntity>();
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

            _gravity = FindAnyObjectByType<PlanetGravity>();

            _rb.useGravity = !_gravity;

            _planet = _gravity?.gameObject.transform;
            _gravity?.Subscribe(_rb);
        }

        private void HandleLook()
        {
            var lookInput = _input.LookInput;

            Pitch = Mathf.Clamp(Pitch - lookInput.y * mouseSensitivity, minPitch, maxPitch);

            transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));
        }

        private void HandleMovement()
        {
            if (_input.SpinPressed) return;
            if (_input.AttackPressed) return;

            var moveInput = _input.MoveInput;

            if (moveInput == Vector2.zero) return;

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            _rb.MovePosition(_rb.position + moveDirection * (_moveSpeed * Time.fixedDeltaTime));
        }

        private void AlignToSurface()
        {
            if (!_gravity) return;

            var gravityDirection = (transform.position - _planet.position).normalized;

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void MovementAction(InputAction.CallbackContext ctx)
        {
            _entity.SetBool(MoveHash, ctx.performed);
        }

        private void JumpAction(InputAction.CallbackContext obj)
        {
            if (!_isGrounded) return;

            _entity.SetTrigger(JumpHash);

            _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void SprintAction(bool value)
        {
            _entity.SetBool(SprintHash, value);

            _moveSpeed = value ? sprintSpeed : walkSpeed;
        }

        private void SpinAction(bool value)
        {
            _entity.SetBool(SpinHash, value);
        }

        private void ClickedAction(bool value)
        {
            _entity.SetBool(AttackHash, value);
        }
    }
}