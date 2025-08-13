using AI;
using EventHandler;
using GamePlay;
using Unity.Netcode;
using UnityEngine;

namespace Players.Roles
{
    public class SeekerRole : NetworkBehaviour
    {
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackRadius = 1f;
        [SerializeField] private LayerMask hiderMask;
        [SerializeField] private Transform attackOrigin;

        private PlayerEntity entity;
        private Hittable hittable;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
            hittable = GetComponent<Hittable>();
        }

        private void OnEnable()
        {
            if (!IsOwner) return;
            GamePlayEventHandler.PlayerAttack += OnPlayerAttack;
            GamePlayEventHandler.NpcDeath += OnNpcDeath;
            entity.playerMarker.color = entity.roleColor.seekerColor;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;

            GamePlayEventHandler.PlayerAttack -= OnPlayerAttack;
            GamePlayEventHandler.NpcDeath -= OnNpcDeath;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position + (transform.forward * attackRange), attackRadius);
        }

        private void OnNpcDeath()
        {
            hittable.Damaged();
        }

        private void OnPlayerAttack()
        {
            print($"client-{entity.clientId.Value} Seeker Attack");

            if (!Physics.SphereCast(attackOrigin.position, attackRadius, transform.forward,
                    out var hit, attackRange, hiderMask)) return;

            var target = hit.collider.gameObject.GetComponent<NetworkObject>();
            hit.collider.GetComponent<PlayerVfx>().HitEffect();

            var targetRef = new NetworkObjectReference(target);

            OnPlayerHitRpc(
                targetRef,
                RpcTarget.Single(target.OwnerClientId, RpcTargetUse.Temp)
            );

            // OnPlayerHitRpc(RpcTarget.Single(target.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnPlayerHitRpc(RpcParams rpcParams)
        {
            print($"target-{OwnerClientId} Hit");

            var target = NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<Hittable>();

            target.Damaged();
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnPlayerHitRpc(NetworkObjectReference targetRef, RpcParams rpcParams = default)
        {
            if (!targetRef.TryGet(out var nob)) return;

            var hittable = nob.GetComponent<Hittable>();
            if (hittable != null)
            {
                hittable.Damaged();
            }

            if (!nob.TryGetComponent<Npa>(out var npa)) return;

            var dir = Vector3.ProjectOnPlane(transform.position, transform.up).normalized;

            npa.lastHitDirection = (transform.position - (transform.position + dir)).normalized;
        }
    }
}