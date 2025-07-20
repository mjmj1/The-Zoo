using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace UI.SessionList
{
    public class SessionView : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TMP_Text sessionNameText;
        [SerializeField] private TMP_Text sessionPlayersText;
        
        private ISessionInfo _sessionInfo;
        
        internal readonly UnityEvent<ISessionInfo> OnSelected = new();

        public void OnSelect(BaseEventData eventData)
        {
            OnSelected?.Invoke(_sessionInfo);
        }

        public void Bind(ISessionInfo info)
        {
            _sessionInfo = info;

            IsLock(_sessionInfo.HasPassword);
            sessionNameText.text = _sessionInfo.Name;

            var currentPlayers = _sessionInfo.MaxPlayers - _sessionInfo.AvailableSlots;
            sessionPlayersText.text = $"{currentPlayers}/{_sessionInfo.MaxPlayers}";
        }

        private void IsLock(bool value)
        {
            lockIcon.SetActive(value);
        }
    }
}