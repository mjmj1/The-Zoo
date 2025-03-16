using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using static Static.Strings;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour
    {
        public ISession Session {get; private set;}
        public NetworkManager NetworkManager {get; private set;}
        
        private string _sessionName;
        private string _sessionCode;
        private string _playerName;
        
        private int _maxPlayers = 8;
        
        private ConnectionState _state = ConnectionState.Disconnected;

        private enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
        }
        
        private static string GenerateRandomProfileName()
        {
            return "User_" + Guid.NewGuid().ToString("N")[..8];
        }
        
        private static string GenerateRandomSessionId()
        {
            return "Session_" + Guid.NewGuid().ToString("N")[..8];
        }
        
        public static async void SignInAnonymously()
        {
            try
            {
                var profileName = GenerateRandomProfileName();
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
        
        public async void UpdateSessionAsync(string sessionName, int maxPlayers)
        {
            try
            {
                var options = new UpdateLobbyOptions()
                {
                    Name = sessionName,
                    MaxPlayers = maxPlayers
                };
            
                await LobbyService.Instance.UpdateLobbyAsync(Session.Id, options);
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private async Task CreateSessionAsync()
        {
            _state = ConnectionState.Connecting;

            try
            {
                var options = new SessionOptions() {
                    Name = _sessionName,
                    MaxPlayers = _maxPlayers,
                    PlayerProperties = new()
                    {
                        {PLAYERNAME, new PlayerProperty(_playerName)}
                    }
                }.WithDistributedAuthorityNetwork();
                
                Session = await MultiplayerService.Instance.CreateSessionAsync(options);
                
                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }
        
        private async Task JoinSessionByCodeAsync(string code)
        {
            _state = ConnectionState.Connecting;

            try
            {
                var options = new JoinSessionOptions() {
                    PlayerProperties = new()
                    {
                        {PLAYERNAME, new PlayerProperty(_playerName)}
                    }
                };
                
                Session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options);
                
                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
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
                        {PLAYERNAME, new PlayerProperty(_playerName)}
                    }
                }.WithDistributedAuthorityNetwork();
                
                var sessionId = GenerateRandomSessionId();
                
                Session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options);
                
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
                NetworkManager = GetComponent<NetworkManager>();
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
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
                print($"_session.Name: {Session.Name}");
                print($"_session.Id: {Session.Id}");
                print($"_session.Code: {Session.Code}");
                print($"_session.MaxPlayers: {Session.MaxPlayers}");
            }
        }
        
        private async void OnDestroy()
        {
            await Session?.LeaveAsync()!;
            AuthenticationService.Instance.SignOut();
        }

        public async void Connect()
        {
            if (_sessionName.IsNullOrEmpty()) return;
            
            _playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            await CreateOrJoinSessionAsync();
        }

        public async void Join()
        {
            if (_sessionCode.IsNullOrEmpty()) return;
            
            _playerName = PlayerPrefs.GetString(PLAYERNAME);

            await JoinSessionByCodeAsync(_sessionCode);
        }
        
        public void OnSessionNameInputFieldEndEdit(string arg0)
        {
            _sessionName = arg0;
        }
        
        public void OnSessionCodeInputFieldEndEdit(string arg0)
        {
            _sessionCode = arg0;
        }

        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (NetworkManager.LocalClient.IsSessionOwner)
            {
                Debug.Log($"Client-{NetworkManager.LocalClientId} is the session owner!");
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            }

            if (NetworkManager.LocalClient.IsSessionOwner)
            {
                NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            }
        }
    }
}
