using System;
using UI;
using Unity.Netcode;

namespace EventHandler
{
    internal static class GamePlayEventHandler
    {
        internal static event Action OnGameStart;
        internal static event Action OnGameOver;
        internal static event Action OnGameFinished;
        internal static event Action<string, bool> OnPlayerReady;
        internal static event Action OnPlayerLogin;
        internal static event Action OnSceneEvent;


        private static void GameStart()
        {
            OnGameStart?.Invoke();
        }

        private static void GameOver()
        {
            OnGameOver?.Invoke();
        }

        private static void GameFinished()
        {
            OnGameFinished?.Invoke();
        }
        
        internal static void PlayerReady(string id, bool value)
        {
            OnPlayerReady?.Invoke(id, value);
        }
        
        internal static void PlayerLogin()
        {
            OnPlayerLogin?.Invoke();
        }

        private static void SceneEvent()
        {
            OnSceneEvent?.Invoke();
        }
    }
}