using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Preferences
{
    public class ProfileNameManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField profileNameField;
        [SerializeField] private Button profileNameButton;
    
        private string GenerateRandomProfileName()
        {
            return "User_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    
        private string LoadProfileName()
        {
            return PlayerPrefs.GetString("ProfileName", GenerateRandomProfileName());
        }
    
        private void SaveProfileName(string profileName)
        {
            PlayerPrefs.SetString("ProfileName", profileName);
            
            PlayerPrefs.Save();
        }

        private void OnProfileNameButtonClick()
        {
            if (string.IsNullOrEmpty(profileNameField.text)) return;
            
            SaveProfileName(profileNameField.text);

            UIEventManager.OpenInfoWindow("닉네임 설정이 완료되었습니다.");
        }

        private void Start()
        {
            profileNameButton.onClick.AddListener(OnProfileNameButtonClick);
        }

        private void OnEnable()
        {
            profileNameField.placeholder.GetComponent<TextMeshProUGUI>().text = LoadProfileName();
        }

        private void OnDisable()
        {
            profileNameField.text = "";
        }
    }
}
