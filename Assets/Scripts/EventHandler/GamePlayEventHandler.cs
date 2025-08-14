using System;

namespace EventHandler
{
    internal static class GamePlayEventHandler
    {
        internal static event Action<string, bool> PlayerReady;
        internal static event Action PlayerLogin;
        internal static event Action PlayerAttack;
        internal static event Action<bool, bool, int> CheckInteractable;
        internal static event Action<string> UIChanged;
        internal static event Action NpcDeath;
        internal static event Action PlayerPickup;
        internal static event Action<bool> PlayerSpined;

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
        internal static void OnUIChanged(string name)
        {
            UIChanged?.Invoke(name);
        }

        internal static void OnNpcDeath()
        {
            NpcDeath?.Invoke();
        }

        internal static void OnPlayerPickup()
        {
            PlayerPickup?.Invoke();
        }
        internal static void OnPlayerSpined(bool value)
        {
            PlayerSpined?.Invoke(value);
        }
    }
}