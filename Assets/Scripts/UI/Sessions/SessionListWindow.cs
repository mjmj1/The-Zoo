using System.Collections.Generic;
using System.Threading.Tasks;
using Networks;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.Sessions
{
    public class SessionListWindow : MonoBehaviour
    {
        [SerializeField]
        GameObject sessionItemPrefab;
        
        [SerializeField]
        GameObject contentParent;
        
        IList<GameObject> items = new List<GameObject>();
        IList<ISessionInfo> sessions;
        
        ISessionInfo selectedSessionInfo;
        
        void Start()
        {
            RefreshSessionList();
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
                    sessionItem.OnSessionSelected.AddListener(SelectSession);
                }
                items.Add(itemPrefab);
            }
        }
        
        void SelectSession(ISessionInfo sessionInfo)
        {
            selectedSessionInfo = sessionInfo;
            /*if (Session == null)
                m_EnterSessionButton.interactable = true;*/
        }
        
        async Task UpdateSessions()
        {
            sessions = await ConnectionManager.QuerySessions();
        }
    }
}
