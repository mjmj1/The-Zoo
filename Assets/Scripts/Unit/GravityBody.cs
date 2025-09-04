using UnityEngine;

namespace World
{
    public class GravityBody : MonoBehaviour
    {
        private readonly float alignSpeed = 500f;
        private Rigidbody rb;

        public void Initialize()
        {
            rb.useGravity = !PlanetGravity.Instance;
            PlanetGravity.Instance?.Subscribe(rb);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            PlanetGravity.Instance?.Unsubscribe(rb);
        }

        private void Update()
        {
            AlignToSurface();
        }

        private void AlignToSurface()
        {
            if (!PlanetGravity.Instance) return;

            var gravityDirection = -PlanetGravity.Instance.GetGravityDirection(transform.position);

            var targetRotation = Quaternion.FromToRotation(
                transform.up, gravityDirection) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
        }
    }
}