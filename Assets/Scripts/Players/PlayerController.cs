#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using EventHandler;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

namespace Players
{
    /// <summary>
    ///     The custom editor for the <see cref="PlayerController" /> component.
    /// </summary>
    [CustomEditor(typeof(PlayerController), true)]
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
            groundMask = serializedObject.FindProperty(nameof(PlayerController.groundMask));
            jumpForce = serializedObject.FindProperty(nameof(PlayerController.jumpForce));
            walkSpeed = serializedObject.FindProperty(nameof(PlayerController.walkSpeed));
            sprintSpeed = serializedObject.FindProperty(nameof(PlayerController.runSpeed));
            rotationSpeed = serializedObject.FindProperty(nameof(PlayerController.rotationSpeed));
            mouseSensitivity =
                serializedObject.FindProperty(nameof(PlayerController.mouseSensitivity));

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
            var characterController = target as PlayerController;

            void SetExpanded(bool expanded)
            {
                characterController.controllerPropertiesVisible = expanded;
            }

            if (characterController)
                DrawFoldOutGroup<PlayerController>(characterController.GetType(),
                    DisplayCharacterControllerProperties,
                    characterController.controllerPropertiesVisible, SetExpanded);
            base.OnInspectorGUI();
        }
    }
#endif

    public class PlayerController : NetworkTransform, IMoveState
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

        private PlayerNetworkAnimator animator;

        internal InputHandler Input;
        private PlayerEntity entity;
        private bool isAround;
        private bool isSpin;

        private float moveSpeed;

        private Rigidbody rb;

        private PlayerReadyChecker readyChecker;
        private float slowdownRate = 1f;

        public bool CanMove { get; set; } = true;
        public bool IsJumping { get; set; }
        public bool IsSpinning { get; set; }

        public void Reset()
        {
            CanMove = true;
        }

        private void Start()
        {
            if (!IsOwner) return;

            moveSpeed = walkSpeed;
        }

        private void Update()
        {
            if (!IsOwner) return;

            AlignToSurface();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            HandleMovement();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }

        public override void OnNetworkSpawn()
        {
            InitializeComponent();
            InitializeFollowCamera();
            Subscribe();

            InitializeGravity();

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Unsubscribe();
            PlanetGravity.Instance?.Unsubscribe(rb);

            base.OnNetworkDespawn();
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(transform.position, 0.05f, groundMask);
        }

        private void OnOnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (!IsOwner) return;

            InitializeGravity();

            if (!sceneName.Equals("Lobby")) return;

            GamePlayEventHandler.OnUIChanged("Lobby");

            Reset();
            entity.Reset();
            readyChecker.Reset();

            var clients = NetworkManager.ConnectedClientsIds.ToList();

            var pos = Util.GetCirclePositions(Vector3.zero, clients.IndexOf(clientId), 5f, 8);

            transform.SetPositionAndRotation(pos,
                Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }

        private void Subscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            Input.InputActions.Player.Look.performed += Look;
            Input.InputActions.Player.Look.canceled += Look;
            Input.InputActions.Player.RightClick.performed += Rmb;
            Input.InputActions.Player.RightClick.canceled += Rmb;
            Input.InputActions.Player.Move.performed += Movement;
            Input.InputActions.Player.Move.canceled += Movement;
            Input.InputActions.Player.Run.performed += Run;
            Input.InputActions.Player.Run.canceled += Run;
            Input.InputActions.Player.Spin.performed += Spin;
            Input.InputActions.Player.Spin.canceled += Spin;
            Input.InputActions.Player.Jump.performed += Jump;
            Input.InputActions.Player.Attack.started += Attack;

            entity.health.OnValueChanged += Hit;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            Input.InputActions.Player.Look.performed -= Look;
            Input.InputActions.Player.Look.canceled -= Look;
            Input.InputActions.Player.RightClick.performed -= Rmb;
            Input.InputActions.Player.RightClick.canceled -= Rmb;
            Input.InputActions.Player.Move.performed -= Movement;
            Input.InputActions.Player.Move.canceled -= Movement;
            Input.InputActions.Player.Run.performed -= Run;
            Input.InputActions.Player.Run.canceled -= Run;
            Input.InputActions.Player.Spin.performed -= Spin;
            Input.InputActions.Player.Spin.canceled -= Spin;
            Input.InputActions.Player.Jump.performed -= Jump;
            Input.InputActions.Player.Attack.performed -= Attack;

            entity.health.OnValueChanged -= Hit;
        }

        private void Rmb(InputAction.CallbackContext ctx)
        {
            isAround = ctx.performed;
        }

        private void InitializeComponent()
        {
            if (!IsOwner) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            Input = GetComponent<InputHandler>();
            entity = GetComponent<PlayerEntity>();
            animator = GetComponent<PlayerNetworkAnimator>();
            readyChecker = GetComponent<PlayerReadyChecker>();
        }

        private void InitializeFollowCamera()
        {
            if (!IsOwner) return;

            CameraManager.Instance.SetFollowTarget(transform);
            CameraManager.Instance.LookMove();
            CameraManager.Instance.SetEulerAngles(transform.rotation.eulerAngles.y);
        }

        private void InitializeGravity()
        {
            if (!IsOwner) return;

            rb.useGravity = !PlanetGravity.Instance;

            PlanetGravity.Instance?.Subscribe(rb);
        }

        private void HandleMovement()
        {
            if (!CanMove || isSpin) return;

            var moveInput = Input.MoveInput;

            if (moveInput == Vector2.zero) return;

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position +
                            moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));

        }

        private void AlignToSurface()
        {
            if (!PlanetGravity.Instance) return;

            var gravityDirection = -PlanetGravity.Instance.GetGravityDirection(transform.position);

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void Look(InputAction.CallbackContext ctx)
        {
            if (!CanMove) return;

            if (isAround)
            {
                CameraManager.Instance.LookAround();
            }
            else
            {
                CameraManager.Instance.LookMove();

                transform.Rotate(Vector3.up * (Input.LookInput.x * mouseSensitivity));

                CameraManager.Instance.SetEulerAngles(transform.rotation.eulerAngles.y);
            }
        }

        private void Movement(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled)
            {
                CameraManager.Instance.Orbit.HorizontalAxis.Value = 0;
                CameraManager.Instance.LookAround();
            }

            animator.OnMove(ctx);
        }

        private void Jump(InputAction.CallbackContext ctx)
        {
            if (!IsGrounded()) return;
            if (!CanMove) return;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            if (isSpin) return;

            animator.OnJump(ctx);
        }

        private void Run(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                moveSpeed = runSpeed;
            }
            if (ctx.canceled)
            {
                moveSpeed = walkSpeed;
            }
            animator.OnRun(ctx);
        }

        private void Attack(InputAction.CallbackContext ctx)
        {
            if (!CanMove) return;
            if (!IsGrounded()) return;
            if (isSpin) return;

            entity.AlignForward();

            GamePlayEventHandler.OnPlayerAttack();

            animator.OnAttack(ctx);

        }

        private void Spin(InputAction.CallbackContext ctx)
        {
            if (!CanMove) return;

            isSpin = ctx.performed;
            animator.OnSpin(ctx);
        }

        private void Hit(int previousValue, int newValue)
        {
            StartCoroutine(Slowdown());

            if (newValue > 0) animator.OnHit();
            else StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            animator.OnDeath();

            yield return new WaitForSeconds(3f);

            entity.isDead.Value = true;

            CanMove = true;

            animator.OnRebind();
        }

        private IEnumerator Slowdown()
        {
            slowdownRate = 0.7f;

            yield return new WaitForSeconds(0.3f);

            slowdownRate = 1f;
        }
    }
}