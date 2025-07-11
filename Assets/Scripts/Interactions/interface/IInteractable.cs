using System;
using UnityEngine;
using UnityEngine.UI;

namespace Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public enum InteractableType
        {
            LeftClick,
            RightClick,
            R,
            F
        }

        [SerializeField] protected Image interactionUI;
        [SerializeField] protected RectTransform canvas;
        private Camera cam;
        public Vector3 offset;

        protected virtual void Start()
        {
            cam = Camera.main;
            offset = new Vector3(1.0f, 1.0f, 0);
            
            if (interactionUI != null)
            {
                interactionUI.gameObject.SetActive(false);
            }
        }

        public void ShowInteractableUI()
        {
            if (!interactionUI) return;
            
            interactionUI.gameObject.SetActive(true);
            
            if (!cam.transform.hasChanged) return;

            interactionUI.transform.position = cam.WorldToScreenPoint(transform.position + offset);
        }

        public void HideInteractableUI()
        {
            interactionUI?.gameObject.SetActive(false);
        }

        public abstract void StartInteract();
        public abstract void StopInteract();

        public abstract InteractableType GetInteractableType();
    }
}