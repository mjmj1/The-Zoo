using System;
using UI;
using UnityEngine;

namespace Players
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;

        public float mouseSensitivity = 3f;
        public float minPitch = -20f;
        public float maxPitch = 30f;

        public float height = 5f;
        public float distance = 10f;
        public float rotationSmoothTime = 0.1f;
        
        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _defaultOffset;

        private float _pitch = 10f;

        private Transform _planetCenter;

        private void Start()
        {
            _planetCenter = FindAnyObjectByType<PlanetGravity>()?.transform;

            _defaultOffset = new Vector3(0, height, -distance);
        }

        private void FixedUpdate()
        {
            if (!target) return;
            if (UIManager.IsCursorLocked()) return;
            
            RotateCamera();
            FollowTarget();
        }

        private void RotateCamera()
        {
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
        }

        private void FollowTarget()
        {
            var gravityDirection = (target.position - _planetCenter.position).normalized;

            var pitchRotation = Quaternion.AngleAxis(_pitch, target.right);
            var localOffset = pitchRotation * (target.rotation * _defaultOffset);

            var targetPosition = target.position + localOffset;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity,
                rotationSmoothTime);

            transform.rotation = Quaternion.LookRotation(target.position - transform.position, gravityDirection);
        }
    }
}