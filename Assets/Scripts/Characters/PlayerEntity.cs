using Unity.Netcode;

namespace Characters
{
    public class PlayerEntity : NetworkBehaviour
    {
        private void Awake()
        {
            if (!IsOwner) return;
        }

        private void Start()
        {
            if (!IsOwner) return;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
        }
    }
}