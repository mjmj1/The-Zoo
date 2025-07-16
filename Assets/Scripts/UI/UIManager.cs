using EventHandler;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas backgroundCanvas;
        [SerializeField] private Canvas titleCanvas;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Canvas lobbyCanvas;
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Canvas popupCanvas;

        private void OnEnable()
        {
            popupCanvas.gameObject.SetActive(true);

            SwitchUI(UIType.Title);

            GamePlayEventHandler.OnPlayerLogin += OnPlayerLogin;
            ConnectionEventHandler.OnSessionConnectStart += OnSessionConnectStart;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

            UnityServices.Initialized += UnityServicesOnInitialized;

            NetworkManager.OnDestroying += OnDestroying;
        }

        private void UnityServicesOnInitialized()
        {
            UnityServices.Initialized -= UnityServicesOnInitialized;

            AuthenticationService.Instance.SignedIn += OnSignedIn;
        }

        private void OnDestroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= OnDestroying;

            ConnectionEventHandler.OnSessionConnectStart -= OnSessionConnectStart;

            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        private void SwitchUI(UIType uiType)
        {
            titleCanvas.gameObject.SetActive(uiType == UIType.Title);
            mainCanvas.gameObject.SetActive(uiType == UIType.Main);
            lobbyCanvas.gameObject.SetActive(uiType == UIType.Lobby);
            backgroundCanvas.gameObject.SetActive(uiType != UIType.Lobby);
            loadingCanvas.gameObject.SetActive(false);
        }

        private void OnSignedIn()
        {
            SwitchUI(UIType.Main);
        }
        
        private void OnPlayerLogin()
        {
            loadingCanvas.gameObject.SetActive(true);
        }
        
        private void OnSessionConnectStart()
        {
            loadingCanvas.gameObject.SetActive(true);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SwitchUI(UIType.Lobby);
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SwitchUI(UIType.Main);
        }
    }

    internal enum UIType
    {
        Title,
        Main,
        Lobby
    }
}