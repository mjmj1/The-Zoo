using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionView : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text sessionNameText;
        [SerializeField] private TMP_Text sessionPlayersText;

        public UnityEvent<ISessionInfo> onSessionSelected;

        private ISessionInfo _sessionInfo;

        public void OnSelect(BaseEventData eventData)
        {
            onSessionSelected?.Invoke(_sessionInfo);
        }

        public void Bind(ISessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            UpdateLocked(_sessionInfo.HasPassword);

            UpdateSessionName(_sessionInfo.Name);
            
            var currentPlayers = _sessionInfo.MaxPlayers - _sessionInfo.AvailableSlots;
            UpdatePlayersCount(currentPlayers, _sessionInfo.MaxPlayers);
        }

        private void UpdateLocked(bool hasPassword)
        {
            lockIcon.enabled = hasPassword;
        }
        
        private void UpdateSessionName(string sessionName)
        {
            sessionNameText.text = sessionName;
        }

        private void UpdatePlayersCount(int currentPlayers, int maxPlayers)
        {
            sessionPlayersText.text = $"{currentPlayers}/{maxPlayers}";
        }
    }
}