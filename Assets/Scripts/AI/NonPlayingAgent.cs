using System;
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
    public class NonPlayingAgent : Agent, IMoveState
    {
        [Header("Movement")] public float walkSpeed = 4f;

        public float runSpeed = 7f;
        public float jumpForce = 3f;
        public float rotationSpeed = 500f;

        public LayerMask groundMask;

        public Vector2 moveInput;
        public Vector2 lookInput;

        public bool idle;
        public bool look;
        public bool walking;
        public bool running;
        public bool isAction;
        public bool seekerDetected;
        private readonly float slowdownRate = 1f;
        private PlayerNetworkAnimator animator;

        private float moveSpeed;
        private RayPerceptionSensorComponent3D raySensor;

        private Rigidbody rb;
        private bool spinRewardGiven;


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

        private void FixedUpdate()
        {
            idle = moveInput == Vector2.zero;
            walking = moveInput != Vector2.zero;
            look = lookInput.x != 0;
            running = Mathf.Approximately(moveSpeed, runSpeed);
        }

        public bool CanMove { get; set; }
        public bool IsJumping { get; set; }
        public bool IsSpinning { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<PlayerNetworkAnimator>();
            raySensor = GetComponent<RayPerceptionSensorComponent3D>();
        }

        private bool IsGrounded()
        {
            return Physics.CheckSphere(transform.position, 0.05f, groundMask);
        }

        public override void OnEpisodeBegin()
        {
            IsSpinning = false;
            CanMove = true;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(IsGrounded());
            sensor.AddObservation(IsSpinning);
            sensor.AddObservation(IsJumping);
            sensor.AddObservation(idle);
            sensor.AddObservation(look);
            sensor.AddObservation(walking);
            sensor.AddObservation(running);
            sensor.AddObservation(isAction);

            seekerDetected = false;

            var observations = raySensor.RaySensor.RayPerceptionOutput;
            if (observations?.RayOutputs == null) return;

            foreach (var output in observations.RayOutputs)
                if (output.HitTagIndex == 0)
                {
                    seekerDetected = true;
                    break;
                }

            sensor.AddObservation(seekerDetected);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var continuousActions = actions.ContinuousActions;
            var discreteActions = actions.DiscreteActions;

            HandleMove(discreteActions);
            HandleContinuousActions(continuousActions);
            HandleJumpAction(discreteActions);
            HandleDiscreteActions(discreteActions);
        }

        private void HandleMove(ActionSegment<int> action)
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
                return;
            }

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position +
                            moveDirection * (moveSpeed * slowdownRate * Time.fixedDeltaTime));

            animator.SetBool(PlayerNetworkAnimator.MoveHash, true);
        }

        private void HandleContinuousActions(ActionSegment<float> actions)
        {
            if (!CanMove || IsSpinning) return;

            lookInput.x = actions[0];

            transform.Rotate(Vector3.up * (lookInput.x * 10f));
        }

        private void HandleJumpAction(ActionSegment<int> actions)
        {
            if (actions[2] == 1 && IsGrounded())
            {
                if (!CanMove) return;

                if (!IsSpinning) animator.SetTrigger(PlayerNetworkAnimator.JumpHash);

                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                if (isAction) return;

                StartCoroutine(ActionCooldown());
            }
        }

        private void HandleDiscreteActions(ActionSegment<int> discreteActions)
        {
            if (!CanMove) return;

            switch (discreteActions[3])
            {
                case 1:
                    if (!walking) break;

                    moveSpeed = runSpeed;
                    animator.SetBool(PlayerNetworkAnimator.RunHash, true);
                    animator.SetBool(PlayerNetworkAnimator.SpinHash, false);
                    break;
                case 2:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.SpinHash, true);
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);

                    if (!isAction)
                        StartCoroutine(ActionCooldown());
                    break;
                case 3:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.SpinHash, false);
                    animator.SetBool(PlayerNetworkAnimator.RunHash, false);
                    animator.SetTrigger(PlayerNetworkAnimator.AttackHash);

                    if (!isAction)
                        StartCoroutine(ActionCooldown());
                    break;
                default:
                    moveSpeed = walkSpeed;
                    animator.SetBool(PlayerNetworkAnimator.SpinHash, false);
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

        private IEnumerator ActionCooldown()
        {
            isAction = true;

            yield return new WaitForSeconds(Random.Range(5f, 10f));

            isAction = false;
        }
    }
}