using System;
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

        private void Awake()
        {
            GameManager.Instance.connectionManager.NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        }

        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            gameSetupButton.onClick.AddListener(OnSetupButtonClick);
            gameStartButton.onClick.AddListener(OnGameStartButtonClick);
        }
        
        private void OnSessionOwnerPromoted(ulong owerId)
        {
            Setup();
        }

        private void OnEnable()
        {
            Setup();
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
            if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
            }
            else
            {
                
            }
        }

        private void Setup()
        {
            if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
            {
                gameSetupButton.gameObject.SetActive(true);
                gameStartButton.GetComponentInChildren<TMP_Text>().text = "Game Start";
            }
            else
            {
                gameSetupButton.gameObject.SetActive(false);
                gameStartButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            }
        }
    }
}