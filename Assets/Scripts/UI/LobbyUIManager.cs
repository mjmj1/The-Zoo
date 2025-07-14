using Characters;
using Networks;
using TMPro;
using UI.PlayerList;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] private PlayerListView playerListView;
        [SerializeField] private GameObject gameSetup;
        
        [Header("Buttons")]
        [SerializeField] private Button quitButton;
        [SerializeField] private Button gameStartButton;
        [SerializeField] private Button gameReadyButton;
        
        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            gameReadyButton.onClick.AddListener(OnGameReadyButtonClick);

            var session = ConnectionManager.Instance.CurrentSession;
            session.SessionHostChanged += OnSessionHostChanged;
        }

        private void OnEnable()
        {
            ChangeLobbyUI(ConnectionManager.Instance.CurrentSession.IsHost);
        }

        private void OnDestroy()
        {
            quitButton.onClick.RemoveAllListeners();
            gameStartButton.onClick.RemoveAllListeners();

            var session = ConnectionManager.Instance.CurrentSession;
            session.SessionHostChanged -= OnSessionHostChanged;
        }

        private void OnSessionHostChanged(string obj)
        {
            ChangeLobbyUI(ConnectionManager.Instance.CurrentSession.IsHost);
        }

        private void OnQuitButtonClick()
        {
            ConnectionManager.Instance.DisconnectSessionAsync();
        }

        private void OnGameStartButtonClick()
        {
            GameManager.Instance.GameStartRpc();
        }
        
        private void OnGameReadyButtonClick()
        {
            GameManager.Instance.GameReadyRpc();
        }

        private void ChangeLobbyUI(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text =
                isHost ? "Game Start" : "Ready";
        }
    }
}