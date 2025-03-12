using System;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Networks
{
    public class SessionManager : MonoBehaviour
    {
        static SessionManager instance;

        public static SessionManager Instance
        {
            get
            {
                if (instance == null || instance.gameObject == null)
                    CreateInstance();
                return instance;
            }
        }
        
        static void CreateInstance()
        {
            var gameObject = new GameObject($"{nameof(SessionManager)}");
            instance = gameObject.AddComponent<SessionManager>();
            DontDestroyOnLoad(gameObject);
        }
        
        async void QuerySessions()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);

            foreach (var session in results.Sessions)
            {
                print(session.ToString());
                print(session.Id);
                print(session.Name);
            }
        }
    }
}