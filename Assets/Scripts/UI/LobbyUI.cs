using System;
using Characters;
using Networks;
using TMPro;
using UI.PlayerList;
using Unity.Netcode;
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
            
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            gameReadyButton.onClick.AddListener(OnGameReadyButtonClick);
            
            SwitchUI(session.IsHost);
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

        private void OnSessionHostChanged(string obj)
        {
            SwitchUI(ConnectionManager.Instance.CurrentSession.IsHost);
        }

        private void OnQuitButtonClick()
        {
            ConnectionManager.Instance.DisconnectSessionAsync();
        }

        private void OnGameStartButtonClick()
        {
            GameManager.Instance.PlayerCount = playerListView.transform.childCount;
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