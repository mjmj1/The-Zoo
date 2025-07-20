using System;
using UnityEngine.InputSystem;

namespace EventHandler
{
    internal static class GamePlayEventHandler
    {
        internal static event Action<string, bool> PlayerReady;
        internal static event Action PlayerLogin;
        internal static event Action PlayerAttack;

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
    }
}