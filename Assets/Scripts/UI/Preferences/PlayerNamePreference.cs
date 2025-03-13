using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Preferences
{
    public class PlayerNamePreference : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Button playerNameSaveButton;
        
        private const string Playernamekey = "PlayerName";
        
        private void Start()
        {
            playerNameSaveButton.onClick.AddListener(OnProfileNameButtonClicked);
        }

        private void OnEnable()
        {
            playerNameInputField.placeholder.GetComponent<TextMeshProUGUI>().text = LoadProfileName();
        }

        private void OnDisable()
        {
            playerNameInputField.text = "";
        }
        
        private string LoadProfileName()
        {
            return PlayerPrefs.GetString(Playernamekey);
        }
    
        private void SaveProfileName(string profileName)
        {
            PlayerPrefs.SetString(Playernamekey, profileName);
            
            PlayerPrefs.Save();
        }

        public void OnProfileNameButtonClicked()
        {
            if (string.IsNullOrEmpty(playerNameInputField.text)) return;

            SaveProfileName(playerNameInputField.text);

            TitleUIManager.OpenInformationPopup("닉네임 설정이 완료되었습니다.");
        }
    }
}
