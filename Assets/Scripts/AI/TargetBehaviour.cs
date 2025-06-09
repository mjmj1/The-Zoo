using UnityEngine;

namespace AI
{
    public class TargetBehaviour : MonoBehaviour
    {
        private PlanetGravity _gravity;
        private Transform _planet;
        private Rigidbody _rb;
        
        private readonly float _rotationSpeed = 50f;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _gravity = FindAnyObjectByType<PlanetGravity>();

            _rb.useGravity = !_gravity;

            _planet = _gravity?.gameObject.transform;
            _gravity?.Subscribe(_rb);
        }
        
        private void OnDestroy()
        {
            _gravity?.Unsubscribe(_rb);
        }
        
        private void FixedUpdate()
        {
            AlignToSurface();
        }

        private void OnTriggerEnter(Collider other)
        {
            var agent = other.GetComponent<AgentBehaviour>();
            if (!agent) return;

            agent.FindTarget(transform);
            agent.EndEpisode();
        }

        private void AlignToSurface()
        {
            if (!_gravity) return;

            var gravityDirection = (transform.position - _planet.position).normalized;

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}