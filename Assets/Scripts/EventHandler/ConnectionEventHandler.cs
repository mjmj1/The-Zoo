using System;

namespace EventHandler
{
    static class ConnectionEventHandler
    {
        internal static event Action SessionConnectStart;
        internal static event Action SessionConnected;
        internal static event Action SessionDisconnected;
        internal static event Action ConnectionFailed;

        internal static void OnSessionConnectStart()
        {
            SessionConnectStart?.Invoke();
        }
        
        internal static void OnSessionDisconnected()
        {
            SessionDisconnected?.Invoke();
        }

        internal static void OnSessionConnected()
        {
            SessionConnected?.Invoke();
        }

        internal static void OnConnectionFailed()
        {
            ConnectionFailed?.Invoke();
        }
    }
}