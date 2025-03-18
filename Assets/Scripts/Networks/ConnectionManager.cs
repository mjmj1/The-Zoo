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

        void Start()
        {
            NetworkManager.SceneManager.OnLoadComplete += OnOnLoadComplete;
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

        public async void Connect()
        {
            if (_sessionName.IsNullOrEmpty()) return;
            
            await CreateOrJoinSessionAsync();
        }

        public async void Join()
        {
            if (_sessionCode.IsNullOrEmpty()) return;
            
            await JoinSessionByCodeAsync(_sessionCode);
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
                
                print($"{Session.Id} Joined");
                
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
                
                print($"{Session.Id} Joined");
                
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
        
        public void OnSessionNameInputFieldEndEdit(string arg0)
        {
            _sessionName = arg0;
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
            
            if (NetworkManager.LocalClient.IsSessionOwner && SceneManager.GetActiveScene().name != "Lobby")
            {
                NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            }
        }
        
        void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                Debug.Log($"Client-{clientId} is disconnected");
                
                SceneManager.LoadScene("Title", LoadSceneMode.Single);
            
                Session = null;
            
                _state = ConnectionState.Disconnected;
            }
        }
        
        void OnOnLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            if(!scenename.Equals("Lobby")) return;
            
            switch (_state)
            {
                case ConnectionState.Connected:
                    GameManager.Instance.GetLobbyCanvas().gameObject.SetActive(true);
                    break;
                case ConnectionState.Disconnected:
                    GameManager.Instance.GetTitleCanvas().gameObject.SetActive(true);
                    break;
            }
        }
    }
}
