using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject titleMenu;
        [SerializeField] GameObject lobbyMenu;
        [SerializeField] GameObject loadingScreen;
        
        static InformationPopup _informationPopup;

        void Awake()
        {
            _informationPopup = GetComponent<InformationPopup>();
            titleMenu.SetActive(true);
            lobbyMenu.SetActive(false);
            loadingScreen.SetActive(false);
        }

        void Start()
        {
            GameManager.Instance.connectionManager.OnSessionStarted += OnSessionStarted;
            GameManager.Instance.connectionManager.NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            GameManager.Instance.connectionManager.NetworkManager.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
        }

        void OnSessionStarted()
        {
            loadingScreen.gameObject.SetActive(true);
        }

        void OnClientConnectedCallback(ulong clientId)
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClientId == clientId)
            {
                titleMenu.SetActive(false);
                lobbyMenu.SetActive(true);
                
                loadingScreen.gameObject.SetActive(false);
            }
        }
        
        void OnOnClientDisconnectCallback(ulong clientId)
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
