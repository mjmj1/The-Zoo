using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] Button startButton;
        [SerializeField] TMP_InputField sessionField;
        
        private string _profileName;
        private string _sessionName;
        private int _maxPlayers = 8;
        private ConnectionState _state = ConnectionState.Disconnected;
        private ISession _session;
        private NetworkManager _networkManager;

        private enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
        }

        private async void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            _networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
            await UnityServices.InitializeAsync();
        }

        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (_networkManager.LocalClient.IsSessionOwner)
            {
                Debug.Log($"Client-{_networkManager.LocalClientId} is the session owner!");
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (_networkManager.LocalClientId == clientId)
            {
                Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            }

            if (_networkManager.LocalClient.IsSessionOwner)
            {
                _networkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);    
            }
        }

        private void OnDestroy()
        {
            _session?.LeaveAsync();
        }

        private async Task CreateOrJoinSessionAsync()
        {
            _state = ConnectionState.Connecting;

            try
            {
                AuthenticationService.Instance.SwitchProfile(_profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var options = new SessionOptions() {
                    Name = _sessionName,
                    MaxPlayers = _maxPlayers
                }.WithDistributedAuthorityNetwork();

                _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }

        private void OnStartButtonClicked()
        {
            _profileName = PlayerPrefs.GetString("ProfileName");
            _sessionName = sessionField.text;
            
            _ = CreateOrJoinSessionAsync();
        }
    }
}
