using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utils;
using static Characters.InputHandler;

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
        [SerializeField] private Transform seeker;

        private Animator _animator;
        private PlanetGravity _gravity;
        private PlayerInputActions _inputActions;

        private bool _movePressed;
        private Transform _planet;

        private RayPerceptionSensorComponent3D _raySensor;
        private Rigidbody _rb;

        private bool _seekerDetected;

        private bool _spinPressed;

        private float _stepReward;
        private bool _targetDetected;

        private bool SpinPressed
        {
            get => _spinPressed;
            set
            {
                _spinPressed = value;
                OnSpinPressed?.Invoke(_spinPressed);
            }
        }

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

            Time.timeScale = 20f;
            Application.targetFrameRate = -1;

            _rb = GetComponent<Rigidbody>();

            _animator = GetComponent<Animator>();

            _raySensor = GetComponent<RayPerceptionSensorComponent3D>();

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
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(_rb.linearVelocity.normalized);
            sensor.AddObservation(_rb.linearVelocity.magnitude);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var moveX = actionBuffers.DiscreteActions[0];
            var moveZ = actionBuffers.DiscreteActions[1];
            var rot = actionBuffers.DiscreteActions[2];
            var r = actionBuffers.DiscreteActions[3];
            var f = actionBuffers.DiscreteActions[4];
            var seekerResponse = actionBuffers.DiscreteActions[5];

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

            var observations = _raySensor.RaySensor.RayPerceptionOutput;

            if (observations?.RayOutputs == null) return;

            var totalReward = -_stepReward;

            var toSeeker = Vector3.zero;
            var toTarget = Vector3.zero;

            foreach (var output in observations.RayOutputs)
                switch (output.HitTagIndex)
                {
                    case 0:
                    {
                        _seekerDetected = true;

                        toSeeker = (output.HitGameObject.transform.position - transform.position)
                            .normalized;

                        break;
                    }

                    case 1:
                    {
                        _targetDetected = true;

                        toTarget = (output.HitGameObject.transform.position - transform.position)
                            .normalized;

                        break;
                    }
                }

            if (_targetDetected) totalReward += FindTarget(toTarget);

            if (_seekerDetected) totalReward += FindSeeker(seekerResponse, toSeeker);

            if (_movePressed)
            {
                var moveDir = Vector3
                    .ProjectOnPlane(_rb.linearVelocity.normalized, transform.up.normalized)
                    .normalized;
                var alignment = Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, moveDir));

                totalReward += _stepReward * alignment;
            }

            AddReward(totalReward);
        }

        private float FindSeeker(int seekerResponse, Vector3 toSeeker)
        {
            switch (seekerResponse)
            {
                case 0:
                    if (_rb.linearVelocity.magnitude < 0.1f)
                        return _stepReward * 0.5f;
                    return _stepReward * -0.2f;

                case 1:
                    if (!(_rb.linearVelocity.magnitude > 0.1f)) return 0f;

                    return Vector3.Dot(_rb.linearVelocity.normalized, -toSeeker) > 0.7f
                        ? _stepReward * 0.4f
                        : _stepReward * -0.3f;

                case 2:
                    if (!(_rb.linearVelocity.magnitude > 0.1f)) return 0f;

                    var dot = Vector3.Dot(_rb.linearVelocity.normalized,
                        Vector3.Cross(Vector3.up, toSeeker));
                    return Mathf.Abs(dot) > 0.7f ? _stepReward * 0.3f : _stepReward * -0.2f;
            }

            return 0f;
        }

        private float FindTarget(Vector3 toTarget)
        {
            var alignToTarget = Mathf.Clamp01(Vector3.Dot(_rb.linearVelocity.normalized, toTarget));

            return _stepReward * alignToTarget;
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

        public void FindTarget(Transform t)
        {
            var toTarget = (t.position - transform.position).normalized;

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
            _animator.SetBool(MoveHash, value);
        }

        private void SpinAction(bool value)
        {
            _animator.SetBool(SpinHash, value);
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
            transform.position = Util.GetRandomPositionInSphere(spawnRadius);
            seeker.transform.position = Util.GetRandomPositionInSphere(spawnRadius);
            target.transform.position = Util.GetRandomPositionInSphere(spawnRadius);
        }
    }
}