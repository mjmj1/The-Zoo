using System;
using System.Collections;
using EventHandler;
using Interactions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utils;

namespace Players.Roles
{
    public class HiderRole : NetworkBehaviour
    {
        [SerializeField] private LayerMask interactLayer;
        [SerializeField] private LayerMask pickupLayer;
        [SerializeField] private float interactRange = 0.7f;
        [SerializeField] private float interactRadius = 0.3f;
        [SerializeField] private Transform origin;

        private Interactable currentInteractable;

        private PlayerEntity entity;
        private InputHandler inputHandler;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
            inputHandler = GetComponent<InputHandler>();
        }

        private void FixedUpdate()
        {
            if (IsOwner) CheckForInteractable();
        }

        private void OnEnable()
        {
            if (!IsOwner) return;

            entity.playerMarker.color = entity.roleColor.hiderColor;

            inputHandler.InputActions.Player.Interact.performed += Interact;
            inputHandler.InputActions.Player.Interact.canceled += Interact;

            GamePlayEventHandler.PlayerAttack += TryInteract;
        }

        private void OnDisable()
        {
            inputHandler.InputActions.Player.Interact.performed -= Interact;
            inputHandler.InputActions.Player.Interact.canceled -= Interact;

            GamePlayEventHandler.PlayerAttack -= TryInteract;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin.position + (transform.forward * interactRange), interactRadius);
        }

        private void Interact(InputAction.CallbackContext ctx)
        {
            if(ctx.performed)
                currentInteractable?.StartInteract();
            else
                currentInteractable?.StopInteract();
        }

        private void CheckForInteractable()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hit,
                    interactRange, interactLayer))
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

        private void TryInteract()
        {
            if (!IsOwner) return;
            if (!Physics.SphereCast(origin.position, interactRadius, transform.forward,
                    out var hit, interactRange, pickupLayer)) return;

            if (!hit.collider.TryGetComponent<NetworkObject>(out var no)) return;

            RequestPickupRpc(new NetworkObjectReference(no),
                RpcTarget.Single(no.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void RequestPickupRpc(NetworkObjectReference pickupRef, RpcParams param = default)
        {
            if (!pickupRef.TryGet(out var no) || !no.IsSpawned) return;

            if (!no.TryGetComponent<Pickup>(out var p)) return;
            if (p.consumed.Value) return;

            p.consumed.Value = true;

            StartCoroutine(PickupRoutine(no, pickupRef));
        }

        [Rpc(SendTo.Everyone)]
        private void ApplyPickedVisualsRpc(NetworkObjectReference pickupRef)
        {
            if (!pickupRef.TryGet(out var no)) return;

            no.GetComponent<Pickup>().PickUp();
        }

        private IEnumerator PickupRoutine(NetworkObject no, NetworkObjectReference pickupRef)
        {
            ApplyPickedVisualsRpc(pickupRef);

            yield return new WaitForSeconds(0.3f);

            no.DeferDespawn(2);
        }
    }
}