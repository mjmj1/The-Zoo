using Static;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Utils;
using static Static.Strings;

namespace Characters
{
    public class PlayerEntity : NetworkBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;

        public NetworkVariable<FixedString32Bytes> playerName = new();
        public NetworkVariable<ulong> clientId = new();
        
        private CharacterNetworkAnimator _networkAnimator;

        private void Awake()
        {
            _networkAnimator = GetComponent<CharacterNetworkAnimator>();
        }

        public override void OnNetworkSpawn()
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
            clientId.OnValueChanged += OnClientIdChanged;
            
            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);
            
            if (!IsOwner) return;

            playerName.Value = Manage.Session().CurrentPlayer.Properties[PLAYERNAME].Value;
            clientId.Value = NetworkManager.LocalClientId;
        }

        public override void OnNetworkDespawn()
        {
            playerName.OnValueChanged -= OnPlayerNameChanged;
            clientId.OnValueChanged -= OnClientIdChanged;
            
            base.OnNetworkDespawn();
        }

        private void OnPlayerNameChanged(FixedString32Bytes prev, FixedString32Bytes current)
        {
            playerNameText.text = current.ToString();
        }
        
        private void OnClientIdChanged(ulong prev, ulong current)
        {
            name = $"Client-{current}";
        }

        public void SetTrigger(int id)
        {
            _networkAnimator.SetTrigger(id);
        }

        public void ResetTrigger(int id)
        {
            _networkAnimator.ResetTrigger(id);
        }

        public void SetBool(int id, bool value)
        {
            _networkAnimator.SetBool(id, value);
        }

        public void SetFloat(int id, float value)
        {
            _networkAnimator.SetFloat(id, value);
        }
    }
}