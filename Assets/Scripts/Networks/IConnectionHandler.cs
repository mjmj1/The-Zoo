using System;

namespace Networks
{
    public interface IConnectionHandler
    {
        event Action OnSessionConnecting;
        event Action OnSessionDisconnected;
    }
}