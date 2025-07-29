using Unity.Netcode;

namespace Players.Roles
{
    public class HiderRole : NetworkBehaviour
    {
        private PlayerEntity entity;

        private void Awake()
        {
            entity = GetComponent<PlayerEntity>();
        }

        private void OnEnable()
        {
            if (!IsOwner) return;
            entity.playerMarker.color = entity.roleColor.hiderColors;
        }
    }
}
