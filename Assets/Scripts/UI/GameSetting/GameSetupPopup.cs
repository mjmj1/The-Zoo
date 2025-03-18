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
        [SerializeField] private Button gameSettingButton;
        [SerializeField] private Button gameStartButton;

        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TMP_Dropdown aiLevelDropdown;
        [SerializeField] private TMP_Dropdown npcRatioDropdown;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private GameObject worldObject;
        [SerializeField] private GameObject interactionObjects;
        SphereCollider sphereCollider;

        private int _maxPlayersOptionValue;
        private int _aiLevelOptionValue;
        private int _npcRatioOptionValue;
        
        private void Start()
        {
            sphereCollider = worldObject.GetComponentInChildren<SphereCollider>();

            gameStartButton.onClick.AddListener(OnGameStartButtonClick);

            gameSettingButton.onClick.AddListener(OnSettingButtonClick);
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            
            maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersDropdownValueChanged);
            aiLevelDropdown.onValueChanged.AddListener(OnAiLevelDropdownValueChanged);
            npcRatioDropdown.onValueChanged.AddListener(OnNpcRatioDropdownValueChanged);

            var maxPlayers = GameManager.Instance.connectionManager.Session.MaxPlayers.ToString();

            _maxPlayersOptionValue = maxPlayersDropdown.options.FindIndex(data => data.text.Equals(maxPlayers));
            
            maxPlayersDropdown.value = _maxPlayersOptionValue;
            aiLevelDropdown.value = _aiLevelOptionValue;
            npcRatioDropdown.value = _npcRatioOptionValue;

            var roomName = GameManager.Instance.connectionManager.Session.Name;
            roomNameInputField.placeholder.GetComponent<TMP_Text>().text = roomName;
            
            gameObject.SetActive(false);
        }

        public void OnGameStartButtonClick()
        {
            var objects = Instantiate(interactionObjects, Return_RandomPosition(), Quaternion.identity);
        }

        Vector3 Return_RandomPosition()
        {
            Vector3 originPosition = interactionObjects.transform.position;
            // 콜라이더의 사이즈를 가져오는 bound.size 사용
            float range_X = sphereCollider.bounds.size.x;
            float range_Y = sphereCollider.bounds.size.y;
            float range_Z = sphereCollider.bounds.size.z;

            range_X = UnityEngine.Random.Range((range_X / 2) * -1, range_X / 2);
            range_Y = UnityEngine.Random.Range((range_Y / 2) * -1, range_Y / 2);
            range_Z = UnityEngine.Random.Range((range_Z / 2) * -1, range_Z / 2);
            Vector3 RandomPostion = new Vector3(range_X, range_Y, range_Z);

            Vector3 respawnPosition = originPosition + RandomPostion;
            return respawnPosition;
        }

        public void OnSettingButtonClick()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnMaxPlayersDropdownValueChanged(int value)
        {
            _maxPlayersOptionValue = value;
        }
        
        private void OnAiLevelDropdownValueChanged(int value)
        {
            _aiLevelOptionValue = value;
        }
        
        private void OnNpcRatioDropdownValueChanged(int value)
        {
            _npcRatioOptionValue = value;
        }
        
        private void OnConfirmButtonClick()
        {
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
