using System;
using Characters.Roles;
using GamePlay;
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
            Observer,
        }

        [SerializeField] private TMP_Text playerNameText;

        public NetworkVariable<ulong> clientId = new();
        public NetworkVariable<FixedString32Bytes> playerName = new();

        public NetworkVariable<Role> role = new();
        public NetworkVariable<int> health = new(3);
        public NetworkVariable<bool> isDead = new();

        private PlayerRenderer playerRenderer;

        public void Reset()
        {
            NetworkShow();
            role.Value = Role.None;
            isDead.Value = false;
            health.Value = 3;
            CameraManager.Instance.EnableCamera(true);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            clientId.OnValueChanged += OnClientIdChanged;
            playerName.OnValueChanged += OnPlayerNameChanged;

            role.OnValueChanged += OnRoleChanged;
            isDead.OnValueChanged += OnIsDeadChanged;
            health.OnValueChanged += OnHealthChanged;

            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);

            if (!IsOwner) return;

            playerName.Value = AuthenticationService.Instance.PlayerName;
            clientId.Value = NetworkManager.LocalClientId;

            playerRenderer = GetComponent<PlayerRenderer>();

            CameraManager.Instance.EnableCamera(true);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            playerName.OnValueChanged -= OnPlayerNameChanged;
            clientId.OnValueChanged -= OnClientIdChanged;
            role.OnValueChanged -= OnRoleChanged;
            isDead.OnValueChanged -= OnIsDeadChanged;
            health.OnValueChanged -= OnHealthChanged;
        }

        public void Damaged()
        {
            health.Value -= 1;
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
                    gameObject.layer = LayerMask.NameToLayer("Observer");
                    gameObject.GetComponent<SeekerRole>().enabled = false;
                    gameObject.GetComponent<HiderRole>().enabled = false;
                    playerRenderer.UseOrigin();
                    break;
            }
        }

        private void OnIsDeadChanged(bool previousValue, bool newValue)
        {
            if (!newValue) return;

            if (!IsOwner) return;

            playerRenderer.UseGhost();

            NetworkHide();
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            if (!IsOwner) return;

            print($"client-{OwnerClientId} OnHealthChanged: {newValue}");
        }

        private void NetworkHide()
        {
            var netObj = GetComponent<NetworkObject>();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId == netObj.OwnerClientId) continue;
                netObj.NetworkHide(client.ClientId);
            }


        }

        private void NetworkShow()
        {
            var netObj = GetComponent<NetworkObject>();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId == netObj.OwnerClientId) continue;
                netObj.NetworkShow(client.ClientId);
            }
        }
    }
}