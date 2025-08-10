using System.Collections;
using Players;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace AI
{
    public class AgentTransform : NetworkTransform
    {
        public NetworkVariable<int> health = new (3);

        private Rigidbody rb;
        private NPAv2 npa;

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

            health.OnValueChanged += npa.Hit;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            PlanetGravity.Instance?.Unsubscribe(rb);

            health.OnValueChanged -= npa.Hit;
        }

        internal void OnDeath()
        {
            OnDeferringDespawn(1);
        }
    }
}