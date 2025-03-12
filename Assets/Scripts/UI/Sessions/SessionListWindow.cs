using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.Sessions
{
    public class SessionListWindow : MonoBehaviour
    {
        [SerializeField]
        GameObject m_SessionItemPrefab;
        
        [SerializeField]
        GameObject m_ContentParent;
        
        IList<GameObject> m_ListItems = new List<GameObject>();
        IList<ISessionInfo> m_Sessions;
        
        ISessionInfo m_SelectedSessionInfo;
        
        void Start()
        {
            RefreshSessionList();
        }

        public void OnRefreshButtonClicked()
        {
            RefreshSessionList();
        }
        
        internal async void RefreshSessionList()
        {
            await UpdateSessions();
            
            foreach (var listItem in m_ListItems)
            {
                Destroy(listItem);
            }
            
            if (m_Sessions == null)
                return;
            
            foreach (var sessionInfo in m_Sessions)
            {
                var itemPrefab = Instantiate(m_SessionItemPrefab, m_ContentParent.transform);
                if (itemPrefab.TryGetComponent<SessionItem>(out var sessionItem))
                {
                    sessionItem.SetSession(sessionInfo);
                    sessionItem.OnSessionSelected.AddListener(SelectSession);
                }
                m_ListItems.Add(itemPrefab);
            }
        }
        
        void SelectSession(ISessionInfo sessionInfo)
        {
            m_SelectedSessionInfo = sessionInfo;
            /*if (Session == null)
                m_EnterSessionButton.interactable = true;*/
        }
        
        async Task UpdateSessions()
        {
            m_Sessions = await QuerySessions();
        }
        
        async Task<IList<ISessionInfo>> QuerySessions()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            return results.Sessions;
        }
    }
}
