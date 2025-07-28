using System;
using Networks;
using UI.PlayerList;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private PlayerListView playerListView;
        [SerializeField] private GameObject gameSetup;

        [SerializeField] private Button quitButton;
        [SerializeField] private Button gameStartButton;
        [SerializeField] private Button gameReadyButton;

        private void OnEnable()
        {
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) return;

            var session = ConnectionManager.Instance.CurrentSession;
            session.SessionHostChanged += OnSessionHostChanged;
            session.PlayerJoined += OnPlayerChanged;
            session.PlayerHasLeft += OnPlayerChanged;

            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            gameReadyButton.onClick.AddListener(OnGameReadyButtonClick);

            SwitchUI(session.IsHost);

            gameStartButton.interactable = session.PlayerCount > 1;
        }

        private void OnDisable()
        {
            var session = ConnectionManager.Instance.CurrentSession;

            if (session == null) return;

            session.SessionHostChanged -= OnSessionHostChanged;

            quitButton.onClick.RemoveListener(OnQuitButtonClick);
            gameStartButton.onClick.RemoveListener(OnGameStartButtonClick);
            gameReadyButton.onClick.RemoveListener(OnGameReadyButtonClick);
        }

        private void OnPlayerChanged(string obj)
        {
            var session = ConnectionManager.Instance.CurrentSession;
            if (session == null) return;
            gameStartButton.interactable = session.PlayerCount > 1;
        }

        private void OnSessionHostChanged(string obj)
        {
            SwitchUI(ConnectionManager.Instance.CurrentSession.IsHost);
        }

        private async void OnQuitButtonClick()
        {
            try
            {
                var task = ConnectionManager.Instance.DisconnectSessionAsync();

                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnGameStartButtonClick()
        {
            GameManager.Instance.PlayerCount = playerListView.transform.childCount - 1; // µø±‚»≠ æ»µ 
            GameManager.Instance.GameStartRpc();
        }

        private void OnGameReadyButtonClick()
        {
            GameManager.Instance.Ready();
        }

        private void SwitchUI(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.gameObject.SetActive(isHost);
            gameReadyButton.gameObject.SetActive(!isHost);
        }
    }
}