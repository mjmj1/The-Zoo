using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static InformationPopup _informationPopup;
        
        [SerializeField] private GameObject titleMenu;
        [SerializeField] private GameObject lobbyMenu;
        [SerializeField] private GameObject loadingScreen;
        
        public static TitleUIManager TitleUIManager;
        public static LobbyUIManager LobbyUIManager;
        
        public static void OpenInformationPopup(string massage)
        {
            _informationPopup.GetInformationPopup(massage);
        }

        public static bool IsCursorLocked()
        {
            return Cursor.lockState != CursorLockMode.Locked;
        }
        
        public static void HandleMouseLock()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                if (!IsPointerOverUI())
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        
        private static bool IsPointerOverUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }
        
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
            
            TitleUIManager = titleMenu.GetComponent<TitleUIManager>();
            LobbyUIManager = lobbyMenu.GetComponent<LobbyUIManager>();
        }

        public void Update()
        {
            HandleMouseLock();
        }

        private void OnSessionStarted()
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
    }
}