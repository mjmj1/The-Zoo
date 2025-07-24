using GamePlay;
using Players.Roles;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Players
{
    public class PlayerEntity : NetworkBehaviour
    {
        public enum Role
        {
            None,
            Hider,
            Seeker
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
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                NetworkShow(client.ClientId);

            role.Value = Role.None;
            isDead.Value = false;
            health.Value = 3;
            CameraManager.Instance.EnableCamera(true);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            playerRenderer = GetComponent<PlayerRenderer>();

            clientId.OnValueChanged += OnClientIdChanged;
            playerName.OnValueChanged += OnPlayerNameChanged;

            isDead.OnValueChanged += OnIsDeadChanged;
            role.OnValueChanged += OnRoleChanged;

            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);
            OnIsDeadChanged(false, isDead.Value);

            if (!IsOwner) return;

            health.OnValueChanged += OnHealthChanged;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnNetworkSceneLoadComplete;

            playerName.Value = AuthenticationService.Instance.PlayerName;
            clientId.Value = NetworkManager.LocalClientId;

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

        private void OnNetworkSceneLoadComplete(ulong clientId, string sceneName,
            LoadSceneMode mode)
        {
            if (sceneName != "InGame") return;

            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkSceneLoadComplete;

            PlayManager.Instance.ObserverManager.observerIds.OnListChanged += OnObserverListChanged;
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
                    gameObject.layer = LayerMask.NameToLayer("Default");
                    gameObject.GetComponent<SeekerRole>().enabled = false;
                    gameObject.GetComponent<HiderRole>().enabled = false;
                    playerRenderer.UseOriginShader();
                    break;
            }
        }

        private void OnIsDeadChanged(bool previousValue, bool newValue)
        {
            if (!newValue)
            {
                playerRenderer.UseOriginShader();
                return;
            }

            playerRenderer.UseObserverShader();

            gameObject.layer = LayerMask.NameToLayer("Observer");

            playerRenderer.UseObserverShader();

            PlayManager.Instance.ObserverManager.AddRpc(OwnerClientId);

            playerRenderer.UseObserverShader();
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            print($"client-{OwnerClientId} OnHealthChanged: {newValue}");
        }

        private void OnObserverListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            if (OwnerClientId == changeEvent.Value)
                foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (PlayManager.Instance.ObserverManager.observerIds.Contains(client)) continue;

                    NetworkHide(client);
                }

            if (!isDead.Value) return;
            foreach (var observer in PlayManager.Instance.ObserverManager.observerIds)
                NetworkShow(observer);
        }

        internal void NetworkShow(ulong fromId)
        {
            if (OwnerClientId == fromId) return;

            if (NetworkObject.IsNetworkVisibleTo(fromId)) return;

            NetworkObject.NetworkShow(fromId);
        }

        internal void NetworkHide(ulong fromId)
        {
            if (OwnerClientId == fromId) return;

            if (!NetworkObject.IsNetworkVisibleTo(fromId)) return;

            NetworkObject.NetworkHide(fromId);
        }
    }
}