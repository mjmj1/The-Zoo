using System;
using Maps;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace AI
{
    public class AgentTransform : NetworkTransform
    {
        public NetworkVariable<bool> isDead = new();
        private Rigidbody rb;

        protected override void Awake()
        {
            base.Awake();

            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!TorusWorld.Instance) return;

            var wrapped = TorusWorld.Instance.WrapXZ(rb.position);

            if (!((wrapped - rb.position).sqrMagnitude > 0.0001f)) return;

            rb.position = wrapped;
            Teleport(wrapped, transform.rotation, transform.localScale);
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
            PlanetGravity.Instance?.Unsubscribe(rb);

            base.OnNetworkDespawn();
        }

        internal void OnDeath()
        {
            RequestDespawnRpc(new NetworkObjectReference(NetworkObject));
        }

        [Rpc(SendTo.Authority, RequireOwnership = false)]
        private void RequestDespawnRpc(NetworkObjectReference targetRef)
        {
            if (!targetRef.TryGet(out var no)) return;
            if (OwnerClientId != no.OwnerClientId) return;
            if (!no.IsSpawned) return;
            no.DeferDespawn(1);
        }
    }
}