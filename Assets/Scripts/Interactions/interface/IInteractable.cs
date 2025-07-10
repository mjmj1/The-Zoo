namespace Interactions
{
    public interface IInteractable
    {
        enum InteractableType
        {
            LeftClick,
            RightClick,
            R,
            F,
        }
        
        void StartInteract();
        
        void StopInteract();
        
        InteractableType GetInteractableType();
    }
}