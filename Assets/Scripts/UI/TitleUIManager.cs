using System;
using Networks;
using Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static Static.Strings;

namespace UI
{
    public class TitleUIManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button enterButton;
        [SerializeField] private Button quickStartButton;
        [SerializeField] private Button joinByCodeButton;
        [SerializeField] private Button sessionListButton;
        [SerializeField] private Button preferencesButton;
        
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField codeInput;

        [Header("Objects")]
        [SerializeField] private GameObject entrance;
        [SerializeField] private GameObject gameLobby;
        [SerializeField] private GameObject preferences;
        [SerializeField] private GameObject sessionsList;

        private void Start()
        {
            gameLobby.SetActive(false);
            sessionsList.SetActive(false);
            preferences.SetActive(false);
            preferencesButton.gameObject.SetActive(false);

            enterButton.onClick.AddListener(OnEnterButtonClick);
            quickStartButton.onClick.AddListener(OnQuickStartButtonClick);
            joinByCodeButton.onClick.AddListener(OnJoinByCodeButtonClick);
            sessionListButton.onClick.AddListener(OnSessionListButtonClick);
            preferencesButton.onClick.AddListener(OnPreferencesButtonClick);
        }

        private void OnDestroy()
        {
            enterButton.onClick.RemoveAllListeners();
            quickStartButton.onClick.RemoveAllListeners();
            joinByCodeButton.onClick.RemoveAllListeners();
            sessionListButton.onClick.RemoveAllListeners();
            preferencesButton.onClick.RemoveAllListeners();
        }

        private void OnEnterButtonClick()
        {
            if (playerNameInput.text.IsNullOrEmpty()) return;

            PlayerPrefs.SetString(PLAYERNAME, playerNameInput.text);

            gameLobby.SetActive(true);
            entrance.SetActive(false);
            preferencesButton.gameObject.SetActive(true);

            Manage.ConnectionManager().SignInAnonymouslyAsync();
        }
        
        private void OnQuickStartButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Quick, null, null, GetRandomSessionName());

            Manage.ConnectionManager().ConnectAsync(data);
        }

        private void OnJoinByCodeButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.JoinByCode, codeInput.text);

            Manage.ConnectionManager().ConnectAsync(data);
        }
        
        private void OnSessionListButtonClick()
        {
            sessionsList.SetActive(!sessionsList.activeSelf);
        }
        
        private void OnPreferencesButtonClick()
        {
            preferences.SetActive(!preferences.activeSelf);
        }
    }
}