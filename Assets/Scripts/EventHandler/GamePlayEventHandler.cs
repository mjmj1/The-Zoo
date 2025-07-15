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
        internal static event Action<ulong, bool> OnPlayerReady;
        internal static event Action OnPlayerLogin;
        
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

        internal static void PlayerReady(ulong clientId, bool newValue)
        {
            OnPlayerReady?.Invoke(clientId, newValue);
        }
        
        internal static void PlayerLogin()
        {
            OnPlayerLogin?.Invoke();
        }
    }
}