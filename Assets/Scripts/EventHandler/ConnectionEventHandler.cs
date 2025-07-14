using System;

namespace EventHandler
{
    static class ConnectionEventHandler
    {
        internal static event Action OnSessionConnectStart;
        internal static event Action OnSessionConnected;
        internal static event Action OnSessionDisconnected;

        internal static void SessionConnectStart()
        {
            OnSessionConnectStart?.Invoke();
        }
        
        internal static void SessionDisconnected()
        {
            OnSessionDisconnected?.Invoke();
        }

        internal static void SessionConnected()
        {
            OnSessionConnected?.Invoke();
        }
    }
}