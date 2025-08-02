using Interactions;
using Players.Roles;
using UnityEngine;
using Utils;

namespace Players
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 2f;

        private Interactable currentInteractable;
        private InputHandler inputHandler;

        private void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            

            inputHandler.InputActions.Player.Interact.performed += _ => currentInteractable?.StartInteract();
            inputHandler.InputActions.Player.Interact.canceled += _ => currentInteractable?.StopInteract();
        }

        private void FixedUpdate()
        {
            CheckForInteractable();
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
            MyLogger.Print(this, "gameObject.layer : " + this.gameObject.layer);
            if (this.gameObject.layer != 8)// 8 is layered as Seeker
            {
                currentInteractable = interactable;
                currentInteractable?.ShowInteractableUI();
            }
        }

        private void UnfocusInteractable()
        {
            if (!currentInteractable) return;
            
            currentInteractable?.HideInteractableUI();
            
            currentInteractable = null;
        }

        private void Interact()
        {
            
        }
    }
}