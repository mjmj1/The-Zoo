using Characters;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace AI
{
    public class AgentBehaviour : Agent
    {
        public Vector2 moveInput;
        public bool turnLeft;
        public bool turnRight;
        public bool spinPressed;

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private float spawnRadius = 7.5f;
        [SerializeField] private Transform target;
        private Animator _animator;
        private PlanetGravity _gravity;
        private PlayerInputActions _inputActions;
        private Transform _planet;
        private Rigidbody _rb;

        private const float AlignmentRewardWeight = 0.005f;
        private const float Reward = 1f;
        private float _stepReward;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) RequestDecision();
        }

        private void FixedUpdate()
        {
            AlignToSurface();
        }

        private void OnDestroy()
        {
            _gravity?.Unsubscribe(_rb);
            _inputActions.Disable();
        }

        public override void Initialize()
        {
            base.Initialize();

            _stepReward = Reward / MaxStep;

            Time.timeScale = 20f; // 20배 빠르게 진행
            Application.targetFrameRate = -1; // 최대 프레임 제한 해제

            _rb = GetComponent<Rigidbody>();

            _animator = GetComponent<Animator>();

            _gravity = FindAnyObjectByType<PlanetGravity>();

            _planet = _gravity?.gameObject.transform;
            _gravity?.Subscribe(_rb);

            InitializeInputSystem();
        }

        public override void OnEpisodeBegin()
        {
            MoveRandomPosition();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var toTarget = (target.position - transform.position).normalized;

            sensor.AddObservation(toTarget);
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var moveX = actionBuffers.ContinuousActions[0];
            var moveZ = actionBuffers.ContinuousActions[1];
            var rotateAction = actionBuffers.DiscreteActions[0];
            var r = actionBuffers.DiscreteActions[1];
            var f = actionBuffers.DiscreteActions[2];

            Movement(moveX, moveZ);
            Rotation(rotateAction);

            if (f == 1)
            {
                // TODO: F
            }

            if (r == 1)
            {
                // TODO: R
            }

            AddReward(-_stepReward);

            var toTarget = (target.position - transform.position).normalized;

            var alignToTarget = Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, toTarget));

            if (alignToTarget > 0.9f)
            {
                AddReward(_stepReward * alignToTarget);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var c = actionsOut.ContinuousActions;
            var d = actionsOut.DiscreteActions;

            c[0] = moveInput.x;
            c[1] = moveInput.y;
            
            // Branch 0: rotate
            if (turnLeft) d[0] = 1;
            else if (turnRight) d[0] = 2;
            else d[0] = 0;

            // Branch 1: R
            d[1] = _inputActions.Player.Interact.IsPressed() ? 1 : 0;

            // Branch 2: F
            d[2] = _inputActions.Player.Spin.IsPressed() ? 1 : 0;
        }

        public void FindTarget()
        {
            var toTarget = (target.position - transform.position).normalized;

            var alignToTarget = Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, toTarget));

            AddReward(Reward * alignToTarget);
        }

        private void InitializeInputSystem()
        {
            _inputActions = new PlayerInputActions();

            _inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            _inputActions.Player.Move.performed += MovementAction;
            _inputActions.Player.Move.canceled += MovementAction;

            _inputActions.Player.TurnLeft.performed += _ => turnLeft = true;
            _inputActions.Player.TurnLeft.canceled += _ => turnLeft = false;

            _inputActions.Player.TurnRight.performed += _ => turnRight = true;
            _inputActions.Player.TurnRight.canceled += _ => turnRight = false;

            _inputActions.Player.Spin.performed += ctx =>
            {
                spinPressed = true;
                SpinAction(spinPressed);
            };

            _inputActions.Player.Spin.canceled += ctx =>
            {
                spinPressed = false;
                SpinAction(spinPressed);
            };

            _inputActions.Enable();
        }

        private void MovementAction(InputAction.CallbackContext ctx)
        {
            _animator.SetBool(CharacterHandler.MoveId, ctx.performed);
        }

        private void SpinAction(bool value)
        {
            _animator.SetBool(CharacterHandler.SpinId, value);

            MyLogger.Print(this, $"{value}");
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

        private void Movement(float moveX, float moveZ)
        {
            if (spinPressed) return;

            var moveDir = transform.forward * moveZ + transform.right * moveX;
            moveDir.Normalize();

            _rb.MovePosition(_rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
        }

        private void Rotation(int action)
        {
            var rotation = action switch
            {
                1 => -rotationSpeed,
                2 => rotationSpeed,
                _ => 0f
            };

            transform.Rotate(Vector3.up * rotation * Time.deltaTime);
        }

        private void MoveRandomPosition()
        {
            target.transform.position = GetRandomPosition();
            transform.position = GetRandomPosition();
        }

        private Vector3 GetRandomPosition()
        {
            return Random.onUnitSphere.normalized * spawnRadius;
        }
    }
}