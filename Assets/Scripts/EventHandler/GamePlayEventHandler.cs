using System;

namespace EventHandler
{
    internal static class GamePlayEventHandler
    {
        internal static event Action<string, bool> PlayerReady;
        internal static event Action PlayerLogin;
        internal static event Action PlayerAttack;
        internal static event Action<bool, bool, int> CheckInteractable;

        internal static void OnPlayerReady(string id, bool value)
        {
            PlayerReady?.Invoke(id, value);
        }

        internal static void OnPlayerLogin()
        {
            PlayerLogin?.Invoke();
        }

        internal static void OnPlayerAttack()
        {
            PlayerAttack?.Invoke();
        }

        internal static void OnCheckInteractable(bool value, bool isTarget, int count)
        {
            CheckInteractable?.Invoke(value, isTarget, count);
        }
    }
}