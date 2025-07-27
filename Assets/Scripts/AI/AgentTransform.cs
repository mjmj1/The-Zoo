using Unity.Netcode.Components;
using UnityEngine;

namespace AI
{
    public class AgentTransform : NetworkTransform
    {
        private Rigidbody rb;

        protected override void Awake()
        {
            base.Awake();

            rb = GetComponent<Rigidbody>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            PlanetGravity.Instance?.Unsubscribe(rb);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            rb.useGravity = !PlanetGravity.Instance;
            rb.isKinematic = false;
            PlanetGravity.Instance?.Subscribe(rb);

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            PlanetGravity.Instance?.Unsubscribe(rb);
        }
    }
}