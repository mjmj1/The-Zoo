using Networks;
using TMPro;
using UI.SessionList;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System;
using EventHandler;
using WebSocketSharp;

namespace UI
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private Button quickStartButton;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button sessionListButton;
        [SerializeField] private SessionListView sessionsList;

        private void OnEnable()
        {
            sessionsList.gameObject.SetActive(false);
            joinButton.onClick.AddListener(OnJoinButtonClick);
            quickStartButton.onClick.AddListener(OnQuickStartButtonClick);
            sessionListButton.onClick.AddListener(sessionsList.Toggle);
            GamePlayEventHandler.OnUIChanged("Title");
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClick);
            quickStartButton.onClick.RemoveListener(OnQuickStartButtonClick);
            sessionListButton.onClick.RemoveListener(sessionsList.Toggle);
        }
        
        private async void OnQuickStartButtonClick()
        {
            try
            {
                var data = new ConnectionData(ConnectionData.ConnectionType.Quick, null, null,
                    Util.GetRandomSessionName());

                await ConnectionManager.Instance.ConnectAsync(data);
            }
            catch (Exception ex)
            {
                InformationPopup.instance.ShowPopup(ex.Message);
            }
        }

        private async void OnJoinButtonClick()
        {
            try
            {
                var code = codeInput.text;

                if (code.IsNullOrEmpty())
                {
                    InformationPopup.instance.ShowPopup("코드를 입력해주세요");

                    return;
                }

                var data = new ConnectionData(ConnectionData.ConnectionType.JoinByCode, codeInput.text);

                await ConnectionManager.Instance.ConnectAsync(data);
            }
            catch (Exception ex)
            {
                InformationPopup.instance.ShowPopup(ex.Message);
            }
        }
    }
}