using Static;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static InformationPopup _informationPopup;

        public static TitleUIManager TitleUIManager;
        public static LobbyUIManager LobbyUIManager;

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
            Manage.ConnectionManager().OnSessionConnecting += OnSessionConnecting;
            Manage.ConnectionManager().OnSessionDisconnected += OnSessionDisconnected;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            
            TitleUIManager = titleMenu.GetComponent<TitleUIManager>();
            LobbyUIManager = lobbyMenu.GetComponent<LobbyUIManager>();
        }

        public void Update()
        {
            HandleMouseLock();
        }

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

            if (!Input.GetMouseButtonDown(0) || Cursor.lockState == CursorLockMode.Locked) return;

            if (IsPointerOverUI()) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }

        private void SetupUI(bool isConnected)
        {
            titleMenu.SetActive(!isConnected);
            lobbyMenu.SetActive(isConnected);
            
            loadingScreen.gameObject.SetActive(false);
        }

        private void OnSessionConnecting()
        {
            loadingScreen.gameObject.SetActive(true);
        }

        private void OnSessionDisconnected()
        {
            loadingScreen.gameObject.SetActive(false);
        }
        
        private void OnClientConnectedCallback(ulong clientId)
        {
            if(Manage.LocalClient().ClientId != clientId) return;
            
            SetupUI(true);
        }
        
        private void OnClientDisconnectCallback(ulong clientId)
        {
            if(Manage.LocalClient().ClientId != clientId) return;
            
            SetupUI(false);
        }
    }
}