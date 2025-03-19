using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject TitleMenu;
        [SerializeField] GameObject LobbyMenu;
        
        static InformationPopup _informationPopup;

        void Awake()
        {
            _informationPopup = GetComponent<InformationPopup>();
            TitleMenu.SetActive(true);
            LobbyMenu.SetActive(false);
        }

        void Start()
        {
            GameManager.Instance.connectionManager.NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            GameManager.Instance.connectionManager.NetworkManager.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
        }
        
        void OnClientConnectedCallback(ulong clientId)
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClientId == clientId)
            {
                TitleMenu.SetActive(false);
                LobbyMenu.SetActive(true);
            }
        }
        
        void OnOnClientDisconnectCallback(ulong clientId)
        {
            if (GameManager.Instance.connectionManager.NetworkManager.LocalClientId == clientId)
            {
                TitleMenu.SetActive(true);
                LobbyMenu.SetActive(false);
            }
        }

        public static void OpenInformationPopup(string massage)
        {
            _informationPopup.GetInformationPopup(massage);
        }
    }
}
