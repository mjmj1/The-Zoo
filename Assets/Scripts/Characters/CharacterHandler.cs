#if UNITY_EDITOR
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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
        private SerializedProperty isGround;
        private SerializedProperty isAttack;
        private SerializedProperty isSpin;

        public override void OnEnable()
        {
            isGround = serializedObject.FindProperty(nameof(CharacterHandler.isGround));
            isAttack = serializedObject.FindProperty(nameof(CharacterHandler.isAttack));
            isSpin = serializedObject.FindProperty(nameof(CharacterHandler.isSpin));

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
            EditorGUILayout.PropertyField(isGround);
            EditorGUILayout.PropertyField(isAttack);
            EditorGUILayout.PropertyField(isSpin);
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

        public bool isGround = true;
        public bool isAttack;
        public bool isSpin;

        private CharacterNetworkAnimator animator;

        private PlanetGravity gravity;
        private InputHandler input;

        private float moveSpeed;
        private Quaternion previousRotation;

        private Rigidbody rb;

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
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.1f, groundMask);;
        }

        private void OnOnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (OwnerClientId != clientId) return;

            InitializeFollowCamera();
            InitializeGravity();

            if (sceneName != "Lobby") return;

            var pos = Util.GetCirclePositions(Vector3.zero, Random.Range(0, 8), 5f, 8);

            transform.SetPositionAndRotation(pos,
                Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }

        private void Subscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            input.InputActions.Player.Move.performed += Movement;
            input.InputActions.Player.Move.canceled += Movement;
            input.InputActions.Player.Run.performed += Run;
            input.InputActions.Player.Run.canceled += Run;
            input.InputActions.Player.Spin.performed += Spin;
            input.InputActions.Player.Spin.canceled += Spin;
            input.InputActions.Player.Jump.performed += Jump;
            input.InputActions.Player.Attack.performed += Attack;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            input.InputActions.Player.Move.performed -= Movement;
            input.InputActions.Player.Move.canceled -= Movement;
            input.InputActions.Player.Run.performed -= Run;
            input.InputActions.Player.Run.canceled -= Run;
            input.InputActions.Player.Spin.performed -= Spin;
            input.InputActions.Player.Spin.canceled -= Spin;
            input.InputActions.Player.Jump.performed -= Jump;
            input.InputActions.Player.Attack.performed -= Attack;
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
            if (isAttack || isSpin) return;

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

        private void Movement(InputAction.CallbackContext ctx)
        {
            animator.OnMove(ctx);
        }

        private void Jump(InputAction.CallbackContext ctx)
        {
            if (!isGround) return;
            if (isAttack) return;
            if (isSpin) return;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            animator.OnJump(ctx);
        }

        private void Run(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) moveSpeed = runSpeed;
            if (ctx.canceled) moveSpeed = walkSpeed;

            animator.OnRun(ctx);
        }

        private void Attack(InputAction.CallbackContext ctx)
        {
            if (!isGround) return;
            if (isAttack) return;
            if (isSpin) return;

            animator.OnAttack(ctx);
        }

        private void Spin(InputAction.CallbackContext ctx)
        {
            if (!isGround) return;
            if (isAttack) return;

            isSpin = ctx.performed;
            animator.OnSpin(ctx);
        }
    }
}