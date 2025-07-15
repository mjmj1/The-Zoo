using System;
using EventHandler;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using Utils;

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

        public static UIManager Instance;

        private void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            popupCanvas.gameObject.SetActive(true);

            SwitchUI(UIType.Title);
            
            ConnectionEventHandler.OnSessionConnectStart += OnSessionConnectStart;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            
            UnityServices.Initialized += UnityServicesOnInitialized;
            
            NetworkManager.OnDestroying += Destroying;
        }

        private void UnityServicesOnInitialized()
        {
            UnityServices.Initialized -= UnityServicesOnInitialized;
            
            print("Unity Services initialized.");
            
            if(AuthenticationService.Instance == null) print("AuthenticationService.Instance == null");
            
            AuthenticationService.Instance.SignedIn += OnSignedIn;
        }

        private void Destroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= Destroying;
            
            ConnectionEventHandler.OnSessionConnectStart -= OnSessionConnectStart;
            
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        internal void SwitchUI(UIType uiType)
        {
            titleCanvas.gameObject.SetActive(uiType == UIType.Title);
            mainCanvas.gameObject.SetActive(uiType == UIType.Main);
            lobbyCanvas.gameObject.SetActive(uiType == UIType.Lobby);
            backgroundCanvas.gameObject.SetActive(uiType != UIType.Lobby);
            loadingCanvas.gameObject.SetActive(false);
        }
        
        private void OnSignedIn()
        {
            print("Signed In.");
            
            SwitchUI(UIType.Main);
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