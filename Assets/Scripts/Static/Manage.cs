using Unity.Netcode;
using Unity.Services.Multiplayer;

namespace Static
{
    public abstract class Manage
    {
        public static ISession Session()
        {
            return GameManager.Instance.connectionManager.Session;
        }
    }
}