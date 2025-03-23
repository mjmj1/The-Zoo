using System;
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

        Transform _planetCenter;
        Vector3 _defaultOffset;
        
        float _pitch = 10f;
        Vector3 _currentVelocity = Vector3.zero;
        
        void Start()
        {
            /*Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;*/
            
            _planetCenter = FindAnyObjectByType<PlanetGravity>()?.transform;
            
            _defaultOffset = new Vector3(0, height, -distance);
        }
        
        void FixedUpdate()
        {
            if (!target) return;
            
            RotateCamera();
            FollowTarget();
        }
        
        void RotateCamera()
        {
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
        }
        
        void FollowTarget()
        {
            var gravityDirection = (target.position - _planetCenter.position).normalized;

            var pitchRotation = Quaternion.AngleAxis(_pitch, target.right);
            var localOffset = pitchRotation * (target.rotation * _defaultOffset);

            var targetPosition = target.position + localOffset;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, rotationSmoothTime);
            
            transform.rotation = Quaternion.LookRotation(target.position - transform.position, gravityDirection);
        }
    }
}
