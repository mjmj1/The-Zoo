using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.GameSetting
{
    public class GameSetupPopup : MonoBehaviour
    {
        [SerializeField] TMP_InputField roomNameInputField;
        [SerializeField] TMP_Dropdown maxPlayersDropdown;
        [SerializeField] TMP_Dropdown aiLevelDropdown;
        [SerializeField] TMP_Dropdown npcRatioDropdown;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;

        private int _maxPlayersOptionValue;
        private int _aiLevelOptionValue;
        private int _npcRatioOptionValue;
        
        private void Start()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            
            maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersDropdownValueChanged);
            aiLevelDropdown.onValueChanged.AddListener(OnAiLevelDropdownValueChanged);
            npcRatioDropdown.onValueChanged.AddListener(OnNpcRatioDropdownValueChanged);

            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            var roomName = GameManager.Instance.connectionManager.Session.Name;
            roomNameInputField.placeholder.GetComponent<TMP_Text>().text = roomName;
            
            var maxPlayers = GameManager.Instance.connectionManager.Session.MaxPlayers.ToString();
            _maxPlayersOptionValue = maxPlayersDropdown.options.FindIndex(data => data.text.Equals(maxPlayers));
            
            maxPlayersDropdown.value = _maxPlayersOptionValue;
            aiLevelDropdown.value = _aiLevelOptionValue;
            npcRatioDropdown.value = _npcRatioOptionValue;
        }
        
        void OnMaxPlayersDropdownValueChanged(int value)
        {
            _maxPlayersOptionValue = value;
        }
        
        void OnAiLevelDropdownValueChanged(int value)
        {
            _aiLevelOptionValue = value;
        }
        
        void OnNpcRatioDropdownValueChanged(int value)
        {
            _npcRatioOptionValue = value;
        }
        
        void OnConfirmButtonClick()
        {
            var maxPlayers = int.Parse(maxPlayersDropdown.options[_maxPlayersOptionValue].text);
            
            GameManager.Instance.connectionManager.UpdateSessionAsync(roomNameInputField.text, maxPlayers);

            OnClose();
        }

        void OnCancelButtonClick()
        {
            OnClose();
        }

        private void OnClose()
        {
            gameObject.SetActive(false);
            roomNameInputField.text = string.Empty;
        }
    }
}
