using Unity.Netcode;

namespace Players
{
    public class Hittable : NetworkBehaviour
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