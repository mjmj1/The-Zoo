#if UNITY_EDITOR
using System;
using UI;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

namespace Characters
{
    /// <summary>
    ///     The custom editor for the <see cref="CharacterHandler" /> component.
    /// </summary>
    [CustomEditor(typeof(CharacterHandler), true)]
    public class CharacterControllerEditor : NetworkTransformEditor
    {
        private SerializedProperty _mouseSensitivity;
        private SerializedProperty _walkSpeed;
        private SerializedProperty _sprintSpeed;
        private SerializedProperty _rotationSpeed;

        public override void OnEnable()
        {
            _walkSpeed = serializedObject.FindProperty(nameof(CharacterHandler.walkSpeed));
            _sprintSpeed = serializedObject.FindProperty(nameof(CharacterHandler.sprintSpeed));
            _rotationSpeed = serializedObject.FindProperty(nameof(CharacterHandler.rotationSpeed));
            _mouseSensitivity = serializedObject.FindProperty(nameof(CharacterHandler.mouseSensitivity));
            base.OnEnable();
        }

        private void DisplayCharacterControllerProperties()
        {
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
        // These bool properties ensure that any expanded or collapsed property views
        // within the inspector view will be saved and restored the next time the
        // asset/prefab is viewed.
        public bool controllerPropertiesVisible;
#endif
        public float walkSpeed = 4f;
        public float sprintSpeed = 7f;
        public float rotationSpeed = 50f;
        public float mouseSensitivity = 0.1f;
        public float minPitch = -10f;
        public float maxPitch = 20f;

        private PlanetGravity _gravity;
        private InputHandler _input;
        private Transform _planet;
        private Quaternion _previousRotation;

        private Rigidbody _rb;

        private PlayerEntity _entity;

        private float _moveSpeed;
        
        public static readonly int MoveId = Animator.StringToHash("Move");
        public static readonly int SprintId = Animator.StringToHash("Sprint");
        public static readonly int SpinId = Animator.StringToHash("Spin");
        public static readonly int ClickedId = Animator.StringToHash("Clicked");
        
        public float Pitch { get; private set; }

        private void Start()
        {
            if (!IsOwner) return;

            _moveSpeed = walkSpeed;
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

        private void Update()
        {
            if (!IsOwner) return;
            
            AlignToSurface();
            
            HandleLook();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            HandleMovement();
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
            _entity.SetBool(MoveId, ctx.performed);
        }

        private void SprintAction(bool value)
        {
            _entity.SetBool(SprintId, value);

            _moveSpeed = value ? sprintSpeed : walkSpeed;
        }

        private void SpinAction(bool value)
        {
            _entity.SetBool(SpinId, value);
        }
        
        private void ClickedAction(bool value)
        {
            _entity.SetBool(ClickedId, value);
        }
    }
}