#if UNITY_EDITOR
using System;
using UI;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
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
        private SerializedProperty _moveSpeed;
        private SerializedProperty _rotationSpeed;

        public override void OnEnable()
        {
            _moveSpeed = serializedObject.FindProperty(nameof(CharacterHandler.moveSpeed));
            _rotationSpeed = serializedObject.FindProperty(nameof(CharacterHandler.rotationSpeed));
            _mouseSensitivity = serializedObject.FindProperty(nameof(CharacterHandler.mouseSensitivity));
            base.OnEnable();
        }

        private void DisplayCharacterControllerProperties()
        {
            EditorGUILayout.PropertyField(_moveSpeed);
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
        public float moveSpeed = 5f;
        public float rotationSpeed = 50f;
        public float mouseSensitivity = 0.25f;
        public float minPitch = -10f;
        public float maxPitch = 20f;

        private PlanetGravity _gravity;
        private InputHandler _input;
        private Transform _planet;
        private Quaternion _previousRotation;

        private Rigidbody _rb;

        private PlayerEntity _entity;

        private static readonly int MoveId = Animator.StringToHash("Move");
        
        public float Pitch { get; private set; }

        private void Start()
        {
            if (!IsOwner) return;

            MyLogger.Print(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (!IsOwner) return;
            
            Init();
            InitCamera();
            Subscribe();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (UIManager.IsCursorLocked()) return;

            if (!_gravity) return;
            AlignToSurface();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            if (UIManager.IsCursorLocked()) return;

            HandleLook();
            HandleMovement();
        }

        public override void OnDestroy()
        {
            _gravity?.Unsubscribe(_rb);
            base.OnDestroy();
        }

        private void OnOnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (OwnerClientId != clientId) return;

            MyLogger.Print(this);

            InitCamera();
            InitSceneLoaded();
        }

        private void Subscribe()
        {
            MyLogger.Print(this);

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            _input.InputActions.Player.Move.performed += ctx => _entity.SetBool(MoveId, true);
            _input.InputActions.Player.Move.canceled += ctx => _entity.SetBool(MoveId, false);;
            
            /*_input.InputActions.Player.Sprint.performed += ctx => _entity.SetTrigger("Sprint");
            _input.InputActions.Player.Sprint.canceled += ctx => _entity.ResetTrigger("Sprint");*/
        }

        private void Init()
        {
            if (!IsOwner) return;

            MyLogger.Print(this);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<InputHandler>();
            _entity = GetComponent<PlayerEntity>();
        }

        private void InitCamera()
        {
            var cam = FindAnyObjectByType<ThirdPersonCamera>();
            cam?.ConnectToTarget(transform);
        }

        private void InitSceneLoaded()
        {
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
            var moveInput = _input.MoveInput;
            
            if (moveInput == Vector2.zero) return;
            
            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();
            _rb.MovePosition(_rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime));
        }

        private void AlignToSurface()
        {
            var gravityDirection = (transform.position - _planet.position).normalized;

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}