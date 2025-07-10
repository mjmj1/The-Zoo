using System;
using Interactions;
using UnityEngine;

namespace Characters
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 2f;

        private Interactable currentInteractable;
        private InputHandler inputHandler;

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
            if (!Physics.Raycast(transform.position, transform.forward, out var hit,
                    interactionDistance)) return;

            currentInteractable?.gameObject.SetActive(false);

            if (!hit.collider.TryGetComponent<Interactable>(out var interactable)) return;

            currentInteractable = interactable;
            
            currentInteractable?.gameObject.SetActive(true);
        }

        private void UpdateInteractable()
        {
            currentInteractable?.ShowInteractableUI();
        }

        private void Interact()
        {
            currentInteractable?.StartInteract();
        }
    }
}