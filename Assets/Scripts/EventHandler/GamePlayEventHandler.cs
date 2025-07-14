using System;

namespace EventHandler
{
    static class GamePlayEventHandler
    {
        internal static event Action OnGameStart;
        internal static event Action OnGameOver;
        internal static event Action OnGameFinished;

        private static void OnOnGameStart()
        {
            OnGameStart?.Invoke();
        }

        private static void OnOnGameOver()
        {
            OnGameOver?.Invoke();
        }

        private static void OnOnGameFinished()
        {
            OnGameFinished?.Invoke();
        }
    }
}