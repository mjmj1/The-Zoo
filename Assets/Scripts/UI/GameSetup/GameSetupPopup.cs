using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.GameSetting
{
    public class GameSetupPopup : MonoBehaviour
    {
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TMP_Dropdown aiLevelDropdown;
        [SerializeField] private TMP_Dropdown npcPopulationDropdown;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        private int _aiLevelOptionValue;

        private int _maxPlayersOptionValue;
        private int _npcRatioOptionValue;

        private void Start()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);

            maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersDropdownValueChanged);
            aiLevelDropdown.onValueChanged.AddListener(OnAiLevelDropdownValueChanged);
            npcPopulationDropdown.onValueChanged.AddListener(OnNpcPopulationDropdownValueChanged);

            var maxPlayers = GameManager.Instance.connectionManager.Session.MaxPlayers.ToString();

            _maxPlayersOptionValue = maxPlayersDropdown.options.FindIndex(data => data.text.Equals(maxPlayers));

            maxPlayersDropdown.value = _maxPlayersOptionValue;
            aiLevelDropdown.value = _aiLevelOptionValue;
            npcPopulationDropdown.value = _npcRatioOptionValue;

            var roomName = GameManager.Instance.connectionManager.Session.Name;
            roomNameInputField.placeholder.GetComponent<TMP_Text>().text = roomName;

            gameObject.SetActive(false);
        }

        private void OnMaxPlayersDropdownValueChanged(int value)
        {
            _maxPlayersOptionValue = value;
        }

        private void OnAiLevelDropdownValueChanged(int value)
        {
            _aiLevelOptionValue = value;
        }

        private void OnNpcPopulationDropdownValueChanged(int value)
        {
            _npcRatioOptionValue = value;
        }

        private void OnConfirmButtonClick()
        {
            var maxPlayers = int.Parse(maxPlayersDropdown.options[_maxPlayersOptionValue].text);

            GameManager.Instance.connectionManager.UpdateSessionAsync(roomNameInputField.text, maxPlayers);
            
            gameObject.SetActive(false);
        }

        private void OnCancelButtonClick()
        {
            roomNameInputField.text = string.Empty;
            
            gameObject.SetActive(false);
        }
    }
}