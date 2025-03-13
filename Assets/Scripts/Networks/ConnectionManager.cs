using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField sessionNameinputField;
        
        private string _playerName;
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
        
        private static string GenerateRandomProfileName()
        {
            return "User_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
        
        private static string GenerateRandomSessionId()
        {
            return "Session_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
        
        public static async void SignInAnonymously()
        {
            try
            {
                var profileName = GenerateRandomProfileName();
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                print($"{playerInfo}");
                print($"{playerInfo.Id}");
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
        
        public static async Task<IList<ISessionInfo>> QuerySessions()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            return results.Sessions;
        }
        
        private async Task CreateOrJoinSessionAsync()
        {
            _state = ConnectionState.Connecting;

            try
            {
                var options = new SessionOptions() {
                    Name = _sessionName,
                    MaxPlayers = _maxPlayers,
                    PlayerProperties = new()
                    {
                        {"PlayerName", new PlayerProperty(_playerName)}
                    },
                    SessionProperties = new()
                    {
                        {"SessionName", new SessionProperty(_sessionName)}
                    }
                }.WithDistributedAuthorityNetwork();
                
                var sessionId = GenerateRandomSessionId();
                
                _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options);
                
                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }

        private async void Awake()
        {
            try
            {
                _networkManager = GetComponent<NetworkManager>();
                _networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                _networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                print($"_session.Name: {_session.Name}");
                print($"_session.Id: {_session.Id}");
                print($"_session.Code: {_session.Code}");
                print($"_session.Players: {_session.Players}");
            }
        }
        
        private async void OnDestroy()
        {
            await _session?.LeaveAsync()!;
            AuthenticationService.Instance.SignOut();
        }

        public async void Connect()
        {
            _sessionName = sessionNameinputField.text;
            _playerName = PlayerPrefs.GetString("PlayerName");

            await CreateOrJoinSessionAsync();
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
                print(_session.Properties["SessionName"]);
            }
        }
    }
}
