using UI;
using UnityEngine;

namespace Characters
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public float height = 5f;
        public float distance = 10f;
        private Vector3 _defaultOffset;

        private Transform _target;
        private Transform _planetCenter;
        
        private CharacterHandler _characterHandler;

        private void Start()
        {
            _planetCenter = FindAnyObjectByType<PlanetGravity>()?.transform;
            _defaultOffset = new Vector3(0, height, -distance);
        }

        private void FixedUpdate()
        {
            if (!_target) return;

            UpdateCameraPosition();
        }

        public void ConnectToTarget(Transform target)
        {
            _target = target;
            _characterHandler = target.GetComponent<CharacterHandler>();
        }

        private void UpdateCameraPosition()
        {
            var gravityDir = _planetCenter ? (_target.position - _planetCenter.position).normalized : Vector3.up;
            var pitchRot = Quaternion.AngleAxis(_characterHandler.Pitch, _target.right);
            var offset = pitchRot * (_target.rotation * _defaultOffset);
            var targetPos = _target.position + offset;

            transform.position = targetPos; //Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.LookRotation(_target.position - transform.position, gravityDir);
        }
    }
}