using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.GameSetup;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using Utils;
using static EventHandler.ConnectionEventHandler;
using static Networks.ConnectionData;

namespace Networks
{
    public class ConnectionManager : MonoBehaviour
    {
        private const int MaxPlayers = 8;
        private bool initialLoad;

        public static ConnectionManager Instance { get; private set; }

        public ISession CurrentSession { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private async void OnEnable()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!initialLoad)
                {
                    initialLoad = true;
                    GameManager.Instance.LoadLobbyScene();
                }
            
                NetworkManager.OnDestroying += Destroying;
            
                NetworkManager.Singleton.OnClientStopped += OnClientStopped;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
                NetworkManager.Singleton.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
            }
            catch (Exception e)
            {
                MyLogger.Print(this, e.Message);
            }
        }

        private void Destroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= Destroying;
            
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnOnClientDisconnectCallback;
            NetworkManager.Singleton.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
        }

        // <-------------------Connection------------------->
        private async Task HandleSessionFlowAsync(Func<Task<ISession>> sessionFunc)
        {
            try
            {
                SessionConnectStart();

                CurrentSession = await sessionFunc.Invoke();

                SessionConnected();
            }
            catch (Exception e)
            {
                SessionDisconnected();
                Debug.LogError(e);
            }
        }

        private async Task SignInAsync()
        {
            try
            {
                AuthenticationService.Instance.SignInFailed += SignInFailed;
                AuthenticationService.Instance.SwitchProfile(Util.GetRandomString(8));
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
                SessionDisconnected();
                
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
                .PlayerProperty(Util.PLAYERNAME, AuthenticationService.Instance.PlayerName.Split('#')[0])
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
                .PlayerProperty(Util.PLAYERNAME, AuthenticationService.Instance.PlayerName.Split('#')[0])
                .BuildCreate();

            var sessionId = $"Session_{Util.GetRandomString(5)}";

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, options));

            PublicSessionAsync();
        }

        private async Task JoinSessionByCodeAsync(string code, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(Util.PLAYERNAME, AuthenticationService.Instance.PlayerName.Split('#')[0])
                .BuildJoin();

            await HandleSessionFlowAsync(async () =>
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options));
        }

        private async Task JoinSessionByIdAsync(string id, string password = null)
        {
            var options = new SessionOptionBuilder()
                .Password(password)
                .PlayerProperty(Util.PLAYERNAME, AuthenticationService.Instance.PlayerName.Split('#')[0])
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

        public async void UpdateSessionAsync(
            GameOptionField<string> sessionName, GameOptionField<string> password,
            GameOptionField<bool> isPrivate, GameOptionField<int> playerSlot)
        {
            try
            {
                await WithHostSessionAsync(async host =>
                {
                    if (sessionName.IsDirty) host.Name = sessionName.Current;

                    if (password.IsDirty)
                    {
                        host.Password = password.Current;
                        host.SetProperty(Util.PASSWORD,
                            new SessionProperty(password.Current, VisibilityPropertyOptions.Private));
                    }

                    if (isPrivate.IsDirty) host.IsPrivate = isPrivate.Current;

                    if (playerSlot.IsDirty)
                        host.SetProperty(Util.PLAYERSLOT, new SessionProperty(playerSlot.Current.ToString()));

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
                await WithHostSessionAsync(async host =>
                {
                    host.Host = newHost;
                    await host.SavePropertiesAsync();
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
            }
        }

        public async Task Login(string playerName)
        {
            if (AuthenticationService.Instance == null)
            {
                print("AuthenticationService.Instance == null");
                return;
            }

            if (!AuthenticationService.Instance.IsSignedIn) await SignInAsync();

            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            
            print($"logged in. {playerName}");
        }
    }
}