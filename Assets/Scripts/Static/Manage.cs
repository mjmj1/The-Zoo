using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Static
{
    public abstract class Manage
    {
        public static ISession Session()
        {
            return GameManager.Instance.connectionManager.Session;
        }

        public static string LocalPlayerId()
        {
            return AuthenticationService.Instance.PlayerId;
        }
        
        public static ulong LocalClientId()
        {
            return NetworkManager.Singleton.LocalClientId;
        }
    }
}