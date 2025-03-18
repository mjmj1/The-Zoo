using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using UnityEngine;
using WebSocketSharp;
using static Static.Strings;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour
    {
        public ISession Session {get; private set;}
        public NetworkManager NetworkManager {get; private set;}

        string _sessionName;
        string _sessionCode;
        string _playerName;
        
        int _maxPlayers = 8;
        
        ConnectionState _state = ConnectionState.Disconnected;

        private enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
        }

        async void Awake()
        {
            try
            {
                NetworkManager = GetComponent<NetworkManager>();
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
                NetworkManager.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
                
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                print($"_session.Name: {Session.Name}");
                print($"_session.Id: {Session.Id}");
                print($"_session.Code: {Session.Code}");
                print($"_session.MaxPlayers: {Session.MaxPlayers}");
                print($"Player Name: {Session.CurrentPlayer.Properties[PLAYERNAME].Value}");
            }
        }
        
        async void OnDestroy()
        {
            try
            {
                await Session?.LeaveAsync();
                AuthenticationService.Instance.SignOut();
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
        
        public async void ConnectAsync()
        {
            try
            {
                _sessionName = GetRandomSessionName();
            
                await CreateOrJoinSessionAsync();
                
                Session.PlayerJoined += PlayerJoined;
                Session.PlayerLeaving += PlayerLeaving;
                Session.PlayerHasLeft += PlayerHasLeft;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        void PlayerJoined(string id)
        {
            print($"PlayerJoined: {id}");
        }
        
        void PlayerLeaving(string id)
        {
            print($"PlayerLeaving: {id}");
        }
        
        void PlayerHasLeft(string id)
        {
            print($"PlayerHasLeft: {id}");
        }

        public async void JoinAsync()
        {
            try
            {
                if (_sessionCode.IsNullOrEmpty()) return;
            
                await JoinSessionByCodeAsync(_sessionCode);
                
                Session.PlayerJoined += PlayerJoined;
                Session.PlayerLeaving += PlayerLeaving;
                Session.PlayerHasLeft += PlayerHasLeft;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public async void SignInAnonymouslyAsync()
        {
            try
            {
                var profileName = GenerateRandomProfileName();
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public async Task<IList<ISessionInfo>> QuerySessionsAsync()
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
                Debug.LogException(e);
            }
        }

        public async void DisconnectSessionAsync()
        {
            try
            {
                await Session.LeaveAsync();
                
                _state = ConnectionState.Disconnected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }
        
        async Task CreateSessionAsync()
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
        
        async Task JoinSessionByCodeAsync(string code)
        {
            _playerName = PlayerPrefs.GetString(PLAYERNAME);
            
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
        
        public async Task JoinSessionByIdAsync(string id)
        {
            _playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            _state = ConnectionState.Connecting;

            try
            {
                var options = new JoinSessionOptions() {
                    PlayerProperties = new()
                    {
                        {PLAYERNAME, new PlayerProperty(_playerName)}
                    }
                };
                
                Session = await MultiplayerService.Instance.JoinSessionByIdAsync(id, options);
                
                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }
        
        async Task CreateOrJoinSessionAsync()
        {
            _playerName = PlayerPrefs.GetString(PLAYERNAME);
            
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
        
        public void OnSessionCodeInputFieldEndEdit(string arg0)
        {
            _sessionCode = arg0;
        }

        void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (NetworkManager.LocalClient.IsSessionOwner)
            {
                Debug.Log($"Client-{NetworkManager.LocalClientId} is the session owner!");
            }
        }

        void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            }
        }
        
        void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                print($"Client-{clientId} is disconnected");
            
                Session = null;
            
                _state = ConnectionState.Disconnected;
            }
        }
    }
}
