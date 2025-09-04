using Unity.Netcode;

namespace Unit
{
    public class HittableBody : NetworkBehaviour
    {
        public NetworkVariable<int> healthPoint = new (3);

        public void SetHealthPoint(int hp)
        {
            healthPoint.Value = hp;
        }

        public void Damaged(int damage)
        {
            healthPoint.Value -= damage;
        }

        public void Healed(int heal)
        {
            healthPoint.Value += heal;
        }
    }
}