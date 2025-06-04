using System;
using Characters;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace AI
{
    public class AgentBehaviour : Agent
    {
        private const float AlignmentRewardWeight = 0.005f;
        private const float Reward = 1f;

        public Vector2 moveInput;
        public bool turnLeft;
        public bool turnRight;

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private float spawnRadius = 7.5f;
        [SerializeField] private Transform target;
        private Animator _animator;
        private PlanetGravity _gravity;
        private PlayerInputActions _inputActions;


        private Transform _planet;
        private Rigidbody _rb;

        private float _stepReward;

        private bool _spinPressed;

        private bool SpinPressed
        {
            get => _spinPressed;
            set
            {
                _spinPressed = value;
                OnSpinPressed?.Invoke(_spinPressed);
            }
        }

        private bool _movePressed;
        private bool MovePressed
        {
            get => _movePressed;
            set
            {
                _movePressed = value;
                OnMovePressed?.Invoke(_movePressed);
            }
        }

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

        public event Action<bool> OnMovePressed;
        public event Action<bool> OnSpinPressed;

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
            var moveX = actionBuffers.DiscreteActions[0];
            var moveZ = actionBuffers.DiscreteActions[1];
            var rot = actionBuffers.DiscreteActions[2];
            var r = actionBuffers.DiscreteActions[3];
            var f = actionBuffers.DiscreteActions[4];

            Movement(moveX, moveZ);
            Rotation(rot);

            // TODO: F
            if (f == 1)
            {
                // SpinPressed = f == 1;
            }

            if (r == 1)
            {
                // TODO: R
            }

            AddReward(-_stepReward);

            var toTarget = (target.position - transform.position).normalized;

            var alignToTarget = Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, toTarget));

            if (alignToTarget > 0.9f) AddReward(_stepReward * alignToTarget);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var d = actionsOut.DiscreteActions;

            // Branch 0: move z
            d[0] = moveInput.y switch
            {
                > 0 => 1, // forward
                < 0 => 2, // backward
                _ => 0
            };

            // Branch 0: move x
            d[1] = moveInput.x switch
            {
                < 0 => 1, // left
                > 0 => 2, // right
                _ => 0
            };

            // Branch 2: rotate
            if (turnLeft) d[2] = 1;
            else if (turnRight) d[2] = 2;
            else d[2] = 0;

            // Branch 3: R
            d[3] = _inputActions.Player.Interact.IsPressed() ? 1 : 0;

            // Branch 4: F
            d[4] = _inputActions.Player.Spin.IsPressed() ? 1 : 0;
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

            _inputActions.Player.TurnLeft.performed += _ => turnLeft = true;
            _inputActions.Player.TurnLeft.canceled += _ => turnLeft = false;

            _inputActions.Player.TurnRight.performed += _ => turnRight = true;
            _inputActions.Player.TurnRight.canceled += _ => turnRight = false;

            _inputActions.Player.Spin.performed += ctx => SpinPressed = true;
            _inputActions.Player.Spin.canceled += ctx => SpinPressed = false;

            OnMovePressed += MovementAction;
            OnSpinPressed += SpinAction;

            _inputActions.Enable();
        }

        private void MovementAction(bool value)
        {
            _animator.SetBool(CharacterHandler.MoveId, value);
        }

        private void SpinAction(bool value)
        {
            _animator.SetBool(CharacterHandler.SpinId, value);
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

        private void Movement(int z, int x)
        {
            if (SpinPressed) return;

            MovePressed = z + x > 0;

            var moveZ = z switch
            {
                1 => 1f, // forward
                2 => -1f, // backward
                _ => 0f
            };

            var moveX = x switch
            {
                1 => -1f, // left
                2 => 1f, // right
                _ => 0f
            };

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