using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.Preferences
{
    public class PlayerNamePreference : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Button playerNameSaveButton;

        private void Start()
        {
            playerNameSaveButton.onClick.AddListener(OnPlayerNameChanged);
        }

        private void OnDisable()
        {
            playerNameInputField.text = "";
        }

        private async void OnPlayerNameChanged()
        {
            if (string.IsNullOrEmpty(playerNameInputField.text)) return;

            var playerName = playerNameInputField.text;

            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

            UIManager.OpenInformationPopup("닉네임 설정이 완료되었습니다.");
        }
    }
}
