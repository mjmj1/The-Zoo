using Unity.Netcode;

namespace Players
{
    public class PlayerHealth : NetworkBehaviour
    {
        public NetworkVariable<int> health = new (3);

        public void Reset()
        {
            health.Value = 3;
        }

        public void Damaged()
        {
            health.Value -= 1;
        }
    }
}