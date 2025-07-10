using UnityEngine;

namespace Interactions
{
    public class SpawnerInteractable : Interactable
    {
        private bool isInteracting = false;
        
        public override void StartInteract()
        {
            if (isInteracting) return;
            
            isInteracting = true;
            print($"{gameObject.name} is interacting...");
        }

        public override void StopInteract()
        {
            if (!isInteracting) return;
            
            isInteracting = false;
            print($"{gameObject.name} is stop interacting...");
        }
        
        public override Interactable.InteractableType GetInteractableType()
        {
            return Interactable.InteractableType.LeftClick;
        }
    }
}