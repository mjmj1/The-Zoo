using Unity.Netcode;
using UnityEngine;

namespace Characters.Roles
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
            print($"client-{entity.clientId.Value} Assigned Hider");
        }
    }
}
