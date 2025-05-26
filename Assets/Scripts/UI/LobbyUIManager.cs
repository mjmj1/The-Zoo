using System;
using Static;
using TMPro;
using UI.PlayerList;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplay.Authoring.Editor;
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
        
        private void OnSessionHostChanged(string obj)
        {
            ChangeLobbyUI(Manage.Session().IsHost);
        }

        private void OnQuitButtonClick()
        {
            Manage.ConnectionManager().DisconnectSessionAsync();
        }

        private void OnGameStartButtonClick()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }

        private void ChangeLobbyUI(bool isHost)
        {
            gameSetup.gameObject.SetActive(isHost);
            gameStartButton.GetComponentInChildren<TMP_Text>().text = isHost ? "Game Start" : "Ready";
        }
    }
}