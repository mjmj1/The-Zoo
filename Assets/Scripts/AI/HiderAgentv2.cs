using System;
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
            None,
            Jumping,
            Looking,
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

        private RayPerceptionSensorComponent3D raySensor;
        private PlayerInputActions input;
        private Animator animator;
        private Rigidbody rb;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public AgentMoveState currentMoveState;
        public AgentActionState currentAAState;
        public HiderState currentHiderState;

        private readonly float slowdownRate = 1f;
        private float moveSpeed;

        public bool CanMove { get; set; }
        public bool IsSpinning { get; set; }

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

        private void Start()
        {
            PlanetGravity.Instance.Subscribe(rb);
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

        public override void OnEpisodeBegin()
        {
            transform.position = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            CanMove = true;
            IsSpinning = false;

            if (seeker != null)
                seeker.position =
                    Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());
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
            sensor.AddObservation((int)currentHiderState);
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

            if (input.Player.Spin.phase == InputActionPhase.Performed)
                discreteActionsOut[3] = 1;
            else if (input.Player.Spin.phase == InputActionPhase.Canceled)
                discreteActionsOut[3] = 0;

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

            rb.MovePosition(rb.position + moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));
        }

        private void HandleLookActions(ActionSegment<float> action)
        {
            if (!CanMove || IsSpinning) return;

            lookInput.x = action[0];

            currentAAState = lookInput.x != 0 ? AgentActionState.Looking : AgentActionState.None;

            transform.Rotate(Vector3.up * (lookInput.x * 10f));
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
                IsSpinning = true;
            }
            else if(action[3] == 0)
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, false);
                currentAAState = AgentActionState.None;
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
                    moveSpeed = runSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, true);
                    if (moveInput == Vector2.zero) break;
                    currentMoveState = AgentMoveState.Running;
                    break;
                case 2:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    animator.SetTrigger(PlayerNetworkAnimator.AttackHash);
                    currentAAState = AgentActionState.Attacking;
                    break;
                default:
                    if (currentAAState == AgentActionState.Looking) break;
                    if (currentAAState == AgentActionState.Jumping) break;
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    currentAAState = AgentActionState.None;
                    break;
            }
        }

        private void HandleRewards()
        {

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
    }
}