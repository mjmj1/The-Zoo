using System.Collections;
using Players;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public class Npa : Agent, IMoveState
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

        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float jumpForce = 3f;
        [SerializeField] private float rotationSpeed = 500f;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public AgentMoveState currentMoveState;
        public AgentActionState currentAAState;
        public Transform foundSeeker;
        public bool isAction;

        private float slowdownRate = 1f;
        private float moveSpeed;

        private AgentTransform agent;
        private Hittable hittable;
        private RayPerceptionSensorComponent3D raySensor;
        private PlayerNetworkAnimator animator;
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
            if (NetworkManager.Singleton.LocalClientId !=
                NetworkManager.Singleton.CurrentSessionOwner)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            AlignToSurface();
        }

        public bool CanMove { get; set; }
        public bool IsSpinning { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            rb = GetComponent<Rigidbody>();
            agent = GetComponent<AgentTransform>();
            hittable = GetComponent<Hittable>();
            animator = GetComponent<PlayerNetworkAnimator>();
            raySensor = GetComponent<RayPerceptionSensorComponent3D>();
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

        private void OnDestroy()
        {
            hittable.health.OnValueChanged -= Hit;
        }

        public override void OnEpisodeBegin()
        {
            StopAllCoroutines();

            started = false;

            CanMove = true;
            isAction = false;
            IsSpinning = false;

            started = true;
            freeze = false;
            StartCoroutine(Freeze());

            foundSeeker = null;
            spinHoldTime = 0f;

            currentMoveState = AgentMoveState.Idle;
            currentAAState = AgentActionState.None;

            hasHit = false;
            lastHitDirection = Vector3.zero;

            hittable.health.OnValueChanged += Hit;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(moveInput);
            sensor.AddObservation(lookInput);
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up.normalized);
            sensor.AddObservation(transform.forward.normalized);
            IsSeekerFind(out foundSeeker);
            sensor.AddObservation(foundSeeker);
            sensor.AddObservation(isAction);
            sensor.AddObservation(IsGrounded());
            sensor.AddObservation(IsSpinning);
            sensor.AddObservation(freeze);
            sensor.AddObservation(hasHit);
            sensor.AddObservation(lastHitDirection);
            sensor.AddObservation((int)currentMoveState);
            sensor.AddObservation((int)currentAAState);

            if (!foundSeeker) return;

            var dist = Vector3.Distance(transform.position, foundSeeker.position);

            if (dist > 10f)
            {
                foundSeeker = null;
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (agent.isDead.Value) return;

            var continuousActions = actions.ContinuousActions;
            var discreteActions = actions.DiscreteActions;

            HandleMoveActions(discreteActions);
            HandleLookActions(continuousActions);
            HandleJumpAction(discreteActions);
            HandleSpinAction(discreteActions);
            HandleExtraActions(discreteActions);
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

            transform.Rotate(Vector3.up * (lookInput.x * 10f));
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

            yield return new WaitForSeconds(Random.Range(2f, 5f));

            freeze = false;
        }

        private IEnumerator HitCycle()
        {
            hasHit = true;

            yield return new WaitForSeconds(Random.Range(2f, 4f));

            hasHit = false;
        }

        internal void Hit(int previousValue, int newValue)
        {
            StartCoroutine(HitCycle());
            StartCoroutine(Slowdown());

            if (newValue > 0) animator.OnHit();
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Death");
                StartCoroutine(DeathCoroutine());
            }
        }

        private IEnumerator DeathCoroutine()
        {
            agent.isDead.Value = true;

            yield return new WaitForEndOfFrame();

            moveInput = Vector2.zero;

            currentMoveState = AgentMoveState.Idle;
            currentAAState = AgentActionState.None;

            yield return new WaitForEndOfFrame();

            animator.SetBool(PlayerNetworkAnimator.MoveHash, false);
            animator.SetBool(PlayerNetworkAnimator.RunHash, false);

            yield return new WaitForEndOfFrame();

            animator.OnDeath();

            yield return new WaitForSeconds(3f);

            agent.OnDeath();
        }

        private IEnumerator Slowdown()
        {
            slowdownRate = 0.5f;

            yield return new WaitForSeconds(0.3f);

            slowdownRate = 1f;
        }
    }
}