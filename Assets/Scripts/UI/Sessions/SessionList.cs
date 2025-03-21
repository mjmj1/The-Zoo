using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Networks;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Sessions
{
    public class SessionList : MonoBehaviour
    {
        [SerializeField]
        GameObject sessionItemPrefab;
        
        [SerializeField] GameObject contentParent;
        
        [SerializeField] Button joinSessionButton;
        
        [SerializeField] Button createSessionButton;
        
        [SerializeField] Button refreshSessionListButton;
        
        IList<GameObject> items = new List<GameObject>();
        IList<ISessionInfo> sessions;

        ISessionInfo _selectedSessionInfo;
        
        void Start()
        {
            RefreshSessionListAsync();
            
            joinSessionButton.interactable = false;
            
            joinSessionButton.onClick.AddListener(OnJoinButtonClick);
            createSessionButton.onClick.AddListener(OnCreateButtonClick);
            refreshSessionListButton.onClick.AddListener(OnRefreshButtonClick);
        }

        private void OnDestroy()
        {
            joinSessionButton.onClick.RemoveListener(OnJoinButtonClick);
            createSessionButton.onClick.RemoveListener(OnCreateButtonClick);
            refreshSessionListButton.onClick.RemoveListener(OnRefreshButtonClick);
        }

        public async void OnJoinButtonClick()
        {
            if (_selectedSessionInfo == null) return;

            await GameManager.Instance.connectionManager.JoinSessionByIdAsync(_selectedSessionInfo.Id);
            
            _selectedSessionInfo = null;
            joinSessionButton.interactable = false;
            gameObject.SetActive(false);
        }

        public void OnCreateButtonClick()
        {
            
        }
        
        public void OnRefreshButtonClick()
        {
            RefreshSessionListAsync();
        }
        
        private async void RefreshSessionListAsync()
        {
            try
            {
                await UpdateSessionsAsync();
            
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
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void SelectSession(ISessionInfo sessionInfo)
        {
            _selectedSessionInfo = sessionInfo;
            if (_selectedSessionInfo != null)
                joinSessionButton.interactable = true;
        }

        private async Task UpdateSessionsAsync()
        {
            sessions = await GameManager.Instance.connectionManager.QuerySessionsAsync();
        }
    }
}
