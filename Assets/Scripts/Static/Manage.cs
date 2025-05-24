using Networks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

namespace Static
{
    public abstract class Manage
    {
        public static ConnectionManager ConnectionManager()
        {
            return Networks.ConnectionManager.Instance;
        }

        public static ISession Session()
        {
            return Networks.ConnectionManager.Instance.ActiveSession;
        }

        public static string LocalPlayerId()
        {
            return AuthenticationService.Instance.PlayerId;
        }
    }
}