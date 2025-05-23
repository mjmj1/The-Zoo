using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static Static.Strings;

namespace Characters
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<FixedString32Bytes> playerName = new(string.Empty);

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
            
            playerName.Value = PlayerPrefs.GetString(PLAYERNAME);
        }
    }
}