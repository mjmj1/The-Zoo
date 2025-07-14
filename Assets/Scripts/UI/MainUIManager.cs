using Networks;
using TMPro;
using UI.SessionList;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System;

namespace UI
{
    public class MainUIManager : MonoBehaviour
    {
        public enum MainState
        {
            Title,
            Menu,
        }
        [Header("Title")]
        [SerializeField] private GameObject title;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button enterButton;

        [Header("Menu")]
        [SerializeField] private GameObject menu;
        [SerializeField] private Button quickStartButton;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button sessionListButton;
        [SerializeField] private SessionListView sessionsList;

        private void Start()
        {
            SwitchUI(MainState.Title);
        }

        private void OnEnable()
        {
            joinButton.onClick.AddListener(OnJoinButtonClick);
            enterButton.onClick.AddListener(OnEnterButtonClick);
            quickStartButton.onClick.AddListener(OnQuickStartButtonClick);
            sessionListButton.onClick.AddListener(sessionsList.Toggle);
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveAllListeners();
            enterButton.onClick.RemoveAllListeners();
            quickStartButton.onClick.RemoveAllListeners();
            sessionListButton.onClick.RemoveAllListeners();
        }

        public void SwitchUI(MainState mainState)
        {
            title.SetActive(mainState == MainState.Title);
            menu.SetActive(mainState == MainState.Menu);
            
            sessionsList.gameObject.SetActive(false);
        }

        private async void OnEnterButtonClick()
        {
            if (string.IsNullOrEmpty(playerNameInput.text))
            {
                InformationPopup.instance.ShowPopup("플레이어의 이름을 입력해주세요");
                return;
            }

            var playerName = playerNameInput.text;

            try
            {
                await ConnectionManager.Instance.Login(playerName);
                MyLogger.Print(this, $"Login Success! PlayerName: {playerName}");
                SwitchUI(MainState.Menu);
            }
            catch (Exception e)
            {
                MyLogger.Print(this, $"Login Failed! PlayerName: {playerName}, Error: {e.Message}");
                InformationPopup.instance.ShowPopup("로그인에 실패했습니다. 다시 시도해주세요.");
            }
        }

        private void OnQuickStartButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Quick, null, null, Util.GetRandomSessionName());

            ConnectionManager.Instance.ConnectAsync(data);
        }

        private void OnJoinButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.JoinByCode, codeInput.text);

            ConnectionManager.Instance.ConnectAsync(data);
        }
    }
}