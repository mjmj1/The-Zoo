using Networks;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

namespace Static
{
    public abstract class Manage
    {
        public static ISession Session()
        {
            return ConnectionManager.instance.CurrentSession;
        }

        public static string LocalPlayerId()
        {
            return AuthenticationService.Instance.PlayerId;
        }
    }
}