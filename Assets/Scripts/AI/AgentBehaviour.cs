using Characters;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI
{
    public class AgentBehaviour : Agent
    {
        public Vector2 moveInput;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float spawnRadius = 7.5f;
        [SerializeField] private Transform target;
        private PlanetGravity _gravity;
        private PlayerInputActions _inputActions;
        private Transform _planet;
        private Rigidbody _rb;
        private Animator _animator;
        
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
            var localVel = transform.InverseTransformDirection(_rb.linearVelocity);

            sensor.AddObservation(toTarget); // 3D 방향
            sensor.AddObservation(localVel); // 속도
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var moveX = actionBuffers.ContinuousActions[0];
            var moveZ = actionBuffers.ContinuousActions[1];

            var moveDir = transform.forward * moveZ + transform.right * moveX;
            moveDir.Normalize();

            _rb.MovePosition(_rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
            
            RotateTowardsMoveDirection(moveDir);
            
            // 보상 조건
            var distance = Vector3.Distance(transform.position, target.position);

            if (distance < 1.5f)
            {
                SetReward(1f);
                EndEpisode();
            }

            var radius = Vector3.Distance(_planet.position, transform.position);

            if (radius is > 9f or < 6f)
            {
                SetReward(-1f);
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var c = actionsOut.ContinuousActions;
            c[0] = moveInput.x;
            c[1] = moveInput.y;
        }

        private void InitializeInputSystem()
        {
            _inputActions = new PlayerInputActions();

            _inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            _inputActions.Player.Move.performed += MovementAction;
            _inputActions.Player.Move.canceled += MovementAction;
            
            _inputActions.Enable();
        }
        
        private void MovementAction(InputAction.CallbackContext ctx)
        {
            _animator.SetBool(CharacterHandler.MoveId, ctx.performed);
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

        private void RotateTowardsMoveDirection(Vector3 moveDir)
        {
            if (moveDir.sqrMagnitude < 0.001f) return; // 움직이지 않으면 회전하지 않음

            var targetRotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(moveDir, transform.up), transform.up
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5f * Time.fixedDeltaTime
            );
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