using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.Preferences
{
    public class PlayerNamePreference : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Button playerNameSaveButton;
        
        void Start()
        {
            playerNameSaveButton.onClick.AddListener(OnProfileNameButtonClick);
        }

        void OnEnable()
        {
            playerNameInputField.placeholder.GetComponent<TextMeshProUGUI>().text = LoadProfileName();
        }

        void OnDisable()
        {
            playerNameInputField.text = "";
        }
        
        string LoadProfileName()
        {
            return PlayerPrefs.GetString(PLAYERNAME);
        }
    
        void SaveProfileName(string profileName)
        {
            PlayerPrefs.SetString(PLAYERNAME, profileName);
            
            PlayerPrefs.Save();
        }

        public void OnProfileNameButtonClick()
        {
            if (string.IsNullOrEmpty(playerNameInputField.text)) return;

            SaveProfileName(playerNameInputField.text);

            TitleUIManager.OpenInformationPopup("닉네임 설정이 완료되었습니다.");
        }
    }
}
