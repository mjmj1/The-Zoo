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

        private Canvas backgroundCanvas;
        private Canvas titleCanvas;
        private Canvas mainCanvas;
        private Canvas lobbyCanvas;
        private Canvas loadingCanvas;
        private Canvas popupCanvas;

        private void OnEnable()
        {
            AssignAllCanvases();

            SceneManager.sceneLoaded += OnSceneLoaded;
            GamePlayEventHandler.OnPlayerLogin += OnPlayerLogin;
            ConnectionEventHandler.OnSessionConnectStart += OnSessionConnectStart;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            UnityServices.Initialized += UnityServicesOnInitialized;
            NetworkManager.OnDestroying += OnDestroying;

            if (popupCanvas != null)
                popupCanvas.gameObject.SetActive(true);
        }

        private void OnDestroying(NetworkManager obj)
        {
            NetworkManager.OnDestroying -= OnDestroying;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            ConnectionEventHandler.OnSessionConnectStart -= OnSessionConnectStart;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Lobby") return;

            AssignAllCanvases();

            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                SwitchUI(UIType.Title);
                return;
            }

            if (ConnectionManager.Instance && ConnectionManager.Instance.CurrentSession != null)
            {
                SwitchUI(UIType.Lobby);
            }
            else if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                SwitchUI(UIType.Main);
            }
            else
            {
                SwitchUI(UIType.Title);
            }
        }

        private void AssignAllCanvases()
        {
            backgroundCanvas = FindCanvas(backgroundCanvasName);
            titleCanvas = FindCanvas(titleCanvasName);
            mainCanvas = FindCanvas(mainCanvasName);
            lobbyCanvas = FindCanvas(lobbyCanvasName);
            loadingCanvas = FindCanvas(loadingCanvasName);
            popupCanvas = FindCanvas(popupCanvasName);
        }

        private Canvas FindCanvas(string objName)
        {
            var go = GameObject.Find(objName);
            if (go == null)
            {
                Debug.LogWarning($"[UIManager] {objName} not found in scene.");
                return null;
            }
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning($"[UIManager] {objName} has no Canvas component.");
            }
            return canvas;
        }

        private void UnityServicesOnInitialized()
        {
            UnityServices.Initialized -= UnityServicesOnInitialized;

            AuthenticationService.Instance.SignedIn += OnSignedIn;
        }

        private void SwitchUI(UIType uiType)
        {
            titleCanvas.gameObject.SetActive(uiType == UIType.Title);
            mainCanvas.gameObject.SetActive(uiType == UIType.Main);
            lobbyCanvas.gameObject.SetActive(uiType == UIType.Lobby);
            backgroundCanvas.gameObject.SetActive(uiType != UIType.Lobby);
            loadingCanvas.gameObject.SetActive(false);
        }

        private void OnSignedIn()
        {
            SwitchUI(UIType.Main);
        }

        private void OnPlayerLogin()
        {
            loadingCanvas.gameObject.SetActive(true);
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