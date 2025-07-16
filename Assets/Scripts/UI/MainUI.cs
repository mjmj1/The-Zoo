using Networks;
using TMPro;
using UI.SessionList;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System;
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
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClick);
            quickStartButton.onClick.RemoveListener(OnQuickStartButtonClick);
            sessionListButton.onClick.AddListener(sessionsList.Toggle);
        }
        
        private void OnQuickStartButtonClick()
        {
            var data = new ConnectionData(ConnectionData.ConnectionType.Quick, null, null, Util.GetRandomSessionName());

            ConnectionManager.Instance.ConnectAsync(data);
        }

        private void OnJoinButtonClick()
        {
            var code = codeInput.text;
            
            if (code.IsNullOrEmpty())
            {
                InformationPopup.instance.ShowPopup("코드를 입력해주세요");

                return;
            }
            
            var data = new ConnectionData(ConnectionData.ConnectionType.JoinByCode, codeInput.text);

            ConnectionManager.Instance.ConnectAsync(data);
        }
    }
}