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
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject gameSetup;
        [SerializeField] private Button gameStartButton;

        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);

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
            if (ConnectionManager.Instance.CurrentSession.IsHost)
            {
                if (!GameManager.Instance.CanGameStart()) return;

                GameManager.Instance.LoadSceneRpc("InGame");

                return;
            }

            var entity = NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<PlayerEntity>();
            var isReady = entity.isReady.Value;

            entity.isReady.Value = !isReady;

            GameManager.Instance.ReadyRpc(AuthenticationService.Instance.PlayerId, entity.isReady.Value);
        }

        private void ChangeLobbyUI(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text =
                isHost ? "Game Start" : "Ready";
        }
    }
}