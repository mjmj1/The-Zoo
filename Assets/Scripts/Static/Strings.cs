using System;
using UnityEngine;

namespace Static
{
    public class Strings : MonoBehaviour
    {
        public static string PLAYER = "Player";
        public static string PLAYERNAME = "PlayerName";
        
        public static string GenerateRandomProfileName()
        {
            return "User_" + Guid.NewGuid().ToString("N")[..8];
        }
        
        public static string GenerateRandomSessionId()
        {
            return "Session_" + Guid.NewGuid().ToString("N")[..8];
        }
    }
}
