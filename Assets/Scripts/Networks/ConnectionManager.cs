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
        private const int MaxPlayers = 10;

        private ConnectionState _state = ConnectionState.Disconnected;

        public ISession ActiveSession { get; private set; }
        private NetworkManager NetworkManager { get; set; }

        private async void Awake()
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

        public event Action OnSessionConnecting;
        public event Action OnSessionDisconnected;

        // <-------------------Connection------------------->
        private async Task HandleSessionFlowAsync(Func<Task<ISession>> sessionFunc)
        {
            try
            {
                _state = ConnectionState.Connecting;

                OnSessionConnecting?.Invoke();

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
                print(data.ToString());
                switch (data.Type)
                {
                    case ConnectionType.Quick:
                        await CreateOrJoinSessionAsync(data.PlayerName, data.SessionName);
                        break;
                    case ConnectionType.Create:
                        await CreateSessionAsync(data.PlayerName, data.SessionName, data.Password);
                        break;
                    case ConnectionType.JoinById:
                        await JoinSessionByIdAsync(data.Code, data.PlayerName);
                        break;
                    case ConnectionType.JoinByCode:
                        await JoinSessionByCodeAsync(data.Code, data.PlayerName);
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

        private async Task CreateSessionAsync(string playerName, string sessionName = null, string password = null,
            int playerSlot = MaxPlayers)
        {
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .MaxPlayers(MaxPlayers)
                .Password(password)
                .IsPrivate(true)
                .PlayerProperty(PLAYERNAME, new PlayerProperty(playerName))
                .BuildCreate();

            await HandleSessionFlowAsync(async () => await MultiplayerService.Instance.CreateSessionAsync(options));

            PublicSessionAsync();
        }

        private async Task CreateOrJoinSessionAsync(string playerName, string sessionName)
        {
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .MaxPlayers(MaxPlayers)
                .Password()
                .IsPrivate(true)
                .PlayerProperty(PLAYERNAME, new PlayerProperty(playerName))
                .BuildCreate();

            var sessionId = GenerateRandomSessionId();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options));

            PublicSessionAsync();
        }

        private async Task JoinSessionByCodeAsync(string code, string playerName, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(PLAYERNAME, new PlayerProperty(playerName))
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options));
        }

        private async Task JoinSessionByIdAsync(string id, string playerName, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(PLAYERNAME, new PlayerProperty(playerName))
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

        public async void UpdateSessionAsync(string sessionName = null, string password = null, bool? isPrivate = null,
            int? playerSlot = null)
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    var changed = false;

                    if (!sessionName.IsNullOrEmpty())
                    {
                        print($"name changed");
                        host.Name = sessionName;
                        changed = true;
                    }
                    
                    if (password != null)
                    {
                        print($"password changed");
                        host.Password = password;
                        
                        host.SetProperty(PASSWORD, new SessionProperty(password, VisibilityPropertyOptions.Private));

                        changed = true;
                    }
                    
                    if (isPrivate != null)
                    {
                        print($"private changed");
                        host.IsPrivate = (bool)isPrivate;
                        changed = true;
                    }
                    
                    if (changed)
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

        private void OnClientConnectedCallback(ulong clientId)
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
            Connecting,
            Connected
        }
    }
}