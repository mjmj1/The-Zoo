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
        
        private ISessionInfo sessionInfo;
        
        internal readonly UnityEvent<ISessionInfo> OnSelected = new();

        public void OnSelect(BaseEventData eventData)
        {
            OnSelected?.Invoke(sessionInfo);
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
        }
    }
}