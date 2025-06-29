using Networks;
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
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject gameSetup;
        [SerializeField] private Button gameStartButton;

        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);

            var session = Manage.Session();
            session.SessionHostChanged += OnSessionHostChanged;
        }

        private void OnEnable()
        {
            ChangeLobbyUI(Manage.Session().IsHost);
        }

        private void OnDestroy()
        {
            quitButton.onClick.RemoveAllListeners();
            gameStartButton.onClick.RemoveAllListeners();

            var session = Manage.Session();
            session.SessionHostChanged -= OnSessionHostChanged;
        }

        private void OnSessionHostChanged(string obj)
        {
            ChangeLobbyUI(Manage.Session().IsHost);
        }

        private void OnQuitButtonClick()
        {
            ConnectionManager.instance.DisconnectSessionAsync();
        }

        private void OnGameStartButtonClick()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }

        private void ChangeLobbyUI(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text =
                isHost ? "Game Start" : "Ready";
        }
    }
}