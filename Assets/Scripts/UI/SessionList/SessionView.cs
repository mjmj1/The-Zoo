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
        
        private ISessionInfo sessionInfo;
        
        internal UnityEvent<ISessionInfo> OnSelected;
        internal UnityEvent OnDeselected;

        public void OnSelect(BaseEventData eventData)
        {
            OnSelected?.Invoke(sessionInfo);
        }
        
        public void OnDeselect(BaseEventData eventData)
        {
            sessionInfo = null;
            OnDeselected?.Invoke();
        }

        public void Bind(ISessionInfo info)
        {
            sessionInfo = info;

            IsLock(sessionInfo.HasPassword);
            sessionNameText.text = sessionInfo.Name;

            var currentPlayers = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots;
            sessionPlayersText.text = $"{currentPlayers}/{sessionInfo.MaxPlayers}";
        }

        private void IsLock(bool value)
        {
            lockIcon.SetActive(value);
            shine.color = value ? Color.gold : originColor;
        }
    }
}