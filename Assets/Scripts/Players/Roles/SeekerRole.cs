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

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
        }

        private void OnEnable()
        {
            if (!IsOwner) return;
            GamePlayEventHandler.PlayerAttack += OnPlayerAttack;
            entity.playerMarker.color = entity.roleColor.seekerColor;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;

            GamePlayEventHandler.PlayerAttack -= OnPlayerAttack;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position + (transform.forward * attackRange), attackRadius);
        }

        private void OnPlayerAttack()
        {
            print($"client-{entity.clientId.Value} Seeker Attack");

            if (!Physics.SphereCast(attackOrigin.position, attackRadius, transform.forward,
                    out var hit, attackRange, hiderMask)) return;

            var target = hit.collider.gameObject.GetComponent<PlayerEntity>();
            target.GetComponent<PlayerVfx>().HitEffect();
            OnPlayerHitRpc(RpcTarget.Single(target.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void OnPlayerHitRpc(RpcParams rpcParams)
        {
            print($"target-{OwnerClientId} Hit");

            var target = NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<PlayerEntity>();

            target.Damaged();
        }
    }
}