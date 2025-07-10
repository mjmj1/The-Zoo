using UnityEngine;

namespace Interactions
{
    public class SpawnerInteractable : MonoBehaviour, IInteractable
    {
        private bool isInteracting = false;
        
        public void StartInteract()
        {
            if (isInteracting) return;
            
            isInteracting = true;
            print($"{gameObject.name} is interacting...");
        }

        public void StopInteract()
        {
            if (!isInteracting) return;
            
            isInteracting = false;
            print($"{gameObject.name} is stop interacting...");
        }
        
        public IInteractable.InteractableType GetInteractableType()
        {
            return IInteractable.InteractableType.LeftClick;
        }
    }
}