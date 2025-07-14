using System;

namespace EventHandler
{
    static class ConnectionEventHandler
    {
        internal static event Action OnSessionConnecting;
        internal static event Action OnSessionDisconnected;

        internal static void SessionConnecting()
        {
            OnSessionConnecting?.Invoke();
        }

        internal static void SessionDisconnected()
        {
            OnSessionDisconnected?.Invoke();
        }
    }
}