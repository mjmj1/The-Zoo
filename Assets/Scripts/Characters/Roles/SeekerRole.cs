using EventHandler;
using GamePlay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Roles
{
    public class SeekerRole : NetworkBehaviour
    {
        [Tooltip("공격 사정거리")] [SerializeField] private float attackRange = 1f;

        [Tooltip("공격 판정 구체 반경")] [SerializeField]
        private float attackRadius = 1f;

        [Tooltip("Hider 레이어 마스크")] [SerializeField]
        private LayerMask hiderMask;

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

        public void OnPlayerAttack()
        {
            print($"client-{entity.clientId.Value} Seeker Attack");

            if (!Physics.SphereCast(attackOrigin.position, attackRadius, transform.forward,
                    out var hit, attackRange, hiderMask)) return;

            var target = hit.collider.gameObject.GetComponent<PlayerEntity>();

            print($"target-{target.OwnerClientId} Seeker Hit");

            PlayManager.Instance.HitRpc(target.OwnerClientId, RpcTarget.Single(target.OwnerClientId, RpcTargetUse.Temp));
        }
    }
}