using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.PlayerList
{
    public class PlayerView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite hostSprite;
        
        [SerializeField] private Image stateIcon;
        [SerializeField] private TMP_Text playerNameText;

        [SerializeField] private GameObject actionButtons;
        [SerializeField] private Button promoteHostButton;
        [SerializeField] private Button kickButton;
        private IReadOnlyPlayer _data;

        private string _hostId;

        private void Start()
        {
            actionButtons.SetActive(false);
            promoteHostButton.onClick.AddListener(OnPromoteHostButtonClick);
            kickButton.onClick.AddListener(OnKickButtonClick);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;

            if (_data.Id.Equals(_hostId)) return;

            actionButtons.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            actionButtons.SetActive(false);
        }
        
        public void Bind(string hostId, IReadOnlyPlayer data)
        {
            _data = data;
            
            playerNameText.text =
                _data.Properties.TryGetValue(PLAYERNAME, out var nameProperty) ? nameProperty.Value : "Unknown";

            SetHost(hostId);
        }

        public void SetHost(string hostId)
        {
            _hostId = hostId;

            if (_data.Id.Equals(_hostId))
            {
                stateIcon.sprite = hostSprite;
                stateIcon.color = new Color(255, 224, 0, 255);
            }
            else
            {
                stateIcon.sprite = null;
                stateIcon.color = new Color(255, 255, 255, 0);
            }
        }

        private void OnPromoteHostButtonClick()
        {
            GameManager.Instance.connectionManager.ChangeHostAsync(_data.Id);
        }

        private void OnKickButtonClick()
        {
            GameManager.Instance.connectionManager.KickPlayerAsync(_data.Id);
        }
    }
}