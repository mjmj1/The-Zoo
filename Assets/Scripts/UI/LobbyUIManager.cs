using Static;
using TMPro;
using UI.PlayerList;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] private PlayerListView playerListView;
        [SerializeField] private GameObject gameSetupPopup;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button gameSetupButton;
        [SerializeField] private Button gameStartButton;

        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameSetupButton.onClick.AddListener(OnSetupButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            GameManager.Instance.connectionManager.Session.Changed += OnSessionChanged;
        }

        private void OnSessionChanged()
        {
            SetupLobbyControl(Manage.Session().IsHost);
        }

        private void OnEnable()
        {
            SetupLobbyControl(Manage.Session().IsHost);
        }

        private void OnQuitButtonClick()
        {
            GameManager.Instance.connectionManager.DisconnectSessionAsync();
        }

        private void OnSetupButtonClick()
        {
            gameSetupPopup.SetActive(!gameSetupPopup.activeSelf);
        }

        private void OnGameStartButtonClick()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }

        private void SetupLobbyControl(bool isHost)
        {
            gameSetupButton.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text = isHost ? "Game Start" : "Ready";
        }
    }
}