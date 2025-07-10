using System;
using Interactions;
using UI;
using UnityEngine;

namespace Characters
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private InGameUIManager uiManager;

        private InputHandler inputHandler;
        
        private IInteractable currentInteractable;

        private void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            
            inputHandler.InputActions.Player.Interact.performed += _ => Interact();
        }
        
        private void FixedUpdate()
        {
            CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            if (Physics.Raycast(transform.position, transform.forward, out var hit, interactionDistance))
            {
                hit.collider.TryGetComponent<IInteractable>(out var interactable);
                
                UpdateInteractable(interactable);
            }
            else
            {
                ClearInteractable();
            }
        }

        private void UpdateInteractable(IInteractable interactable)
        {
            string prompt = "";
            
            switch (interactable.GetInteractableType())
            {
                case IInteractable.InteractableType.LeftClick:
                    prompt += "Attack (LMB)\n";
                    break;
                case IInteractable.InteractableType.RightClick:
                    break;
                case IInteractable.InteractableType.F:
                    prompt += currentInteractable.GetInteractableType() + " (F)\n";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (!string.IsNullOrEmpty(prompt))
            {
                uiManager.ShowInteractionPrompt(prompt.Trim());
            }
            else
            {
                uiManager.HideInteractionPrompt();
            }
        }

        private void ClearInteractable()
        {
            if (currentInteractable == null) return;

            currentInteractable = null;
            uiManager.HideInteractionPrompt();
        }

        private void Interact()
        {
            currentInteractable?.StartInteract();
        }
    }
}