using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.GameSetting
{
    public class GameSetupPopup : MonoBehaviour
    {
        [SerializeField] private Button gameSettingButton;

        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TMP_Dropdown aiLevelDropdown;
        [SerializeField] private TMP_Dropdown npcRatioDropdown;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private int _maxPlayersOptionValue;
        private int _aiLevelOptionValue;
        private int _npcRatioOptionValue;
        
        private void Start()
        {
            gameSettingButton.onClick.AddListener(OnSettingButtonClick);
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);

            var maxPlayers = GameManager.Instance.connectionManager.Session.MaxPlayers.ToString();

            _maxPlayersOptionValue = maxPlayersDropdown.options.FindIndex(data => data.text.Equals(maxPlayers));
            
            maxPlayersDropdown.value = _maxPlayersOptionValue;
            aiLevelDropdown.value = _aiLevelOptionValue;
            npcRatioDropdown.value = _npcRatioOptionValue;

            var roomName = GameManager.Instance.connectionManager.Session.Name;
            roomNameInputField.placeholder.GetComponent<TMP_Text>().text = roomName;
            
            gameObject.SetActive(false);
        }

        public void OnSettingButtonClick()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnConfirmButtonClick()
        {
            _maxPlayersOptionValue = maxPlayersDropdown.value;
            _aiLevelOptionValue = aiLevelDropdown.value;
            _npcRatioOptionValue = npcRatioDropdown.value;
            
            var maxPlayers = int.Parse(maxPlayersDropdown.options[_maxPlayersOptionValue].text);
            
            GameManager.Instance.connectionManager.UpdateSessionAsync(roomNameInputField.text, maxPlayers);

            OnSettingButtonClick();
        }

        private void OnCancelButtonClick()
        {
            roomNameInputField.text = string.Empty;
            OnSettingButtonClick();
        }
    }
}
