using Networks;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static InformationPopup _informationPopup;

        [SerializeField] private GameObject titleMenu;
        [SerializeField] private GameObject lobbyMenu;
        [SerializeField] private GameObject loadingScreen;

        private void Awake()
        {
            _informationPopup = GetComponent<InformationPopup>();
            titleMenu.SetActive(true);
            lobbyMenu.SetActive(false);
            loadingScreen.SetActive(false);
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            
            ConnectionManager.Instance.OnSessionConnect += OnSessionConnect;
            ConnectionManager.Instance.OnSessionDisconnected += OnSessionDisconnected;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }

            ConnectionManager.Instance.OnSessionConnect -= OnSessionConnect;
            ConnectionManager.Instance.OnSessionDisconnected -= OnSessionDisconnected;

        }

        public static void OpenInformationPopup(string massage)
        {
            _informationPopup.GetInformationPopup(massage);
        }

        private void SetupUI(bool isConnected)
        {
            titleMenu.SetActive(!isConnected);
            lobbyMenu.SetActive(isConnected);

            loadingScreen.gameObject.SetActive(false);
        }

        private void OnSessionConnect()
        {
            loadingScreen.gameObject.SetActive(true);
        }

        private void OnSessionDisconnected()
        {
            loadingScreen.gameObject.SetActive(false);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SetupUI(true);
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SetupUI(false);
        }
    }
}