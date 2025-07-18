using EventHandler;
using Networks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private string titleCanvasName = "TitleCanvas";
        [SerializeField] private string mainCanvasName = "MainCanvas";
        [SerializeField] private string lobbyCanvasName = "LobbyCanvas";
        [SerializeField] private string backgroundCanvasName = "BackgroundCanvas";
        [SerializeField] private string loadingCanvasName = "LoadingCanvas";
        [SerializeField] private string popupCanvasName = "InformationPopup";

        public GameObject backgroundCanvas;
        public GameObject titleCanvas;
        public GameObject mainCanvas;
        public GameObject lobbyCanvas;
        public GameObject loadingCanvas;
        public GameObject popupCanvas;

        private void OnEnable()
        {
            AssignAllCanvases();

            SceneManager.sceneLoaded += OnSceneLoaded;

            GamePlayEventHandler.OnPlayerLogin += OnPlayerLogin;
            ConnectionEventHandler.OnSessionConnectStart += OnSessionConnectStart;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

            UnityServices.Initialized += UnityServicesOnInitialized;

            if (popupCanvas != null)
                popupCanvas.SetActive(true);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            AuthenticationService.Instance.SignedIn -= OnSignedIn;

            ConnectionEventHandler.OnSessionConnectStart -= OnSessionConnectStart;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name != "Lobby") return;

            AssignAllCanvases();

            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                SwitchUI(UIType.Title);
                return;
            }

            if (ConnectionManager.Instance && ConnectionManager.Instance.CurrentSession != null)
                SwitchUI(UIType.Lobby);
            else if (AuthenticationService.Instance != null &&
                     AuthenticationService.Instance.IsSignedIn)
                SwitchUI(UIType.Main);
            else
                SwitchUI(UIType.Title);
        }

        private void SetActiveAllCanvases(bool active)
        {
            titleCanvas.SetActive(active);
            mainCanvas.SetActive(active);
            lobbyCanvas.SetActive(active);
            loadingCanvas.SetActive(active);
            popupCanvas.SetActive(active);
            backgroundCanvas.SetActive(active);
        }

        private void AssignAllCanvases()
        {
            titleCanvas = FindObject(titleCanvasName);
            mainCanvas = FindObject(mainCanvasName);
            lobbyCanvas = FindObject(lobbyCanvasName);
            loadingCanvas = FindObject(loadingCanvasName);
            popupCanvas = FindObject(popupCanvasName);
            backgroundCanvas = FindObject(backgroundCanvasName);
        }

        private GameObject FindObject(string objName)
        {
            var go = GameObject.Find(objName);
            if (go == null)
            {
                Debug.LogWarning($"[UIManager] {objName} not found in scene.");
                return null;
            }

            return go;
        }

        private void UnityServicesOnInitialized()
        {
            UnityServices.Initialized -= UnityServicesOnInitialized;

            AuthenticationService.Instance.SignedIn += OnSignedIn;
        }

        private void SwitchUI(UIType uiType)
        {
            if (!titleCanvas || !mainCanvas || !lobbyCanvas || !loadingCanvas || !popupCanvas ||
                !backgroundCanvas) return;

            titleCanvas.SetActive(uiType == UIType.Title);
            mainCanvas.SetActive(uiType == UIType.Main);
            lobbyCanvas.SetActive(uiType == UIType.Lobby);
            backgroundCanvas.SetActive(uiType != UIType.Lobby);
            loadingCanvas.SetActive(false);
        }

        private void OnSignedIn()
        {
            SwitchUI(UIType.Main);
        }

        private void OnPlayerLogin()
        {
            loadingCanvas.SetActive(true);
        }

        private void OnSessionConnectStart()
        {
            loadingCanvas.SetActive(true);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SwitchUI(UIType.Lobby);
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            SetActiveAllCanvases(true);

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