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

            NetworkManager.Singleton.OnSessionOwnerPromoted += OnSessionOwnerChanged;
        }

        private void OnEnable()
        {
            SetupLobbyControl(Manage.Session().IsHost);
        }
        
        private void OnSessionOwnerChanged(ulong sessionownerpromoted)
        {
            SetupLobbyControl(Manage.LocalClient().IsSessionOwner);
        }

        private void OnActiveSessionChanged()
        {
            print("Session Properties Changed");
            SetupLobbyControl(Manage.Session().IsHost);
        }

        private void OnQuitButtonClick()
        {
            Manage.ConnectionManager().DisconnectSessionAsync();
        }

        private void OnGameStartButtonClick()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }

        private void SetupLobbyControl(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text = isHost ? "Game Start" : "Ready";
        }
    }
}