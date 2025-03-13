using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerNameSetup;
        [SerializeField] private GameObject gameStart;
        
        [SerializeField] private GameObject preferencesButton;
        [SerializeField] private GameObject preferences;
        
        [SerializeField] private GameObject sessionsList;

        private static InformationPopup _informationPopup;
        
        private TMP_InputField _playerNameInputField;
        
        private void Start()
        {
            gameStart.SetActive(false);
            preferencesButton.SetActive(false);
            preferences.SetActive(false);
            sessionsList.SetActive(false);
            
            _informationPopup = GetComponent<InformationPopup>();
            _playerNameInputField = playerNameSetup.GetComponentInChildren<TMP_InputField>();
        }

        
        public void OnPlayerNameSaveButtonClicked()
        {
            PlayerPrefs.SetString("PlayerName", _playerNameInputField.text);
            
            playerNameSetup.SetActive(false);
            gameStart.SetActive(true);
            preferencesButton.SetActive(true);

            ConnectionManager.SignInAnonymously();
        }
        
        public void OnPreferencesButtonClicked()
        {
            preferences.SetActive(!preferences.activeSelf);
        }

        public void OnSessionListButtonClicked()
        {
            sessionsList.SetActive(!sessionsList.activeSelf);
        }
        
        public static void OpenInformationPopup(string msg)
        {
            _informationPopup.GetInformationPopup(msg);
        }
    }
}
