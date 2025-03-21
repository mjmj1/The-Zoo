using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject titleMenu;
        [SerializeField] GameObject lobbyMenu;
        
        static InformationPopup _informationPopup;

        void Awake()
        {
            _informationPopup = GetComponent<InformationPopup>();
            titleMenu.SetActive(true);
            lobbyMenu.SetActive(false);
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
                titleMenu.SetActive(false);
                lobbyMenu.SetActive(true);
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
