using EventHandler;
using Networks;
using TMPro;
using UI.PlayerList;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace Characters
{
    public class PlayerEntity : NetworkBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;

        public NetworkVariable<FixedString32Bytes> playerName = new();
        public NetworkVariable<ulong> clientId = new();
        public AssignedSeekerRole seekerRole;
        public AssignedHiderRole hiderRole;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            playerName.OnValueChanged += OnPlayerNameChanged;
            clientId.OnValueChanged += OnClientIdChanged;
            
            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);

            if (!IsOwner) return;

            playerName.Value = AuthenticationService.Instance.PlayerName;
            clientId.Value = NetworkManager.LocalClientId;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            playerName.OnValueChanged -= OnPlayerNameChanged;
            clientId.OnValueChanged -= OnClientIdChanged;
        }
        
        private void OnPlayerNameChanged(FixedString32Bytes prev, FixedString32Bytes current)
        {
            var str = current.Value.Split('#')[0];
            playerNameText.text = str;
        }

        private void OnClientIdChanged(ulong prev, ulong current)
        {
            name = $"Client-{current}";
        }
    }
}