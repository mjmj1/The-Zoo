using TMPro;
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

        private CharacterNetworkAnimator networkAnimator;

        private void Awake()
        {
            networkAnimator = GetComponent<CharacterNetworkAnimator>();
        }

        public override void OnNetworkSpawn()
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
            clientId.OnValueChanged += OnClientIdChanged;

            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);

            if (IsOwner)
            {
                playerName.Value = AuthenticationService.Instance.PlayerName;
                clientId.Value = NetworkManager.LocalClientId;
            }

            base.OnNetworkSpawn();
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

        public void SetTrigger(int id)
        {
            networkAnimator.SetTrigger(id);
        }

        public void ResetTrigger(int id)
        {
            networkAnimator.ResetTrigger(id);
        }

        public void SetBool(int id, bool value)
        {
            networkAnimator.SetBool(id, value);
        }

        public void SetFloat(int id, float value)
        {
            networkAnimator.SetFloat(id, value);
        }
    }
}