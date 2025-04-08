using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.SessionList
{
    public class SessionView : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text sessionNameText;
        [SerializeField] private TMP_Text sessionPlayersText;

        public UnityEvent<ISessionInfo> onSelect;

        private ISessionInfo _sessionInfo;

        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke(_sessionInfo);
        }

        public void Bind(ISessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            
            lockIcon.enabled = _sessionInfo.HasPassword;
            sessionNameText.text = _sessionInfo.Name;
            
            var currentPlayers = _sessionInfo.MaxPlayers - _sessionInfo.AvailableSlots;
            sessionPlayersText.text = $"{currentPlayers}/{_sessionInfo.MaxPlayers}";
        }
    }
}