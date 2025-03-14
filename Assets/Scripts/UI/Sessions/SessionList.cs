using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Networks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionList : MonoBehaviour
    {
        [SerializeField]
        GameObject sessionItemPrefab;
        
        [SerializeField]
        GameObject contentParent;
        
        [SerializeField]
        Button joinSessionButton;
        
        [SerializeField]
        Button createSessionButton;
        
        [SerializeField]
        Button refreashSessionListButton;
        
        IList<GameObject> items = new List<GameObject>();
        IList<ISessionInfo> sessions;

        ISessionInfo _selectedSessionInfo;
        
        void Start()
        {
            RefreshSessionList();
            
            joinSessionButton.interactable = false;
            
            joinSessionButton.onClick.AddListener(OnJoinButtonClicked);
            createSessionButton.onClick.AddListener(OnCreateButtonClicked);
            refreashSessionListButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        private void OnDestroy()
        {
            joinSessionButton.onClick.RemoveListener(OnJoinButtonClicked);
            createSessionButton.onClick.RemoveListener(OnCreateButtonClicked);
            refreashSessionListButton.onClick.RemoveListener(OnRefreshButtonClicked);
        }

        public async void OnJoinButtonClicked()
        {
            if (_selectedSessionInfo == null) return;
            
            await MultiplayerService.Instance.JoinSessionByIdAsync(_selectedSessionInfo.Id);
        }

        public void OnCreateButtonClicked()
        {
            
        }
        
        public void OnRefreshButtonClicked()
        {
            RefreshSessionList();
        }
        
        private async void RefreshSessionList()
        {
            await UpdateSessions();
            
            foreach (var listItem in items)
            {
                Destroy(listItem);
            }
            
            if (sessions == null)
                return;
            
            foreach (var sessionInfo in sessions)
            {
                var itemPrefab = Instantiate(sessionItemPrefab, contentParent.transform);
                if (itemPrefab.TryGetComponent<SessionItem>(out var sessionItem))
                {
                    sessionItem.SetSession(sessionInfo);
                    sessionItem.onSessionSelected.AddListener(SelectSession);
                }
                items.Add(itemPrefab);
            }
        }

        private void SelectSession(ISessionInfo sessionInfo)
        {
            _selectedSessionInfo = sessionInfo;
            if (_selectedSessionInfo != null)
                joinSessionButton.interactable = true;
        }

        private async Task UpdateSessions()
        {
            sessions = await ConnectionManager.QuerySessions();
        }
    }
}
