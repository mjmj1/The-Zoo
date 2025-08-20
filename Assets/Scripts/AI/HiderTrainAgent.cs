using System.Collections;
using System.Collections.Generic;
using Maps;
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
    public class HiderTrainAgent : Agent, IMoveState
    {
        public enum AgentActionState
        {
            None,
            Jumping,
            Attacking,
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
        [SerializeField] private Transform interactions;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 4.5f;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private TrainPlanetGravity planet;
        [SerializeField] private Collider groundCollider;

        private float moveSpeed;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public AgentMoveState currentMoveState;
        public AgentActionState prevAAState;
        public AgentActionState currentAAState;
        public Transform foundSeeker;
        public bool isAction;

        // freeze
        public bool freeze;
        public float spinHoldTime;

        // Hit
        public bool hasHit;
        public Vector3 lastHitDirection;

        public bool started;

        private readonly float slowdownRate = 1f;
        private readonly float spinTriggerThreshold = 1.2f;

        // agents components
        private RayPerceptionSensorComponent3D raySensor;
        private DecisionRequester dr;
        private BehaviorParameters bp;

        private PlayerInputActions input;
        private Rigidbody rb;
        private Animator animator;

        private int lastSpinInput;
        private bool isSpinHold;

        private void Start()
        {
            if (!planet) return;
            planet.Subscribe(rb);
        }

        private void FixedUpdate()
        {
            if (!TorusWorld.Instance) return;

            var wrapped = TorusWorld.Instance.WrapXZ(rb.position);

            if (!((wrapped - rb.position).sqrMagnitude > 0.0001f)) return;

            transform.position = wrapped;
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
            if (!planet) return;
            planet.Unsubscribe(rb);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Interactable"))
                //print("Collision Enter Penalty");
                AddReward(-0.01f);

            if (collision.collider.CompareTag("Target"))
            {
                print("Target Get");
                AddReward(1f);
                EndEpisode();
            }
        }

        public bool CanMove { get; set; }
        public bool IsJumping { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
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

            if (bp.BehaviorType == BehaviorType.HeuristicOnly) dr.DecisionPeriod = 1;
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

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            CanMove = true;
            isAction = false;
            isSpinHold = false;

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

            if (planet)
            {
                transform.position = planet.transform.position +
                                     Util.GetRandomPositionInSphere(planet.GetRadius());

                if (seeker != null)
                    seeker.position =
                        planet.transform.position + Util.GetRandomPositionInSphere(8f);

                if (target != null)
                    target.position =
                        planet.transform.position + Util.GetRandomPositionInSphere(8f);

                foreach (Transform interactable in interactions)
                {
                    interactable.position =
                        planet.transform.position + Util.GetRandomPositionInSphere(8.5f);

                    var normal = -planet.GetGravityDirection(interactable.position);
                    interactable.rotation =
                        Quaternion.FromToRotation(interactable.transform.up, normal);
                }
            }
            else
            {
                transform.position = Util.GetRandomPosition(groundCollider);

                if (seeker != null)
                    seeker.position = Util.GetRandomPosition(groundCollider);

                if (target != null)
                    target.position = Util.GetRandomPosition(groundCollider);

                foreach (Transform interactable in interactions)
                {
                    interactable.position = Util.GetRandomPosition(groundCollider);
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(moveInput);
            sensor.AddObservation(lookInput);
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up.normalized);
            sensor.AddObservation(transform.forward.normalized);
            sensor.AddObservation(isAction);
            sensor.AddObservation(IsJumping);
            sensor.AddObservation(isSpinHold);
            sensor.AddObservation(freeze);
            sensor.AddObservation(hasHit);
            sensor.AddObservation(lastHitDirection);
            sensor.AddObservation((int)currentMoveState);
            sensor.AddObservation((int)currentAAState);
            sensor.AddObservation(foundSeeker);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;
            var discreteActions = actions.DiscreteActions;

            HandleMoveActions(discreteActions);
            HandleLookActions(continuousActions);
            HandleJumpAction(discreteActions);
            HandleSpinAction(discreteActions);
            HandleAttackAction(discreteActions);
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
            discreteActionsOut[4] = input.Player.Run.phase == InputActionPhase.Performed ? 1 : 0;
            discreteActionsOut[5] = input.Player.Attack.phase == InputActionPhase.Performed ? 1 : 0;
        }

        private void HandleMoveActions(ActionSegment<int> action)
        {
            if (!CanMove || isSpinHold) return;

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
                animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                currentMoveState = AgentMoveState.Idle;
            }
            else
            {
                if (action[4] == 1)
                {
                    moveSpeed = runSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, true);
                    currentMoveState = AgentMoveState.Running;
                }
                else
                {
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    animator.SetBool(PlayerNetworkAnimator.MoveHash, true);
                    currentMoveState = AgentMoveState.Walking;
                }
            }

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position +
                            moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));
        }

        private void HandleLookActions(ActionSegment<float> action)
        {
            if (!CanMove || isSpinHold) return;

            lookInput.x = action[0];

            transform.Rotate(Vector3.up * (lookInput.x * 10f));
        }

        private void HandleJumpAction(ActionSegment<int> action)
        {
            if (!CanMove || IsJumping || isSpinHold) return;

            if (action[2] == 1)
            {
                currentAAState = AgentActionState.Jumping;

                animator.SetTrigger(PlayerNetworkAnimator.JumpHash);

                PlayActionCycle();
            }
            else
            {
                currentAAState = AgentActionState.None;
            }
        }

        private void HandleSpinAction(ActionSegment<int> action)
        {
            if (!CanMove || IsJumping) return;

            if (action[3] == 1)
            {
                isSpinHold = true;

                currentMoveState = AgentMoveState.Idle;
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
                isSpinHold = false;

                animator.SetBool(PlayerNetworkAnimator.SpinHash, false);

                spinHoldTime = 0f;
            }

            lastSpinInput = action[3];
        }

        private void HandleAttackAction(ActionSegment<int> action)
        {
            if (!CanMove || IsJumping || isSpinHold) return;

            if (action[5] == 1)
            {
                animator.SetTrigger(PlayerNetworkAnimator.AttackHash);
                currentAAState = AgentActionState.Attacking;
                PlayActionCycle();
            }
        }

        private void HandleRewards()
        {
            AddReward(-0.0001f);

            var moveDir = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up).normalized;

            if (hasHit)
            {
                var hitDot = -Vector3.Dot(moveDir, lastHitDirection);

                if (currentMoveState != AgentMoveState.Idle && hitDot > 0.8f)
                    //print($"Hit Reward");
                    AddReward(hitDot * 0.001f);

                return;
            }

            lastHitDirection = Vector3.zero;

            var lookDir = transform.forward;

            var dot = Vector3.Dot(moveDir, lookDir);

            if (!freeze)
            {
                if (currentMoveState != AgentMoveState.Idle)
                    if (dot > 0.8)
                        // print($"Move Action");
                        AddReward(dot * 0.00001f);
            }
            else
            {
                if (currentAAState != AgentActionState.None)
                    //print($"Freeze Action Penalty");
                    AddReward(-0.0001f);

                if (currentMoveState != AgentMoveState.Idle)
                    //print($"Freeze Penalty");
                    AddReward(-0.00001f);
                else if (currentMoveState == AgentMoveState.Idle)
                    //print($"Freeze Reward");
                    AddReward(0.00001f);
            }

            if (currentAAState != prevAAState)
                switch (currentAAState)
                {
                    case AgentActionState.Jumping:
                        if (isAction)
                            //print("Jumping Penalty");
                            AddReward(-0.005f);
                        else
                            //print("Jumping Reward");
                            AddReward(0.001f);

                        break;
                    case AgentActionState.Attacking:
                        if (isAction)
                            //print("Attacking Penalty");
                            AddReward(-0.005f);
                        else
                            //print("Attacking Reward");
                            AddReward(0.001f);
                        break;
                    case AgentActionState.SpinStart:
                        if (isAction)
                            //print("SpinStart Penalty");
                            AddReward(-0.005f);
                        else
                            //print("SpinStart Reward");
                            AddReward(0.0005f);
                        break;
                    case AgentActionState.SpinEnd:
                        if (isAction)
                            // print("SpinEnd Reward");
                            AddReward(0.0005f);
                        break;
                }

            prevAAState = currentAAState;

            if (currentAAState == AgentActionState.Spinning)
                if (isAction)
                    //print("Spinning Reward");
                    AddReward(0.0000001f);

            if (IsSeekerFind(out var tr)) foundSeeker = tr;

            if (foundSeeker == null) return;

            if (currentAAState != AgentActionState.None)
                AddReward(-0.01f);

            var dist = Vector3.Distance(transform.position, foundSeeker.position);

            if (dist > 10f)
            {
                //print("Seeker Avoided");
                AddReward(0.01f);
                foundSeeker = null;
            }
            else
            {
                //print("Seeker closed");
                var penalty = (1f - dist / 10f) * 0.01f;
                AddReward(-penalty);
            }
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
                yield return new WaitForSeconds(Random.Range(15f, 20f));

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