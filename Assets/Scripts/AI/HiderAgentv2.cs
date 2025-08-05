using System;
using System.Collections;
using System.Collections.Generic;
using Players;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Random = UnityEngine.Random;

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
        [SerializeField] private Transform target;
        [SerializeField] private List<Transform> interactables;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float rotationSpeed = 500f;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public AgentMoveState currentMoveState;
        public AgentActionState prevAAState;
        public AgentActionState currentAAState;
        public Transform foundSeeker;
        public bool isAction;

        private readonly float slowdownRate = 1f;
        private Animator animator;
        private PlayerInputActions input;
        private float moveSpeed;

        private RayPerceptionSensorComponent3D raySensor;
        private BehaviorParameters bp;
        private DecisionRequester dr;
        private Rigidbody rb;

        // freeze
        public bool freeze;
        public float spinHoldTime;
        private readonly float spinTriggerThreshold = 1.2f;

        // Hit
        public bool hasHit;
        public Vector3 lastHitDirection;

        public bool started;

        private float stepReward;

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

            stepReward = 1f / MaxStep;

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            bp = GetComponent<BehaviorParameters>();
            dr = GetComponent<DecisionRequester>();
            raySensor = GetComponent<RayPerceptionSensorComponent3D>();

            input = new PlayerInputActions();

            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += _ => moveInput = Vector2.zero;

            input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            input.Player.Look.canceled += _ => lookInput = Vector2.zero;

            input.UI.Escape.performed += EscapePressed;
            input.UI.Click.performed += MouseLeftClicked;

            if (bp.BehaviorType == BehaviorType.HeuristicOnly)
            {
                dr.DecisionPeriod = 1;
            }
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

            if (observations.RayOutputs == null) return false;

            foreach (var sub in observations.RayOutputs)
                if (sub.HitTagIndex == 0)
                {
                    tr = sub.HitGameObject.transform;
                    return true;
                }

            return false;
        }

        public override void OnEpisodeBegin()
        {
            StopAllCoroutines();

            started = false;

            transform.position = Util.GetRandomPositionInSphere(PlanetGravity.Instance.GetRadius());

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            CanMove = true;
            isAction = false;
            IsSpinning = false;

            started = true;
            freeze = false;
            StartCoroutine(Freeze());
            StartCoroutine(HitCycle());

            foundSeeker = null;
            spinHoldTime = 0f;

            currentMoveState = AgentMoveState.Idle;
            currentAAState = AgentActionState.None;
            prevAAState = AgentActionState.None;

            hasHit = false;
            lastHitDirection = Vector3.zero;

            if (seeker != null)
                seeker.position =
                    Util.GetRandomPositionInSphere(8f);

            if (target != null)
                target.position =
                    Util.GetRandomPositionInSphere(8f);

            foreach (var interactable in interactables)
            {
                interactable.position = Util.GetRandomPositionInSphere(8.5f);

                var normal = -PlanetGravity.Instance.GetGravityDirection(interactable.position);
                interactable.rotation = Quaternion.FromToRotation(interactable.transform.up, normal);
            }

        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(moveInput);
            sensor.AddObservation(lookInput);
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(foundSeeker);
            sensor.AddObservation(isAction);
            sensor.AddObservation(IsGrounded());
            sensor.AddObservation(IsSpinning);
            sensor.AddObservation(freeze);
            sensor.AddObservation(hasHit);
            sensor.AddObservation(lastHitDirection);
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
            if (!CanMove) return;
            if (!IsGrounded()) return;

            if (action[2] == 1)
            {
                currentAAState = AgentActionState.Jumping;

                if (!IsSpinning)
                    animator.SetTrigger(PlayerNetworkAnimator.JumpHash);

                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                PlayActionCycle();
            }
            else
            {
                currentAAState = AgentActionState.None;
            }
        }

        private void HandleSpinAction(ActionSegment<int> action)
        {
            if (!CanMove) return;
            if (!IsGrounded()) return;

            if (action[3] == 1)
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, true);
                currentAAState = AgentActionState.Spinning;

                spinHoldTime += Time.deltaTime;

                if (spinHoldTime >= spinTriggerThreshold)
                    PlayActionCycle();
            }
            else
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, false);

                spinHoldTime = 0f;
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
                    currentAAState = AgentActionState.Attacking;
                    PlayActionCycle();
                    break;
                default:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    break;
            }
        }

        private void HandleRewards()
        {
            AddReward(-stepReward);

            var moveDir = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up).normalized;
            var lookDir = transform.forward;

            var dot = Vector3.Dot(moveDir, lookDir);

            if (hasHit)
            {
                var hitDot = Vector3.Dot(moveDir, lastHitDirection);

                if (currentMoveState != AgentMoveState.Idle && hitDot > 0.7f)
                {
                    print($"Hit Reward");
                    AddReward(hitDot * stepReward * 5f);

                    if (currentAAState == AgentActionState.Jumping)
                    {
                        print($"Hit Bonus Reward");
                        AddReward(hitDot * stepReward * 0.01f);
                    }
                }

                return;
            }

            if (freeze)
            {
                if (currentAAState != AgentActionState.None)
                {
                    print($"Freeze Action Penalty");
                    AddReward(-stepReward);
                }

                if (currentMoveState != AgentMoveState.Idle)
                {
                    print($"Freeze Penalty");
                    AddReward(stepReward * -3f);
                }
                else if (currentMoveState == AgentMoveState.Idle)
                {
                    print($"Freeze Reward");
                    AddReward(stepReward * 3f);
                }
            }
            else
            {
                if (!IsSpinning)
                {
                    if (currentMoveState == AgentMoveState.Walking)
                    {
                        print($"Walking Action");
                        var reward = dot * stepReward * -5f;
                        AddReward(reward);
                    }
                    else if (currentMoveState == AgentMoveState.Running)
                    {
                        print($"Running Action");
                        var reward = dot * stepReward * -5.2f;
                        AddReward(reward);
                    }
                    else if (currentMoveState == AgentMoveState.Idle)
                    {
                        print($"Idle Penalty");
                        AddReward(stepReward * -10f);
                    }
                }
            }

            if (currentAAState != prevAAState)
            {
                switch (currentAAState)
                {
                    case AgentActionState.Jumping when isAction:
                        print("Jumping Penalty");
                        AddReward(stepReward * -4f);
                        break;
                    case AgentActionState.Jumping:
                        print("Jumping Reward");
                        AddReward(stepReward * 2f);
                        break;
                    case AgentActionState.Attacking when isAction:
                        print("Attacking Penalty");
                        AddReward(stepReward * -4f);
                        break;
                    case AgentActionState.Attacking:
                        print("Attacking Reward");
                        AddReward(stepReward * 2f);
                        break;
                    case AgentActionState.Spinning when isAction:
                        print("Spinning Penalty");
                        AddReward(stepReward * -4f);
                        break;
                    case AgentActionState.Spinning:
                        print("Spinning Reward");
                        AddReward(stepReward * 2f);
                        break;
                }
            }

            prevAAState = currentAAState;

            if (IsSeekerFind(out var tr)) foundSeeker = tr;

            if (foundSeeker == null) return;

            if (currentAAState == AgentActionState.Spinning)
                AddReward(stepReward * -10f);

            var dist = Vector3.Distance(transform.position, foundSeeker.position);

            if (dist > 10f)
            {
                print("Seeker Avoided");
                AddReward(stepReward * 3f);
                foundSeeker = null;
            }
            else
            {
                print("Seeker closed");
                var penalty = (1f - dist / 10f) * stepReward;
                AddReward(-penalty);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Interactable"))
            {
                print("Collision Enter Penalty");
                AddReward(-0.05f);
            }

            if (collision.collider.CompareTag("Target"))
            {
                print("Target Get");
                AddReward(1f);
                EndEpisode();
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

        private void PlayActionCycle()
        {
            if (isAction) return;

            StartCoroutine(ActionCycle());
        }

        private IEnumerator ActionCycle()
        {
            yield return new WaitForEndOfFrame();
            isAction = true;
            yield return new WaitForSeconds(Random.Range(10f, 15f));
            isAction = false;
        }

        private IEnumerator Freeze()
        {
            while (started)
            {
                yield return new WaitForSeconds(Random.Range(4f, 8f));

                PlayFreezeCycle();
            }
        }

        private void PlayFreezeCycle()
        {
            if(freeze) return;

            StartCoroutine(FreezeCycle());
        }

        private IEnumerator FreezeCycle()
        {
            freeze = true;
            print("Freeze");

            yield return new WaitForSeconds(Random.Range(1f, 6f));

            freeze = false;
            print("Unfreeze");
        }

        private IEnumerator HitCycle()
        {
            while (started)
            {
                yield return new WaitForSeconds(Random.Range(8f, 15f));

                hasHit = true;

                Damaged();

                yield return new WaitForSeconds(Random.Range(2f, 4f));

                hasHit = false;
            }
        }

        private void Damaged()
        {
            var randomDir = Random.onUnitSphere;
            randomDir = Vector3.ProjectOnPlane(randomDir, transform.up).normalized;

            lastHitDirection = (transform.position - (transform.position + randomDir)).normalized;

            Debug.DrawRay(transform.position, lastHitDirection, Color.red, 5f);
            Debug.DrawRay(transform.position, rb.linearVelocity.normalized, Color.green);
        }
    }
}