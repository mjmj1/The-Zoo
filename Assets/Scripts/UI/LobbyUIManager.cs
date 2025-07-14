using System;
using Networks;
using TMPro;
using UI.PlayerList;
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
        
        private void OnEnable()
        {
            var session = ConnectionManager.Instance.CurrentSession;
            session.SessionHostChanged += OnSessionHostChanged;
            
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            gameReadyButton.onClick.AddListener(OnGameReadyButtonClick);
            
            ChangeUI(session.IsHost);
        }

        private void OnDisable()
        {
            quitButton.onClick.RemoveAllListeners();
            gameStartButton.onClick.RemoveAllListeners();

            var session = ConnectionManager.Instance.CurrentSession;
            session.SessionHostChanged -= OnSessionHostChanged;
        }

        private void OnSessionHostChanged(string obj)
        {
            ChangeUI(ConnectionManager.Instance.CurrentSession.IsHost);
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

        private void ChangeUI(bool isHost)
        {
            gameStartButton.gameObject.SetActive(isHost);
            gameReadyButton.gameObject.SetActive(!isHost);
            gameSetup.gameObject.SetActive(isHost);
        }
    }
}