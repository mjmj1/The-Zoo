using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIEventManager : MonoBehaviour
    {
        [SerializeField] private GameObject profileSetupUI;
        [SerializeField] private GameObject gameStartUI;
        
        [SerializeField] private GameObject preferencesButton;
        [SerializeField] private GameObject preferencesWindow;
        [SerializeField] private GameObject sessionsWindow;

        private static InfoWindowManager _infoWindowManager;
        
        private TMP_InputField _profileSetupInputField;
        
        private void Start()
        {
            gameStartUI.SetActive(false);
            preferencesButton.SetActive(false);
            preferencesWindow.SetActive(false);
            sessionsWindow.SetActive(false);
            
            _infoWindowManager = GetComponent<InfoWindowManager>();
            _profileSetupInputField = profileSetupUI.GetComponentInChildren<TMP_InputField>();
        }

        
        public void OnEnterButtonClick()
        {
            PlayerPrefs.SetString("ProfileName", _profileSetupInputField.text);
            
            profileSetupUI.SetActive(false);
            gameStartUI.SetActive(true);
            preferencesButton.SetActive(true);
            
            ConnectionManager.SignInAnonymously(PlayerPrefs.GetString("ProfileName"));
        }
        
        public void OnPreferencesClick()
        {
            preferencesWindow.SetActive(!preferencesWindow.activeSelf);
        }

        public void OnSessionListClick()
        {
            sessionsWindow.SetActive(!sessionsWindow.activeSelf);
        }
        
        public static void OpenInfoWindow(string msg)
        {
            _infoWindowManager.GetInfoWindow(msg);
        }
    }
}
