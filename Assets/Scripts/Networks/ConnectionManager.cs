using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using WebSocketSharp;
using static Networks.ConnectionData;
using static Static.Strings;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour, IConnectionHandler
    {
        private const int MaxPlayers = 8;

        public static ConnectionManager Instance;

        private ConnectionState _state = ConnectionState.Disconnected;

        public ISession ActiveSession { get; private set; }
        private NetworkManager NetworkManager { get; set; }

        private async void Awake()
        {
            try
            {
                if (!Instance)
                {
                    Instance = this;
                    DontDestroyOnLoad(gameObject);
                    NetworkManager = GetComponent<NetworkManager>();
                
                    RegisterNetworkEvents();
                
                    await UnityServices.InitializeAsync();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        // test
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                print($"session.Code: {ActiveSession.Code}");
                print($"session.MaxPlayers: {ActiveSession.MaxPlayers}");
                print($"session.Name: {ActiveSession.Name}");
                print($"session.IsPrivate: {ActiveSession.IsPrivate}");
            }
        }

        private async void OnDestroy()
        {
            try
            {
                await ActiveSession.LeaveAsync();
                AuthenticationService.Instance.SignOut();
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public event Action OnSessionConnect;
        public event Action OnSessionDisconnected;

        private void RegisterNetworkEvents()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectCallback;
            NetworkManager.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
            NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

        }
        
        // <-------------------Connection------------------->
        private async Task HandleSessionFlowAsync(Func<Task<ISession>> sessionFunc)
        {
            try
            {
                _state = ConnectionState.Connect;

                OnSessionConnect?.Invoke();

                ActiveSession = await sessionFunc.Invoke();

                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                OnSessionDisconnected?.Invoke();
                
                _state = ConnectionState.Disconnected;
                
                print(e.Message);
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

        public async void ConnectAsync(ConnectionData data)
        {
            try
            {
                switch (data.Type)
                {
                    case ConnectionType.Quick:
                        await QuickSessionAsync(data.SessionName);
                        break;
                    case ConnectionType.Create:
                        await CreateSessionAsync(data.SessionName, data.Password, data.IsPrivate, data.PlayerSlot);
                        break;
                    case ConnectionType.JoinById:
                        await JoinSessionByIdAsync(data.Code);
                        break;
                    case ConnectionType.JoinByCode:
                        await JoinSessionByCodeAsync(data.Code);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                await ActiveSession.LeaveAsync();

                ActiveSession = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                OnSessionDisconnected?.Invoke();

                _state = ConnectionState.Disconnected;
            }
        }

        public async Task<IList<ISessionInfo>> QuerySessionsAsync()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            return results.Sessions;
        }

        private async Task CreateSessionAsync(string sessionName = null, string password = null,
            bool isPrivate = false, int playerSlot = MaxPlayers)
        {
            var playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .PlayerSlot(playerSlot)
                .Password(password)
                .IsPrivate(true)
                .PlayerProperty(PLAYERNAME, playerName)
                .BuildCreate();

            await HandleSessionFlowAsync(async () => await MultiplayerService.Instance.CreateSessionAsync(options));

            if (!isPrivate) PublicSessionAsync();
        }

        private async Task QuickSessionAsync(string sessionName)
        {
            var playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .PlayerSlot(MaxPlayers)
                .Password()
                .IsPrivate(true)
                .PlayerProperty(PLAYERNAME, playerName)
                .BuildCreate();

            var sessionId = GenerateRandomSessionId();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options));

            PublicSessionAsync();
        }

        private async Task JoinSessionByCodeAsync(string code, string password = null)
        {
            var playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(PLAYERNAME, playerName)
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options));
        }

        private async Task JoinSessionByIdAsync(string id, string password = null)
        {
            var playerName = PlayerPrefs.GetString(PLAYERNAME);
            
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(PLAYERNAME, playerName)
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByIdAsync(id, options));
        }

        // <-------------------Host------------------->

        private async Task WithHostSessionAsync(Func<IHostSession, Task> action)
        {
            if (ActiveSession is not { IsHost: true })
                return;

            try
            {
                var host = ActiveSession.AsHost();
                await action.Invoke(host);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async void UpdateSessionAsync(string sessionName = "", string password = "", bool? isPrivate = null,
            int playerSlot = -1)
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    if (!sessionName.IsNullOrEmpty())
                    {
                        print($"sessionName {sessionName}");
                        host.Name = sessionName;
                    }

                    if (!password.IsNullOrEmpty())
                    {
                        print($"password {password}");
                        host.Password = password;
                        host.SetProperty(PASSWORD, new SessionProperty(password, VisibilityPropertyOptions.Private));
                    }

                    if (isPrivate != null)
                    {
                        print($"isPrivate {isPrivate}");
                        host.IsPrivate = isPrivate.Value;
                    }

                    if (playerSlot != -1)
                    {
                        print($"playerSlot {playerSlot}");
                        host.SetProperty(PLAYERSLOT, new SessionProperty(playerSlot.ToString()));
                    }

                    await host.SavePropertiesAsync();
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public async void ChangeHostAsync(string newHost)
        {
            try
            {
                await WithHostSessionAsync(_ =>
                {
                    print("Change Host Function is not Developed");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public async void KickPlayerAsync(string playerId)
        {
            try
            {
                await WithHostSessionAsync(async host => { await host.RemovePlayerAsync(playerId); });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public async void LockSessionAsync()
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    host.IsLocked = true;
                    await host.SavePropertiesAsync();
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public async void UnlockSessionAsync()
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    host.IsLocked = false;
                    await host.SavePropertiesAsync();
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private async void PrivateSessionAsync()
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    host.IsPrivate = true;
                    await host.SavePropertiesAsync();
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private async void PublicSessionAsync()
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    host.IsPrivate = false;
                    await host.SavePropertiesAsync();
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }


        // <-------------------Event------------------->
        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (NetworkManager.LocalClient.IsSessionOwner)
                Debug.Log($"Client-{NetworkManager.LocalClientId} is the session owner!");
        }

        private void OnClientConnectCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
                Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }

        private void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                print($"Client-{clientId} is disconnected");

                _state = ConnectionState.Disconnected;
            }
        }

        private enum ConnectionState
        {
            Disconnected,
            Connect,
            Connected
        }
    }
}