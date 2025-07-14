using System;
using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;
using WebSocketSharp;

namespace UI
{
    public class MainUIManager : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private GameObject title;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button enterButton;
        
        [Header("Menu")]
        [SerializeField] private GameObject menu;
        
        [Header("Quick Start")]
        [SerializeField] private Button quickStartButton;
        
        [Header("Join")]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button joinButton;
        
        [Header("Session List")]
        [SerializeField] private Button sessionListButton;
        [SerializeField] private GameObject sessionsList;
        
        [Header("Preferences")]
        [SerializeField] private Button preferencesButton;
        [SerializeField] private GameObject preferences;
        
        private void Start()
        {
            OnTitle();
        }

        private void OnEnable()
        {
            joinButton.onClick.AddListener(OnJoinButtonClick);
            enterButton.onClick.AddListener(OnEnterButtonClick);
            quickStartButton.onClick.AddListener(OnQuickStartButtonClick);
            sessionListButton.onClick.AddListener(OnSessionListButtonClick);
            preferencesButton.onClick.AddListener(OnPreferencesButtonClick);
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveAllListeners();
            enterButton.onClick.RemoveAllListeners();
            quickStartButton.onClick.RemoveAllListeners();
            sessionListButton.onClick.RemoveAllListeners();
            preferencesButton.onClick.RemoveAllListeners();
        }

        private void OnTitle()
        {
            title.SetActive(true);
            
            menu.SetActive(false);
            
            sessionsList.SetActive(false);
            
            preferences.SetActive(false);
            preferencesButton.gameObject.SetActive(false);
        }

        private void OnMenu()
        {
            title.SetActive(false);
            
            menu.SetActive(true);
            
            sessionsList.SetActive(false);
            
            preferences.SetActive(false);
            
            preferencesButton.gameObject.SetActive(true);
        }
        
        private async void OnEnterButtonClick()
        {
            if (playerNameInput.text.IsNullOrEmpty())
            {
                InformationPopup.instance.ShowPopup("플레이어의 이름을 입력해주세요");
                
                return;
            }

            var playerName = playerNameInput.text;

            var task = ConnectionManager.Instance.Login(playerName);

            await task;
            
            if (task.IsCompletedSuccessfully)
            {
                OnMenu();
            }
            else
            {
                InformationPopup.instance.ShowPopup("로그인에 실패했습니다. 다시 시도해주세요.");
            }
        }
        
        private void OnQuickStartButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Quick, code: null, password: null, Util.GetRandomSessionName());

            ConnectionManager.Instance.ConnectAsync(data);
        }

        private void OnJoinButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.JoinByCode, codeInput.text);

            ConnectionManager.Instance.ConnectAsync(data);
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