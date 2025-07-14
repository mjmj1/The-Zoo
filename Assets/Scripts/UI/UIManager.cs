using EventHandler;
using Networks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Canvas lobbyCanvas;
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Canvas popupCanvas;

        private void Awake()
        {
            popupCanvas.gameObject.SetActive(true);

            OnMain();
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            
            ConnectionEventHandler.OnSessionConnectStart += OnSessionConnectStart;
            ConnectionEventHandler.OnSessionDisconnected += OnSessionDisconnected;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            
            ConnectionEventHandler.OnSessionConnectStart -= OnSessionConnectStart;
            ConnectionEventHandler.OnSessionDisconnected -= OnSessionDisconnected;

        }

        private void OnMain()
        {
            mainCanvas.gameObject.SetActive(true);
            lobbyCanvas.gameObject.SetActive(false);
            loadingCanvas.gameObject.SetActive(false);
        }
        
        private void OnLobby()
        {
            mainCanvas.gameObject.SetActive(false);
            lobbyCanvas.gameObject.SetActive(true);
            loadingCanvas.gameObject.SetActive(false);
        }

        private void OnSessionConnectStart()
        {
            loadingCanvas.gameObject.SetActive(true);
        }

        private void OnSessionDisconnected()
        {
            loadingCanvas.gameObject.SetActive(false);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            OnLobby();
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            OnMain();
        }
    }
}