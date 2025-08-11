using UnityEngine;

namespace Interactions
{
    public class PickupGravity : MonoBehaviour
    {
        private Rigidbody rb;
        private readonly float rotationSpeed = 500f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            rb.useGravity = !PlanetGravity.Instance;

            PlanetGravity.Instance?.Subscribe(rb);
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
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
