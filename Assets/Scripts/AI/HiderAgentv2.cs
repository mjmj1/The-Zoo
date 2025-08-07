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
            SpinStart,
            Spinning,
            SpinEnd
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
        [SerializeField] private LPlanetGravity planet;

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
        private int lastSpinInput = 0;
        private readonly float spinTriggerThreshold = 1.2f;

        // Hit
        public bool hasHit;
        public Vector3 lastHitDirection;

        public bool started;

        private void Start()
        {
            planet.Subscribe(rb);
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
            planet.Unsubscribe(rb);
        }

        public bool CanMove { get; set; }
        public bool IsSpinning { get; set; }

        public override void Initialize()
        {
            base.Initialize();

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

            transform.position = planet.transform.position + Util.GetRandomPositionInSphere(planet.GetRadius());

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
                    planet.transform.position + Util.GetRandomPositionInSphere(8f);

            if (target != null)
                target.position =
                    planet.transform.position + Util.GetRandomPositionInSphere(8f);

            foreach (var interactable in interactables)
            {
                interactable.position = planet.transform.position + Util.GetRandomPositionInSphere(8.5f);

                var normal = -planet.GetGravityDirection(interactable.position);
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

            transform.Rotate(Vector3.up * (lookInput.x * 3f));
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

                if (lastSpinInput == 0)
                {
                    spinHoldTime = 0f;
                    currentAAState = AgentActionState.SpinStart;
                }
                else
                {
                    spinHoldTime += Time.deltaTime;
                    if (spinHoldTime >= spinTriggerThreshold)
                        currentAAState = AgentActionState.SpinEnd;
                    else
                        currentAAState = AgentActionState.Spinning;
                }
                PlayActionCycle();
            }
            else
            {
                animator.SetBool(PlayerNetworkAnimator.SpinHash, false);

                spinHoldTime = 0f;
            }

            lastSpinInput = action[3];
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
            AddReward(-0.0001f);

            var moveDir = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up).normalized;
            var lookDir = transform.forward;

            var dot = Vector3.Dot(moveDir, lookDir);

            AddReward(-Mathf.Abs(lookInput.x) * 0.0001f);

            if (hasHit)
            {
                var hitDot = Vector3.Dot(moveDir, lastHitDirection);

                if (currentMoveState != AgentMoveState.Idle && hitDot > 0.7f)
                {
                    //print($"Hit Reward");
                    AddReward(hitDot * 0.0005f);
                }

                return;
            }

            if (freeze)
            {
                if (currentAAState != AgentActionState.None)
                {
                    //print($"Freeze Action Penalty");
                    AddReward(-0.005f);
                }

                if (currentMoveState != AgentMoveState.Idle)
                {
                    //print($"Freeze Penalty");
                    AddReward(-0.005f);
                }
                else if (currentMoveState == AgentMoveState.Idle)
                {
                    //print($"Freeze Reward");
                    AddReward(-0.0025f);
                }
            }
            else
            {
                if (!IsSpinning)
                {
                    if (currentMoveState == AgentMoveState.Walking)
                    {
                        //print($"Walking Action");
                        var reward = dot * 0.001f;
                        AddReward(reward);
                    }
                    else if (currentMoveState == AgentMoveState.Running)
                    {
                        //print($"Running Action");
                        var reward = dot * 0.0011f;
                        AddReward(reward);
                    }
                }
            }

            if (currentAAState != prevAAState)
            {
                switch (currentAAState)
                {
                    case AgentActionState.Jumping:
                        if (isAction)
                        {
                            //print("Jumping Penalty");
                            AddReward(-0.005f);
                        }
                        else
                        {
                            //print("Jumping Reward");
                            AddReward(0.001f);
                        }

                        break;
                    case AgentActionState.Attacking:
                        if (isAction)
                        {
                            //print("Attacking Penalty");
                            AddReward(-0.005f);
                        }
                        else
                        {
                            //print("Attacking Reward");
                            AddReward(0.001f);
                        }
                        break;
                    case AgentActionState.SpinStart:
                        if (isAction)
                        {
                            //print("SpinStart Penalty");
                            AddReward(-0.005f);
                        }
                        else
                        {
                            //print("SpinStart Reward");
                            AddReward(0.001f);
                        }
                        break;
                    case AgentActionState.SpinEnd:
                        if (isAction)
                        {
                            // print("SpinEnd Reward");
                            AddReward(0.001f);
                        }
                        break;
                }
            }

            prevAAState = currentAAState;

            if (currentAAState == AgentActionState.Spinning)
            {
                if (isAction)
                {
                    //print("Spinning Reward");
                    AddReward(0.000001f);
                }
            }

            if (IsSeekerFind(out var tr)) foundSeeker = tr;

            if (foundSeeker == null) return;

            if (currentAAState != AgentActionState.None)
                AddReward(-0.01f);

            var dist = Vector3.Distance(transform.position, foundSeeker.position);

            if (dist > 10f)
            {
                //print("Seeker Avoided");
                AddReward(0.005f);
                foundSeeker = null;
            }
            else
            {
                //print("Seeker closed");
                var penalty = (1f - dist / 10f) * 0.005f;
                AddReward(-penalty);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Interactable"))
            {
                //print("Collision Enter Penalty");
                AddReward(-0.01f);
            }

            /*if (collision.collider.CompareTag("Target"))
            {
                print("Target Get");
                AddReward(1f);
                EndEpisode();
            }*/
        }


        private void AlignToSurface()
        {
            if (!planet) return;

            var gravityDirection = -planet.GetGravityDirection(transform.position);

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
            isAction = true;
            yield return new WaitForSeconds(Random.Range(10f, 15f));
            isAction = false;
        }

        private IEnumerator Freeze()
        {
            while (started)
            {
                yield return new WaitForSeconds(Random.Range(8f, 15f));

                yield return StartCoroutine(FreezeCycle());
            }
        }

        private IEnumerator FreezeCycle()
        {
            freeze = true;
            //print("Freeze");

            yield return new WaitForSeconds(Random.Range(2f, 5f));

            freeze = false;
            //print("Unfreeze");
        }

        private IEnumerator HitCycle()
        {
            while (started)
            {
                yield return new WaitForSeconds(Random.Range(10f, 15f));

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