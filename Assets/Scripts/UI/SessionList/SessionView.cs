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
    public class SessionView : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Color originColor;
        
        [SerializeField] private Image shine;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TMP_Text sessionNameText;
        [SerializeField] private TMP_Text sessionPlayersText;
        
        private ISessionInfo _sessionInfo;
        
        internal UnityEvent<ISessionInfo> OnSelected;
        internal UnityEvent OnDeselected;

        public void OnSelect(BaseEventData eventData)
        {
            OnSelected?.Invoke(_sessionInfo);
        }
        
        public void OnDeselect(BaseEventData eventData)
        {
            OnDeselected?.Invoke();
        }

        public void Bind(ISessionInfo info)
        {
            print("Bind");

            _sessionInfo = info;

            IsLock(_sessionInfo.HasPassword);
            sessionNameText.text = _sessionInfo.Name;

            var currentPlayers = _sessionInfo.MaxPlayers - _sessionInfo.AvailableSlots;
            sessionPlayersText.text = $"{currentPlayers}/{_sessionInfo.MaxPlayers}";
        }

        private void IsLock(bool value)
        {
            lockIcon.SetActive(value);
            shine.color = value ? Color.gold : originColor;
        }
    }
}