using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace UI.Sessions
{
    public class SessionListWindow : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        internal async Task<IList<ISessionInfo>> QuerySessions()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            //var m_SessionQueryResults = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions).Result;
            // return m_SessionQueryResults.Sessions;
            return null;
        }
    }
}
