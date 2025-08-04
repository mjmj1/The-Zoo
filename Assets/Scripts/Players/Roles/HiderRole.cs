using Interactions;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Players.Roles
{
    public class HiderRole : NetworkBehaviour
    {
        private PlayerEntity entity;
        [SerializeField] private float interactionDistance = 2f;

        private Interactable currentInteractable;
        private InputHandler inputHandler;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
        }
        private void OnEnable()
        {
            if (!IsOwner) return;

            entity.playerMarker.color = entity.roleColor.hiderColor;
        }
        private void Start()
        {
            inputHandler = GetComponent<InputHandler>();


            inputHandler.InputActions.Player.Interact.performed += _ => currentInteractable?.StartInteract();
            inputHandler.InputActions.Player.Interact.canceled += _ => currentInteractable?.StopInteract();
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                CheckForInteractable();
            }
        }

        private void LateUpdate()
        {
            // UnfocusInteractable();
        }

        private void CheckForInteractable()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hit,
                    interactionDistance))
            {
                UnfocusInteractable();
                return;
            }

            if (!hit.collider.TryGetComponent<Interactable>(out var interactable)) return;

            UpdateInteractable(interactable);
        }

        private void UpdateInteractable(Interactable interactable)
        {
            currentInteractable = interactable;
            //currentInteractable?.ShowInteractableUI();
            InGameUI.instance.KeyUI.SetActive(true);
        }

        private void UnfocusInteractable()
        {
            if (!currentInteractable) return;

            //currentInteractable?.HideInteractableUI();
            InGameUI.instance.KeyUI.SetActive(false);

            currentInteractable = null;
        }
    }
}
