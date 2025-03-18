using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static Static.Strings;

namespace UI
{
    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] GameObject playerNameSetup;
        [SerializeField] GameObject gameStart;
        
        [SerializeField] GameObject preferencesButton;
        [SerializeField] GameObject preferences;
        
        [SerializeField] GameObject sessionsList;
        
        TMP_InputField _playerNameInputField;
        
        private void Start()
        {
            gameStart.SetActive(false);
            preferencesButton.SetActive(false);
            preferences.SetActive(false);
            sessionsList.SetActive(false);
            
            _playerNameInputField = playerNameSetup.GetComponentInChildren<TMP_InputField>();
        }
        public void OnPlayerNameSaveButtonClick()
        {
            if(_playerNameInputField.text.IsNullOrEmpty()) return;
            
            PlayerPrefs.SetString(PLAYERNAME, _playerNameInputField.text);
            
            playerNameSetup.SetActive(false);
            gameStart.SetActive(true);
            preferencesButton.SetActive(true);

            GameManager.Instance.connectionManager.SignInAnonymouslyAsync();
        }
        
        public void OnPreferencesButtonClick()
        {
            preferences.SetActive(!preferences.activeSelf);
        }

        public void OnSessionListButtonClick()
        {
            sessionsList.SetActive(!sessionsList.activeSelf);
        }
    }
}
