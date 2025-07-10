namespace Interactions
{
    public interface IDamageable : IInteractable
    {
        void TakeDamage(float damage);
    }
}