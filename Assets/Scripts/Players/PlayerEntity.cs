using GamePlay;
using Players.Roles;
using Scriptable;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

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

        [SerializeField] internal RoleColor roleColor;
        [SerializeField] internal TMP_Text playerNameText;
        [SerializeField] internal SpriteRenderer playerMarker;

        public NetworkVariable<ulong> clientId = new();
        public NetworkVariable<FixedString32Bytes> playerName = new();

        public NetworkVariable<Role> role = new();
        public NetworkVariable<bool> isDead = new();
        public NetworkVariable<int> animalIndex = new();

        private PlayerRenderer playerRenderer;

        internal bool isSpinHold;

        public void Reset()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                NetworkShow(client.ClientId);

            role.Value = Role.None;
            isDead.Value = false;
            playerMarker.color = roleColor.defaultColor;

            CameraManager.Instance.EnableCamera(true);
        }

        public override void OnNetworkSpawn()
        {
            playerRenderer = GetComponent<PlayerRenderer>();

            clientId.OnValueChanged += OnClientIdChanged;
            playerName.OnValueChanged += OnPlayerNameChanged;
            role.OnValueChanged += OnRoleChanged;
            isDead.OnValueChanged += OnIsDeadChanged;

            OnPlayerNameChanged("", playerName.Value);
            OnClientIdChanged(0, clientId.Value);
            OnIsDeadChanged(false, isDead.Value);

            playerMarker.gameObject.SetActive(false);

            if (!IsOwner) return;

            playerMarker.gameObject.SetActive(true);

            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnNetworkSceneLoadComplete;

            playerName.Value = AuthenticationService.Instance.PlayerName;
            clientId.Value = NetworkManager.LocalClientId;

            gameObject.AddComponent<AudioListener>();

            CameraManager.Instance.EnableCamera(true);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            playerName.OnValueChanged -= OnPlayerNameChanged;
            clientId.OnValueChanged -= OnClientIdChanged;
            role.OnValueChanged -= OnRoleChanged;
            isDead.OnValueChanged -= OnIsDeadChanged;

            if (!IsOwner) return;

            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkSceneLoadComplete;

            CameraManager.Instance.EnableCamera(false);
        }

        private void OnNetworkSceneLoadComplete(ulong id, string sceneName, LoadSceneMode mode)
        {
            if (!NetworkManager.Singleton.LocalClientId.Equals(id)) return;
            
            switch (sceneName)
            {
                case "Lobby":
                {
                    MyLogger.Print(this, "옵저버 리스트 구독 해체");
                    if (!PlayManager.Instance) return;

                    PlayManager.Instance.observerManager.observerIds.OnListChanged -= OnObserverListChanged;
                    break;
                }
                case "InGame":
                {
                    MyLogger.Print(this, "옵저버 리스트 구독");
                    if (!PlayManager.Instance) return;

                    PlayManager.Instance.observerManager.observerIds.OnListChanged += OnObserverListChanged;
                    break;
                }
            }
        }

        internal void AlignForward()
        {
            var forward = Vector3.Cross(
                CameraManager.Instance.Orbit.transform.right,
                transform.up).normalized;

            transform.rotation = Quaternion.LookRotation(forward, transform.up);

            CameraManager.Instance.LookMove();
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

            if (!IsOwner) return;

            if(role.Value == Role.Hider)
                PlayManager.Instance.roleManager.RemoveHiderRpc(OwnerClientId);
            else if(role.Value == Role.Seeker)
                PlayManager.Instance.roleManager.RemoveSeekerRpc(OwnerClientId);

            PlayManager.Instance.observerManager.AddRpc(OwnerClientId);
        }

        private void OnObserverListChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<ulong>.EventType.Add) return;

            if (OwnerClientId == changeEvent.Value)
                foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (PlayManager.Instance.observerManager.observerIds.Contains(client)) continue;

                    NetworkHide(client);
                }

            if (!isDead.Value) return;

            foreach (var observer in PlayManager.Instance.observerManager.observerIds)
                NetworkShow(observer);
        }

        private void NetworkShow(ulong fromId)
        {
            if (OwnerClientId == fromId) return;

            if (NetworkObject.IsNetworkVisibleTo(fromId)) return;

            NetworkObject.NetworkShow(fromId);
        }

        private void NetworkHide(ulong fromId)
        {
            if (OwnerClientId == fromId) return;

            if (!NetworkObject.IsNetworkVisibleTo(fromId)) return;

            NetworkObject.NetworkHide(fromId);
        }
    }
}