using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utils;

namespace AI
{
    public class SphereTrainAgent : Agent
    {
        private const float AlignSpeed = 500f;
        [Header("References")]
        public TrainPlanetGravity planet;
        public Transform target;
        public Transform seeker;
        public Transform interactions;

        [Header("Params")]
        public float moveSpeed = 3f;

        public Vector2 moveInput;
        public Vector2 lookInput;

        private RayPerceptionSensorComponent3D raySensor;
        private DecisionRequester dr;
        private BehaviorParameters bp;

        private Rigidbody rb;
        private PlayerInputActions input;

        private void Update()
        {
            AlignToSurface();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Interactable"))
            {
                AddReward(-0.01f);
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Seeker"))
            {
                print("Seeker Get");
                AddReward(-1f);
                EndEpisode();
            }

            if (col.CompareTag("Target"))
            {
                print("Target Get");
                AddReward(1f);
                EndEpisode();
            }
        }

        public override void Initialize()
        {
            rb = GetComponent<Rigidbody>();
            planet.Subscribe(rb);

            input = new PlayerInputActions();

            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += _ => moveInput = Vector2.zero;
            input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            input.Player.Look.canceled += _ => lookInput = Vector2.zero;

            dr = GetComponent<DecisionRequester>();
            bp = GetComponent<BehaviorParameters>();

            if (bp.BehaviorType == BehaviorType.HeuristicOnly)
            {
                dr.DecisionPeriod = 1;
                input.Player.Enable();
            }
            else
            {
                input.Player.Disable();
            }
        }

        public override void OnEpisodeBegin()
        {
            var cen = planet.transform.position;

            transform.position = cen + Util.GetRandomPositionInSphere(planet.GetRadius());

            if (raySensor) raySensor.enabled = true;

            if (seeker)
                seeker.position = cen + Util.GetRandomPositionInSphere(8f);

            if (target)
                target.position = planet.transform.position + Util.GetRandomPositionInSphere(8f);

            foreach (Transform interactable in interactions)
            {
                interactable.position = cen + Util.GetRandomPositionInSphere(8f);

                var normal = -planet.GetGravityDirection(interactable.position);
                interactable.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(rb.linearVelocity.normalized);
            sensor.AddObservation(rb.linearVelocity.magnitude);
            sensor.AddObservation(transform.up.normalized);
            sensor.AddObservation(transform.forward.normalized);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            MoveActions(actions.DiscreteActions);
            LookActions(actions.ContinuousActions);

            HandleRewards();
        }

        private void MoveActions(ActionSegment<int> action)
        {
            moveInput.y = action[0] switch { 1 => 1f, 2 => -1f, _ => 0f };
            moveInput.x = action[1] switch { 1 => 1f, 2 => -1f, _ => 0f };

            var moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            moveDirection.Normalize();

            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }

        private void LookActions(ActionSegment<float> action)
        {
            lookInput.x = action[0];

            transform.Rotate(Vector3.up * (lookInput.x * 3f));
        }

        private void HandleRewards()
        {
            AddReward(-0.001f);

            var moveDir = Vector3.ProjectOnPlane(rb.linearVelocity.normalized, transform.up).normalized;
            var lookDir = transform.forward;

            var dot = Vector3.Dot(moveDir, lookDir);

            if (dot is > 0.8f or < 0f)
            {
                AddReward(dot * 0.001f);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var cOut = actionsOut.ContinuousActions;
            var dOut = actionsOut.DiscreteActions;

            cOut[0] = lookInput.x;

            dOut[0] = moveInput.y switch { > 0 => 1, < 0 => 2, _ => 0 };
            dOut[1] = moveInput.x switch { > 0 => 1, < 0 => 2, _ => 0 };
        }

        private void AlignToSurface()
        {
            if (!planet) return;

            var gravityDirection = -planet.GetGravityDirection(transform.position);

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, AlignSpeed * Time.deltaTime);
        }

        private float SphericalDistance(Vector3 a, Vector3 b)
        {
            var cen = planet.transform.position;
            var na = (a - cen).normalized;
            var nb = (b - cen).normalized;
            var dot = Mathf.Clamp(Vector3.Dot(na, nb), -1f, 1f);
            var angle = Mathf.Acos(dot);
            return planet.GetRadius() * angle;
        }
    }
}