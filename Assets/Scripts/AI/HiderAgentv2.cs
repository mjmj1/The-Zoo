using System.Collections;
using Players;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace AI
{
    public class HiderAgentv2 : Agent, IMoveState
    {
        public enum AgentActionState
        {
            None,
            Jumping,
            Attacking,
            Interacting,
            Spinning
        }

        public enum AgentMoveState
        {
            Idle,
            Walking,
            Running
        }

        [SerializeField] private Transform seeker;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float rotationSpeed = 500f;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public AgentMoveState currentMoveState;
        public AgentActionState currentAAState;

        private readonly float slowdownRate = 1f;
        private Animator animator;
        private PlayerInputActions input;
        private float moveSpeed;

        private RayPerceptionSensorComponent3D raySensor;
        private Rigidbody rb;
        public Transform foundSeeker;
        public bool isAction;
        public bool isSpinningAction;
        public bool nowSpinning;

        private void Start()
        {
            PlanetGravity.Instance.Subscribe(rb);
        }

        private void Update()
        {
            AlignToSurface();
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

        public bool CanMove { get; set; }
        public bool IsSpinning { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            raySensor = GetComponent<RayPerceptionSensorComponent3D>();

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

            input.Player.Disable();
        }

        private void MouseLeftClicked(InputAction.CallbackContext ctx)
        {
            Cursor.lockState = CursorLockMode.Locked;

            input.Player.Enable();
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(transform.position, 0.05f, groundMask);
        }

        private bool IsSeekerFind(out Transform tr)
        {
            tr = null;

            if (raySensor == null) return false;

            var observations = raySensor.RaySensor.RayPerceptionOutput;

            if (observations?.RayOutputs == null) return false;

            foreach (var sub in observations?.RayOutputs)
            {
                if (sub.HitTagIndex == 0)
                {
                    tr = sub.HitGameObject.transform;
                    return true;
                }
            }

            return false;
        }

        public override void OnEpisodeBegin()
        {
            transform.position = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            CanMove = true;
            IsSpinning = false;

            if (seeker != null)
                seeker.position =
                    Util.GetRandomPositionInSphere(8f);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(moveInput);
            sensor.AddObservation(lookInput);
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(IsGrounded());
            sensor.AddObservation(IsSpinning);
            sensor.AddObservation((int)currentMoveState);
            sensor.AddObservation((int)currentAAState);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;
            var discreteActions = actions.DiscreteActions;

            HandleMoveActions(discreteActions);
            HandleLookActions(continuousActions);
            HandleJumpAction(discreteActions);
            HandleSpinAction(discreteActions);
            HandleExtraActions(discreteActions);
            HandleRewards();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;

            continuousActionsOut[0] = lookInput.x;

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
            discreteActionsOut[3] = input.Player.Spin.phase == InputActionPhase.Performed ? 1 : 0;

            if (input.Player.Run.phase == InputActionPhase.Performed)
                discreteActionsOut[4] = 1;
            else if (input.Player.Attack.phase == InputActionPhase.Performed)
                discreteActionsOut[4] = 2;
            else if (input.Player.Interact.phase == InputActionPhase.Performed)
                discreteActionsOut[4] = 3;
            else
                discreteActionsOut[4] = 0;
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
                animator.SetBool(PlayerNetworkAnimator.MoveHash, false);
                currentMoveState = AgentMoveState.Idle;
            }
            else
            {
                animator.SetBool(PlayerNetworkAnimator.MoveHash, true);
                currentMoveState = AgentMoveState.Walking;
            }

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position +
                            moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));
        }

        private void HandleLookActions(ActionSegment<float> action)
        {
            if (!CanMove || IsSpinning) return;

            lookInput.x = action[0];

            transform.Rotate(Vector3.up * (lookInput.x * 5f));
        }

        private void HandleJumpAction(ActionSegment<int> action)
        {
            if (action[2] == 0 || !IsGrounded()) return;
            if (!CanMove) return;

            currentAAState = AgentActionState.Jumping;

            if (!IsSpinning) animator.SetTrigger(PlayerNetworkAnimator.JumpHash);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        private void HandleSpinAction(ActionSegment<int> action)
        {
            if (!CanMove) return;

            if (action[3] == 1)
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, true);
                currentAAState = AgentActionState.Spinning;
                currentMoveState = AgentMoveState.Idle;
                StartCoroutine(SpinAction());
                IsSpinning = true;
            }
            else if (action[3] == 0)
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, false);
                currentAAState = AgentActionState.None;
                StartCoroutine(SpinActionCycle());
                IsSpinning = false;
            }
        }

        private void HandleExtraActions(ActionSegment<int> action)
        {
            if (!CanMove) return;
            if (IsSpinning) return;
            if (!IsGrounded()) return;

            switch (action[4])
            {
                case 1:
                    if (moveInput == Vector2.zero) break;
                    moveSpeed = runSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, true);
                    currentMoveState = AgentMoveState.Running;
                    break;
                case 2:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    animator.SetTrigger(PlayerNetworkAnimator.AttackHash);
                    StartCoroutine(ActionCycle());
                    currentAAState = AgentActionState.Attacking;
                    break;
                default:
                    if (currentAAState == AgentActionState.Jumping) break;
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    StartCoroutine(ActionCycle());
                    currentAAState = AgentActionState.None;
                    break;
            }
        }

        private void HandleRewards()
        {
            AddReward(1f / MaxStep);

            var moveDir = rb.linearVelocity.normalized;
            var lookDir = transform.forward;

            var dot = Vector3.Dot(moveDir, lookDir);

            if (currentMoveState != AgentMoveState.Idle)
            {
                var forwardMoveReward = Mathf.Max(0f, dot);
                AddReward(forwardMoveReward * 0.01f);
            }

            if (IsSeekerFind(out var tr))
            {
                foundSeeker = tr;
            }

            if (foundSeeker != null)
            {
                var dist = Vector3.Distance(transform.position, foundSeeker.position);

                if (dist > 10f)
                {
                    AddReward(0.005f);
                    foundSeeker = null;
                }
                else
                {
                    var penalty = (1f - dist / 10f) * 0.005f;
                    AddReward(-penalty);
                }

                if (currentAAState == AgentActionState.Jumping)
                {
                    if(isAction) AddReward(-0.005f);
                    else
                    {
                        AddReward(0.0025f);
                    }
                }
                else if (currentAAState == AgentActionState.Attacking)
                {
                    if(isAction) AddReward(-0.005f);
                    else
                    {
                        AddReward(0.0025f);
                    }
                }
                else if (currentAAState == AgentActionState.Spinning)
                {
                    if(isSpinningAction) AddReward(-0.01f);
                    else
                    {
                        if(nowSpinning && IsSpinning) AddReward(0.0025f);
                        else AddReward(-0.0025f);
                    }
                }
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

        private IEnumerator ActionCycle()
        {
            isAction = true;
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            isAction = false;
        }

        private IEnumerator SpinActionCycle()
        {
            isSpinningAction = true;
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            isSpinningAction = false;
        }

        private IEnumerator SpinAction()
        {
            nowSpinning = true;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            nowSpinning = false;
        }
    }
}