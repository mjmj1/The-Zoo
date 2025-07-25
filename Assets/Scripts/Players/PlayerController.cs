#if UNITY_EDITOR
using System.Collections;
using Characters;
using EventHandler;
using GamePlay;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;
using Random = UnityEngine.Random;

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

    public class PlayerController : NetworkTransform, ICameraTarget
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

        internal bool CanMove = true;
        private bool isSpin;
        private float slowdownRate = 1f;

        private PlayerNetworkAnimator animator;
        private PlayerEntity entity;
        private InputHandler input;

        private float moveSpeed;
        private Quaternion previousRotation;

        private CharacterController cc;

        private Rigidbody rb;

        public float Pitch { get; set; }

        private void Start()
        {
            if (!IsOwner) return;

            moveSpeed = walkSpeed;
        }

        private void Update()
        {
            if (!IsOwner) return;

            Look(null);
            AlignToSurface();
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
            PlanetGravity.Instance?.Unsubscribe(rb);

            base.OnNetworkDespawn();
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(transform.position, 0.05f, groundMask);;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
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

            //input.InputActions.Player.Look.performed += Look;
            //input.InputActions.Player.Look.canceled += Look;
            input.InputActions.Player.Move.performed += Movement;
            input.InputActions.Player.Move.canceled += Movement;
            input.InputActions.Player.Run.performed += Run;
            input.InputActions.Player.Run.canceled += Run;
            input.InputActions.Player.Spin.performed += Spin;
            input.InputActions.Player.Spin.canceled += Spin;
            input.InputActions.Player.Jump.performed += Jump;
            input.InputActions.Player.Attack.performed += Attack;

            entity.health.OnValueChanged += Hit;
        }

        private void Unsubscribe()
        {
            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadComplete -= OnOnLoadComplete;

            //input.InputActions.Player.Look.performed -= Look;
            //input.InputActions.Player.Look.canceled -= Look;
            input.InputActions.Player.Move.performed -= Movement;
            input.InputActions.Player.Move.canceled -= Movement;
            input.InputActions.Player.Run.performed -= Run;
            input.InputActions.Player.Run.canceled -= Run;
            input.InputActions.Player.Spin.performed -= Spin;
            input.InputActions.Player.Spin.canceled -= Spin;
            input.InputActions.Player.Jump.performed -= Jump;
            input.InputActions.Player.Attack.performed -= Attack;

            entity.health.OnValueChanged -= Hit;
        }

        private void InitializeComponent()
        {
            if (!IsOwner) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            input = GetComponent<InputHandler>();
            entity = GetComponent<PlayerEntity>();
            animator = GetComponent<PlayerNetworkAnimator>();
        }

        private void InitializeFollowCamera()
        {
            if (!IsOwner) return;

            CameraManager.Instance.Find();

            CameraManager.Instance.SetFollowTarget(transform);
        }

        private void InitializeGravity()
        {
            if (!IsOwner) return;

            rb.useGravity = !PlanetGravity.Instance;

            PlanetGravity.Instance.Subscribe(rb);
        }

        private void HandleMovement()
        {
            if (!CanMove || isSpin) return;

            var moveInput = input.MoveInput;

            if (moveInput == Vector2.zero) return;

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position + moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));
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

        private void Look(InputAction.CallbackContext? ctx)
        {
            // if (rb.linearVelocity.magnitude < 0.001f) return;

            /*
            var lookInput = input.LookInput;

            transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));
            */

            /*var euler = transform.rotation.eulerAngles;
            euler.y = CameraManager.Instance.GetEulerAnglesY();
            transform.rotation = Quaternion.Euler(euler);*/

            var rotation = transform.localRotation;
            rotation.y = CameraManager.Instance.follow.transform.localRotation.y;
            transform.localRotation = rotation;

            /*var camForward = Vector3.ProjectOnPlane(CameraManager.Instance.follow.transform.forward, transform.up).normalized;

            var targetRotation = Quaternion.LookRotation(camForward, transform.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, Time.deltaTime);*/
        }

        private void Movement(InputAction.CallbackContext ctx)
        {
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
            if (ctx.performed) moveSpeed = runSpeed;
            if (ctx.canceled) moveSpeed = walkSpeed;

            animator.OnRun(ctx);
        }

        private void Attack(InputAction.CallbackContext ctx)
        {
            if (!CanMove) return;
            if (!IsGrounded()) return;
            if (isSpin) return;

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
            else Death();
        }

        private void Death()
        {
            StartCoroutine(DeathCoroutine());

            animator.OnDeath();
        }

        private IEnumerator DeathCoroutine()
        {
            yield return new WaitForSeconds(1f);

            NetworkObject.Despawn();

            PlayManager.Instance.ChangeObserverMode(transform);
        }

        private IEnumerator Slowdown()
        {
            slowdownRate = 0.7f;

            yield return new WaitForSeconds(0.3f);

            slowdownRate = 1f;
        }
    }
}