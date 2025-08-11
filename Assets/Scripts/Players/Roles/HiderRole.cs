using EventHandler;
using Interactions;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Players.Roles
{
    public class HiderRole : NetworkBehaviour
    {
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private LayerMask layer;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackRadius = 1f;
        [SerializeField] private Transform attackOrigin;

        private Interactable currentInteractable;

        private PlayerEntity entity;
        private InputHandler inputHandler;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
        }

        private void Start()
        {
            inputHandler = GetComponent<InputHandler>();

            inputHandler.InputActions.Player.Interact.performed +=
                _ => currentInteractable?.StartInteract();
            inputHandler.InputActions.Player.Interact.canceled +=
                _ => currentInteractable?.StopInteract();
        }

        private void FixedUpdate()
        {
            if (IsOwner) CheckForInteractable();
        }

        private void OnEnable()
        {
            if (!IsOwner) return;

            entity.playerMarker.color = entity.roleColor.hiderColor;
        }

        private void OnPlayerAttack()
        {
            print($"client-{entity.clientId.Value} Seeker Attack");

            if (!Physics.SphereCast(attackOrigin.position, attackRadius, transform.forward,
                    out var hit, attackRange, layer)) return;


        }

        private void CheckForInteractable()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hit,
                    interactionDistance, layer))
            {
                UnfocusInteractable();
                return;
            }

            if (!hit.collider.TryGetComponent<Interactable>(out var interactable)) return;
            
            FocusInteractable(interactable);
        }

        private void FocusInteractable(Interactable interactable)
        {
            if (interactable && currentInteractable == interactable) return;

            currentInteractable = interactable;
            var target = currentInteractable.targetMission.Value;
            var count = currentInteractable.maxSpawnCount.Value;
            GamePlayEventHandler.OnCheckInteractable(true, target, count);
        }

        private void UnfocusInteractable()
        {
            if (!currentInteractable) return;

            currentInteractable = null;
            GamePlayEventHandler.OnCheckInteractable(false, false, 0);
        }
    }
}