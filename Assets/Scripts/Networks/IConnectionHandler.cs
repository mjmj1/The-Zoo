using System;

namespace Networks
{
    public interface IConnectionHandler
    {
        event Action OnSessionConnect;
        event Action OnSessionDisconnected;
    }
}