using EventHandler;
using System.Collections;
using System.Linq;
using GamePlay;
using Unit;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;
using World;
#if UNITY_EDITOR
using Unity.Netcode.Editor;
#endif

namespace Players
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerController), true)]
    public class CharacterControllerEditor : NetworkTransformEditor
    {
        private SerializedProperty groundMask;
        private SerializedProperty runSpeed;
        private SerializedProperty walkSpeed;

        public override void OnEnable()
        {
            groundMask = serializedObject.FindProperty(nameof(PlayerController.groundMask));
            walkSpeed = serializedObject.FindProperty(nameof(PlayerController.walkSpeed));
            runSpeed = serializedObject.FindProperty(nameof(PlayerController.runSpeed));
            base.OnEnable();
        }

        private void DisplayCharacterControllerProperties()
        {
            EditorGUILayout.PropertyField(groundMask);
            EditorGUILayout.PropertyField(walkSpeed);
            EditorGUILayout.PropertyField(runSpeed);
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

    public class PlayerController : NetworkTransform, IActionState
    {
#if UNITY_EDITOR
        public bool controllerPropertiesVisible;
#endif
        public LayerMask groundMask;
        public float walkSpeed = 30f;
        public float runSpeed = 45f;

        private Rigidbody rb;

        private HittableBody hittableBody;
        private GravityBody gravityBody;

        internal PlayerInputHandler inputHandler;
        private PlayerReadyChecker readyChecker;
        private PlayerEntity entity;

        private UnitNetworkAnimator animator;

        private bool isAround;
        private float moveSpeed;
        private float mouseSensitivity = 0.25f;

        private float slowdownRate = 1f;

        public void Reset()
        {
            CanMove = true;

            walkSpeed = 30f;
            runSpeed = 45f;
            slowdownRate = 1.0f;
        }

        protected override void Awake()
        {
            if (!IsOwner) return;


            rb = GetComponent<Rigidbody>();
            gravityBody = GetComponent<GravityBody>();
            hittableBody = GetComponent<HittableBody>();

            entity = GetComponent<PlayerEntity>();
            readyChecker = GetComponent<PlayerReadyChecker>();
            animator = GetComponent<UnitNetworkAnimator>();
            inputHandler = GetComponent<PlayerInputHandler>();

            base.Awake();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            gravityBody.Initialize();
            hittableBody.SetHealthPoint(3);

            CameraManager.Instance.SetFollowTarget(transform);
            CameraManager.Instance.LookMove();
            CameraManager.Instance.SetEulerAngles(transform.rotation.eulerAngles.y);

            PivotBinder.Instance?.BindPivot(transform);

            SubscribeInputEvent();

            base.OnNetworkSpawn();
        }

        private void Start()
        {
            if (!IsOwner) return;

            moveSpeed = walkSpeed;

            inputHandler.HideCursor();

            var saved = PlayerPrefs.GetFloat("opt_mouse_sens", mouseSensitivity);
            ApplyMouseSensitivity(saved);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            UnsubscribeInputEvent();

            base.OnNetworkDespawn();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            
            HandleMovement();
            
            GamePlayEventHandler.OnPlayerSpined(entity.isSpinHold);
        }

        public bool CanMove { get; set; } = true;
        public bool IsJumping { get; set; }

        public void ApplyMouseSensitivity(float value)
        {
            mouseSensitivity = Mathf.Clamp(value, 0.02f, 5f);
        }

        private void OnNetworkSceneLoadComplete(ulong id, string sceneName, LoadSceneMode mode)
        {
            if (!IsOwner) return;
            if (id != NetworkManager.LocalClientId) return;

            gravityBody.Initialize();
            entity.AlignForward();
            inputHandler.HideCursor();

            GamePlayEventHandler.OnUIChanged(sceneName);

            switch (sceneName)
            {
                case "Lobby":
                {
                    Reset();
                    entity.Reset();
                    // health.Reset();
                    readyChecker.Reset();

                    var clients = NetworkManager.ConnectedClientsIds.ToList();

                    var pos = Util.GetCirclePositions(Vector3.zero, clients.IndexOf(id), 2f, 4);

                    transform.SetPositionAndRotation(pos, Quaternion.LookRotation((Vector3.zero - pos).normalized));
                    break;
                }
                case "InGame":
                {
                    break;
                }
            }
        }

        private void SubscribeInputEvent()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete += OnNetworkSceneLoadComplete;

            inputHandler.InputActions.Player.Look.performed += Look;
            inputHandler.InputActions.Player.Look.canceled += Look;
            inputHandler.InputActions.Player.RightClick.performed += Rmb;
            inputHandler.InputActions.Player.RightClick.canceled += Rmb;
            inputHandler.InputActions.Player.Move.performed += Movement;
            inputHandler.InputActions.Player.Move.canceled += Movement;
            inputHandler.InputActions.Player.Run.performed += Run;
            inputHandler.InputActions.Player.Run.canceled += Run;
            inputHandler.InputActions.Player.Spin.performed += Spin;
            inputHandler.InputActions.Player.Spin.canceled += Spin;
            inputHandler.InputActions.Player.Jump.performed += Jump;
            inputHandler.InputActions.Player.Attack.started += Attack;

            hittableBody.healthPoint.OnValueChanged += Hit;
        }

        private void UnsubscribeInputEvent()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnNetworkSceneLoadComplete;

            inputHandler.InputActions.Player.Look.performed -= Look;
            inputHandler.InputActions.Player.Look.canceled -= Look;
            inputHandler.InputActions.Player.RightClick.performed -= Rmb;
            inputHandler.InputActions.Player.RightClick.canceled -= Rmb;
            inputHandler.InputActions.Player.Move.performed -= Movement;
            inputHandler.InputActions.Player.Move.canceled -= Movement;
            inputHandler.InputActions.Player.Run.performed -= Run;
            inputHandler.InputActions.Player.Run.canceled -= Run;
            inputHandler.InputActions.Player.Spin.performed -= Spin;
            inputHandler.InputActions.Player.Spin.canceled -= Spin;
            inputHandler.InputActions.Player.Jump.performed -= Jump;
            inputHandler.InputActions.Player.Attack.performed -= Attack;

            hittableBody.healthPoint.OnValueChanged -= Hit;
        }

        private void HandleMovement()
        {
            if (!CanMove || entity.isSpinHold) return;

            var moveInput = inputHandler.MoveInput;

            if (moveInput == Vector2.zero) return;

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.AddForce(moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime), ForceMode.VelocityChange);
        }

        private void Rmb(InputAction.CallbackContext ctx)
        {
            isAround = ctx.performed;
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

                transform.Rotate(Vector3.up * (inputHandler.LookInput.x * mouseSensitivity));

                CameraManager.Instance.SetEulerAngles(transform.rotation.eulerAngles.y);
            }
        }

        private void Movement(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled)
            {
                CameraManager.Instance.orbit.HorizontalAxis.Value = 0;
                CameraManager.Instance.LookAround();
            }

            animator.OnMove(ctx);
        }

        private void Jump(InputAction.CallbackContext ctx)
        {
            if (IsJumping) return;
            if (!CanMove) return;
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