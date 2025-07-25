using System.Collections;
using Characters.Roles;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace Players
{
    public class PlayerEntity : NetworkBehaviour
    {
        public enum Role
        {
            None,
            Hider,
            Seeker,
        }

        [SerializeField] private TMP_Text playerNameText;

        public NetworkVariable<FixedString32Bytes> playerName = new();
        public NetworkVariable<int> health = new(3);
        public NetworkVariable<ulong> clientId = new();

        public NetworkVariable<Role> role = new();
        [SerializeField] private ParticleSystem hitEffectPrefab;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            clientId.OnValueChanged += OnClientIdChanged;
            role.OnValueChanged += OnRoleChanged;
            playerName.OnValueChanged += OnPlayerNameChanged;
            health.OnValueChanged += OnHealthChanged;

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

        public void Damaged()
        {
            health.Value -= 1;
            hitEffectPrefab.Play();
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

        private void OnRoleChanged(Role previousValue, Role newValue)
        {
            switch (newValue)
            {
                case Role.Hider:
                    gameObject.layer = LayerMask.NameToLayer("Hider");
                    gameObject.GetComponent<SeekerRole>().enabled = false;
                    gameObject.GetComponent<HiderRole>().enabled = true;
                    
                    break;
                case Role.Seeker:
                    gameObject.layer = LayerMask.NameToLayer("Seeker");
                    gameObject.GetComponent<SeekerRole>().enabled = true;
                    gameObject.GetComponent<HiderRole>().enabled = false;
                    break;
                case Role.None:
                    gameObject.layer = LayerMask.NameToLayer("Default");
                    gameObject.GetComponent<SeekerRole>().enabled = false;
                    gameObject.GetComponent<HiderRole>().enabled = false;

                    break;
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            if (!IsOwner) return;

            print($"client-{OwnerClientId} OnHealthChanged: {newValue}");
            if (newValue != 0) return;
        }
    }
}