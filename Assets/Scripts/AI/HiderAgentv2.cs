using System.Collections;
using Players;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace AI
{
    public class HiderAgentv2 : Agent, IMoveState
    {
        public enum AgentMoveState
        {
            Idle,
            Walking,
            Running,
        }

        public enum AgentActionState
        {
            Jumping,
            Attacking,
            Interacting,
            Spinning,
        }

        public enum HiderState
        {
            Peaceful,
            FindingMission,
            DoMission,
            SeekerDetected,
            IsChased,
            HasHit,
        }

        [SerializeField] private Transform seeker;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float rotationSpeed = 500f;

        public bool idle;
        public bool look;
        public bool walking;
        public bool running;
        public bool spinning;
        public bool attacking;
        public bool isAction;
        public bool isGrounded;
        public bool seekerDetected;
        public bool hasGivenTurnAwayReward;

        private RayPerceptionSensorComponent3D raySensor;
        private PlayerInputActions input;
        private BehaviorParameters bp;
        private Animator animator;
        private Rigidbody rb;

        public Vector2 moveInput;
        public Vector2 lookInput;

        private readonly float slowdownRate = 1f;
        private float moveSpeed;

        public bool CanMove { get; set; }
        public bool IsSpinning { get; set; }

        private void Update()
        {
            AlignToSurface();
        }

        private void FixedUpdate()
        {
            idle = moveInput == Vector2.zero;
            walking = moveInput != Vector2.zero;
            look = lookInput.x != 0;
            spinning = IsSpinning;
            attacking = !CanMove;
            isGrounded = IsGrounded();
            running = Mathf.Approximately(moveSpeed, runSpeed);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            input.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            input.Disable();
        }

        private void OnDestroy()
        {
            input.UI.Escape.performed -= EscapePressed;
            input.UI.Click.performed -= MouseLeftClicked;

            PlanetGravity.Instance.Unsubscribe(rb);
        }

        public override void Initialize()
        {
            base.Initialize();

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            raySensor = GetComponent<RayPerceptionSensorComponent3D>();
            bp = GetComponent<BehaviorParameters>();

            input = new PlayerInputActions();

            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += _ => moveInput = Vector2.zero;

            input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            input.Player.Look.canceled += _ => lookInput = Vector2.zero;

            input.UI.Escape.performed += EscapePressed;
            input.UI.Click.performed += MouseLeftClicked;
        }

        private void EscapePressed(InputAction.CallbackContext ctx)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            input.Player.Disable();
        }

        private void MouseLeftClicked(InputAction.CallbackContext ctx)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            input.Player.Enable();
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(transform.position, 0.05f, groundMask);
        }

        public override void OnEpisodeBegin()
        {
            if (bp.BehaviorType == BehaviorType.InferenceOnly) return;

            transform.position = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            IsSpinning = false;
            CanMove = true;

            PlanetGravity.Instance.Subscribe(rb);

            if (seeker != null)
                seeker.position =
                    Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(IsGrounded());
            sensor.AddObservation(IsSpinning);
            sensor.AddObservation(idle);
            sensor.AddObservation(look);
            sensor.AddObservation(walking);
            sensor.AddObservation(running);
            sensor.AddObservation(isAction);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;
            var discreteActions = actions.DiscreteActions;

            HandleMoveActions(discreteActions);
            HandleLookActions(continuousActions);
            HandleJumpAction(discreteActions);
            HandleSpecialActions(discreteActions);

            if (bp.BehaviorType != BehaviorType.InferenceOnly)
            {
                HandleRewards();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (IsSpinning) return;

            var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;

            continuousActionsOut[0] = Mathf.Clamp(lookInput.x, -1, 1);

            discreteActionsOut[0] = moveInput.y switch
            {
                > 0 => 1,
                < 0 => 2,
                _ => 0
            };
            discreteActionsOut[1] = moveInput.x switch
            {
                > 0 => 1,
                < 0 => 2,
                _ => 0
            };

            discreteActionsOut[2] = input.Player.Jump.phase == InputActionPhase.Performed ? 1 : 0;

            if (input.Player.Run.phase == InputActionPhase.Performed)
                discreteActionsOut[3] = 1;
            else if (input.Player.Spin.phase == InputActionPhase.Performed)
                discreteActionsOut[3] = 2;
            else if (input.Player.Attack.phase == InputActionPhase.Performed)
                discreteActionsOut[3] = 3;
            else
                discreteActionsOut[3] = 0;
        }

        private void HandleMoveActions(ActionSegment<int> action)
        {
            if (!CanMove || IsSpinning) return;

            moveInput.y = action[0] switch
            {
                1 => 1f,
                2 => -1f,
                _ => 0f
            };
            moveInput.x = action[1] switch
            {
                1 => 1f,
                2 => -1f,
                _ => 0f
            };

            if (moveInput == Vector2.zero)
            {
                animator?.SetBool(PlayerNetworkAnimator.MoveHash, false);
                return;
            }

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position +
                            moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));

            animator?.SetBool(PlayerNetworkAnimator.MoveHash, true);
        }

        private void HandleLookActions(ActionSegment<float> actions)
        {
            if (!CanMove || IsSpinning) return;

            lookInput.x = actions[0];

            transform.Rotate(Vector3.up * (lookInput.x * 10f));
        }

        private void HandleJumpAction(ActionSegment<int> action)
        {
            if (action[2] == 1 && IsGrounded())
            {
                if (!CanMove) return;

                if (!IsSpinning) animator?.SetTrigger(PlayerNetworkAnimator.JumpHash);

                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                if (isAction) return;

                StartCoroutine(ActionCooldown());
            }
        }

        private void HandleSpecialActions(ActionSegment<int> action)
        {
            if (!CanMove) return;

            switch (action[3])
            {
                case 1:
                    if (!walking) break;

                    moveSpeed = runSpeed;
                    animator?.SetBool(PlayerNetworkAnimator.RunHash, true);
                    animator?.SetBool(PlayerNetworkAnimator.SpinHash, false);
                    break;
                case 2:
                    moveSpeed = walkSpeed;
                    animator?.SetBool(PlayerNetworkAnimator.SpinHash, true);
                    animator?.SetBool(PlayerNetworkAnimator.RunHash, false);

                    if (!isAction)
                        StartCoroutine(ActionCooldown());
                    break;
                case 3:
                    moveSpeed = walkSpeed;
                    animator?.SetBool(PlayerNetworkAnimator.SpinHash, false);
                    animator?.SetBool(PlayerNetworkAnimator.RunHash, false);
                    animator?.SetTrigger(PlayerNetworkAnimator.AttackHash);

                    if (!isAction)
                        StartCoroutine(ActionCooldown());
                    break;
                default:
                    moveSpeed = walkSpeed;
                    animator?.SetBool(PlayerNetworkAnimator.SpinHash, false);
                    animator?.SetBool(PlayerNetworkAnimator.RunHash, false);
                    break;
            }
        }

        private void HandleRewards()
        {
            // 1) 생존 보상
            AddReward(1f / MaxStep);

            // 2) 자연스러운 행동 보상
            if (running) AddReward(0.0005f);
            else if (walking) AddReward(0.0007f);
            else if (idle && Random.value < 0.5f) AddReward(0.001f);

            if (look) AddReward(0.0001f);

            if (IsSpinning)
            {
                if (isAction)
                    AddReward(-0.005f);
                else
                    AddReward(0.005f);
            }

            if (!IsGrounded())
            {
                if (isAction)
                    AddReward(-0.005f);
                else
                    AddReward(0.005f);
            }
            else if (!CanMove)
            {
                if (isAction)
                    AddReward(-0.005f);
                else
                    AddReward(0.005f);
            }

            seekerDetected = false;

            var observations = raySensor.RaySensor.RayPerceptionOutput;
            if (observations?.RayOutputs == null) return;

            foreach (var output in observations.RayOutputs)
                if (output.HitTagIndex == 0)
                {
                    seekerDetected = true;
                    break;
                }

            if (!seekerDetected)
            {
                hasGivenTurnAwayReward = false;
                return;
            }

            var toSeeker = (seeker.position - transform.position).normalized;
            var dot = Vector3.Dot(seeker.position, -toSeeker);

            var dis = Vector3.Distance(seeker.position, transform.position);
            if (running || idle || IsSpinning || !IsGrounded() || !CanMove)
            {
                if (hasGivenTurnAwayReward)
                {
                    AddReward(-0.015f);
                    return;
                }

                AddReward(0.01f);
                hasGivenTurnAwayReward = true;
            }
            else if (dis > 10f)
            {
                AddReward(0.005f);
            }
            else if (dot > 0.7f)
            {
                AddReward(0.005f);
            }
            else
            {
                AddReward(-0.01f);
            }
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

        private IEnumerator ActionCooldown()
        {
            isAction = true;

            yield return new WaitForSeconds(Random.Range(5f, 10f));

            isAction = false;
        }
    }
}