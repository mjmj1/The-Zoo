using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using Utils;
using WebSocketSharp;
using static Networks.ConnectionData;
using static Static.Strings;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour, IConnectionHandler
    {
        private const int MaxPlayers = 8;
        private bool initialLoad;

        public static ConnectionManager instance;

        public ISession CurrentSession { get; private set; }

        private ConnectionState _state = ConnectionState.Disconnected;

        private void Awake()
        {
            try
            {
                if (!instance)
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
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

        async void Start()
        {
            UnityServices.Initialized += OnUnityServicesInitialized;
            await UnityServices.InitializeAsync();

            if (!initialLoad)
            {
                initialLoad = true;
                GameplayEventHandler.LoadLobbyScene();
            }

            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
            NetworkManager.Singleton.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        }

        private async void OnUnityServicesInitialized()
        {
            UnityServices.Initialized -= OnUnityServicesInitialized;
            await SignInAsync();
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnOnClientDisconnectCallback;
                NetworkManager.Singleton.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
            }
        }

        public event Action OnSessionConnect;
        public event Action OnSessionDisconnected;

        // <-------------------Connection------------------->

        private async Task HandleSessionFlowAsync(Func<Task<ISession>> sessionFunc)
        {
            try
            {
                _state = ConnectionState.Connect;

                OnSessionConnect?.Invoke();

                CurrentSession = await sessionFunc.Invoke();

                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                OnSessionDisconnected?.Invoke();
                
                _state = ConnectionState.Disconnected;
                
                Debug.LogError(e);
            }
        }

        private async Task SignInAsync()
        {
            try
            {
                AuthenticationService.Instance.SignInFailed += SignInFailed;
                AuthenticationService.Instance.SwitchProfile(GetRandomString(5));
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SignInFailed(RequestFailedException e)
        {
            AuthenticationService.Instance.SignInFailed -= SignInFailed;
            Debug.LogWarning($"Sign in via Authentication failed: e.ErrorCode {e.ErrorCode}");
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
            if (CurrentSession == null && _state == ConnectionState.Disconnected) return;

            try
            {
                await CurrentSession.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                OnSessionDisconnected?.Invoke();

                _state = ConnectionState.Disconnected;

                CurrentSession = null;
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
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .PlayerSlot(playerSlot)
                .Password(password)
                .IsPrivate(true)
                .BuildCreate();

            await HandleSessionFlowAsync(async () => await MultiplayerService.Instance.CreateSessionAsync(options));

            if (!isPrivate) PublicSessionAsync();
        }

        private async Task QuickSessionAsync(string sessionName)
        {
            var options = new SessionOptionBuilder()
                .Name(sessionName)
                .PlayerSlot(MaxPlayers)
                .Password()
                .IsPrivate(true)
                .BuildCreate();

            var sessionId = $"Session_{GetRandomString(5)}";

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options));

            PublicSessionAsync();
        }

        private async Task JoinSessionByCodeAsync(string code, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options));
        }

        private async Task JoinSessionByIdAsync(string id, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByIdAsync(id, options));
        }

        // <-------------------Host------------------->

        private async Task WithHostSessionAsync(Func<IHostSession, Task> action)
        {
            if (CurrentSession is not { IsHost: true })
                return;

            try
            {
                var host = CurrentSession.AsHost();
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

        private void OnClientStopped(bool obj)
        {
            DisconnectSessionAsync();
        }

        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            Debug.Log($"Client-{NetworkManager.Singleton.LocalClientId} is the session owner!");
        }

        private void OnClientConnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
                Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }

        private void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                print($"Client-{clientId} is disconnected");

                _state = ConnectionState.Disconnected;
            }
        }


        public async Task OnEnterButtonPressed(string playerName)
        {
            if (AuthenticationService.Instance == null)
            {
                return;
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await SignInAsync();
            }

            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }

        private enum ConnectionState
        {
            Disconnected,
            Connect,
            Connected
        }
    }
}