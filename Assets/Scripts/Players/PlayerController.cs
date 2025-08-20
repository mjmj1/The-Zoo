using EventHandler;
using System.Collections;
using System.Linq;
using GamePlay;
using Maps;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;
#if UNITY_EDITOR
using Unity.Netcode.Editor;
#endif

namespace Players
{
#if UNITY_EDITOR
    /// <summary>
    ///     The custom editor for the <see cref="PlayerController" /> component.
    /// </summary>
    [CustomEditor(typeof(PlayerController), true)]
    public class CharacterControllerEditor : NetworkTransformEditor
    {
        private SerializedProperty groundMask;
        private SerializedProperty mouseSensitivity;
        private SerializedProperty rotationSpeed;
        private SerializedProperty sprintSpeed;
        private SerializedProperty walkSpeed;

        public override void OnEnable()
        {
            groundMask = serializedObject.FindProperty(nameof(PlayerController.groundMask));
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
        public float walkSpeed = 3f;
        public float runSpeed = 4.5f;
        public float rotationSpeed = 50f;
        public float mouseSensitivity = 0.1f;

        internal InputHandler input;
        private PlayerNetworkAnimator animator;
        private PlayerEntity entity;
        private Hittable hittable;
        private bool isAround;

        private float moveSpeed;
        private Quaternion previousRotation;

        private Rigidbody rb;

        private PlayerReadyChecker readyChecker;
        private float slowdownRate = 1f;

        public void Reset()
        {
            CanMove = true;

            walkSpeed = 3f;
            runSpeed = 4.5f;
        }

        private void Start()
        {
            if (!IsOwner) return;

            var saved = PlayerPrefs.GetFloat("opt_mouse_sens", mouseSensitivity);

            ApplyMouseSensitivity(saved);

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
            
            GamePlayEventHandler.OnPlayerSpined(entity.isSpinHold);

            if (!TorusWorld.Instance) return;
            
            var wrapped = TorusWorld.Instance.WrapXZ(rb.position);

            if (!((wrapped - rb.position).sqrMagnitude > 0.0001f)) return;

            rb.position = wrapped;
            Teleport(wrapped, transform.rotation, transform.localScale);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }

        public bool CanMove { get; set; } = true;
        public bool IsJumping { get; set; }

        public void ApplyMouseSensitivity(float value)
        {
            mouseSensitivity = Mathf.Clamp(value, 0.02f, 5f);
        }

        public override void OnNetworkSpawn()
        {
            InitializeComponent();
            InitializeFollowCamera();
            Subscribe();

            InitializePlanet();
            InitializeTorusWorld();

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Unsubscribe();
            PlanetGravity.Instance?.Unsubscribe(rb);

            base.OnNetworkDespawn();
        }

        private void OnOnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (!IsOwner) return;

            InitializePlanet();
            InitializeTorusWorld();

            input.MouseLeftClicked();

            entity.AlignForward();

            if (!sceneName.Equals("Lobby")) return;

            GamePlayEventHandler.OnUIChanged("Lobby");

            Reset();
            entity.Reset();
            hittable.Reset();
            readyChecker.Reset();

            var clients = NetworkManager.ConnectedClientsIds.ToList();

            var pos = Util.GetCirclePositions(Vector3.zero, clients.IndexOf(clientId), 2f, 4);

            transform.SetPositionAndRotation(pos, Quaternion.LookRotation((Vector3.zero - pos).normalized));
        }

        private void Subscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;

            input.InputActions.Player.Look.performed += Look;
            input.InputActions.Player.Look.canceled += Look;
            input.InputActions.Player.RightClick.performed += Rmb;
            input.InputActions.Player.RightClick.canceled += Rmb;
            input.InputActions.Player.Move.performed += Movement;
            input.InputActions.Player.Move.canceled += Movement;
            input.InputActions.Player.Run.performed += Run;
            input.InputActions.Player.Run.canceled += Run;
            input.InputActions.Player.Spin.performed += Spin;
            input.InputActions.Player.Spin.canceled += Spin;
            input.InputActions.Player.Jump.performed += Jump;
            input.InputActions.Player.Attack.started += Attack;

            hittable.health.OnValueChanged += Hit;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            input.InputActions.Player.Look.performed -= Look;
            input.InputActions.Player.Look.canceled -= Look;
            input.InputActions.Player.RightClick.performed -= Rmb;
            input.InputActions.Player.RightClick.canceled -= Rmb;
            input.InputActions.Player.Move.performed -= Movement;
            input.InputActions.Player.Move.canceled -= Movement;
            input.InputActions.Player.Run.performed -= Run;
            input.InputActions.Player.Run.canceled -= Run;
            input.InputActions.Player.Spin.performed -= Spin;
            input.InputActions.Player.Spin.canceled -= Spin;
            input.InputActions.Player.Jump.performed -= Jump;
            input.InputActions.Player.Attack.performed -= Attack;

            hittable.health.OnValueChanged -= Hit;
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
            input = GetComponent<InputHandler>();
            entity = GetComponent<PlayerEntity>();
            hittable = GetComponent<Hittable>();
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

        private void InitializePlanet()
        {
            if (!IsOwner) return;
            if (!PlanetGravity.Instance) return;

            rb.useGravity = false;
            PlanetGravity.Instance.Subscribe(rb);
            PivotBinder.Instance.gameObject.SetActive(false);
        }

        private void InitializeTorusWorld()
        {
            if (!IsOwner) return;
            if (!TorusWorld.Instance) return;

            PivotBinder.Instance.BindPivot(transform);
            TorusWorld.Instance.tile.follow = transform;
        }

        private void HandleMovement()
        {
            if (!CanMove || entity.isSpinHold) return;

            var moveInput = input.MoveInput;

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

                transform.Rotate(Vector3.up * (input.LookInput.x * mouseSensitivity));

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
            if (IsJumping) return;
            if (!CanMove) return;

            // rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            if (entity.isSpinHold) return;

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
            if (!IsOwner) return;
            if (!CanMove) return;
            if (IsJumping) return;
            if (entity.isSpinHold) return;

            entity.AlignForward();

            GamePlayEventHandler.OnPlayerAttack();

            animator.OnAttack(ctx);
        }

        private void Spin(InputAction.CallbackContext ctx)
        {
            if (!CanMove) return;

            entity.isSpinHold = ctx.performed;

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
            slowdownRate = 0.2f;

            yield return new WaitForSeconds(1f);

            slowdownRate = 1f;
        }
    }
}