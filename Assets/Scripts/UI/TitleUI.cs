using System;
using EventHandler;
using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TitleUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button enterButton;

        private void OnEnable()
        {
            enterButton.onClick.AddListener(OnEnterButtonClick);
        }

        private void OnDisable()
        {
            enterButton.onClick.RemoveListener(OnEnterButtonClick);
        }

        private async void OnEnterButtonClick()
        {
            try
            {
                if (string.IsNullOrEmpty(playerNameInput.text)) throw new Exception("플레이어의 이름을 입력해주세요");
                
                GamePlayEventHandler.PlayerLogin();
                
                var playerName = playerNameInput.text;

                await ConnectionManager.Instance.Login(playerName);
            }
            catch (Exception e)
            {
                InformationPopup.instance.ShowPopup(e.Message);
                Debug.LogError(e);
            }
        }
    }
}