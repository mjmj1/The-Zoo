using UnityEngine;

namespace Players
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public float height = 5f;
        public float distance = 10f;
        private Vector3 defaultOffset;

        private Transform target;
        private Transform planetCenter;
        
        private ICameraTarget cameraTarget;

        private void Start()
        {
            planetCenter = FindAnyObjectByType<PlanetGravity>()?.transform;
            defaultOffset = new Vector3(0, height, -distance);
        }

        private void FixedUpdate()
        {
            if (!target) return;

            UpdateCameraPosition();
        }

        public void ConnectToTarget(Transform obj)
        {
            target = obj;
            cameraTarget = obj.GetComponent<ICameraTarget>();
        }

        private void UpdateCameraPosition()
        {
            var gravityDir = planetCenter ? (target.position - planetCenter.position).normalized : Vector3.up;
            var pitchRot = Quaternion.AngleAxis(cameraTarget.Pitch, target.right);
            var offset = pitchRot * (target.rotation * defaultOffset);
            var targetPos = target.position + offset;

            transform.position = targetPos;

            transform.rotation = Quaternion.LookRotation(target.position - transform.position, gravityDir);
        }
    }
}