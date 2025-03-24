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

        public void Start()
        {
            GameManager.Instance.connectionManager.OnSessionStarted += OnSessionStarted;
            GameManager.Instance.connectionManager.NetworkManager.OnClientConnectedCallback +=
                OnClientConnectedCallback;
            GameManager.Instance.connectionManager.NetworkManager.OnClientDisconnectCallback +=
                OnOnClientDisconnectCallback;
        }

        public void OnSessionStarted()
        {
            loadingScreen.gameObject.SetActive(true);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClientId == clientId)
            {
                titleMenu.SetActive(false);
                lobbyMenu.SetActive(true);

                loadingScreen.gameObject.SetActive(false);
            }
        }

        private void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClientId == clientId)
            {
                titleMenu.SetActive(true);
                lobbyMenu.SetActive(false);
            }
        }

        public static void OpenInformationPopup(string massage)
        {
            _informationPopup.GetInformationPopup(massage);
        }
    }
}