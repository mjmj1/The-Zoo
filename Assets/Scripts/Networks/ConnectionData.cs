using UnityEngine;
using static Static.Strings;

namespace Networks
{
    public struct ConnectionData
    {
        public enum ConnectionType
        {
            Quick,
            Create,
            JoinById,
            JoinByCode,
        }
        
        public ConnectionType Type;
        public string Code;
        public string Password;
        public string SessionName;
        public string PlayerName;

        public ConnectionData(ConnectionType type, string code = null, string password = null, string sessionName = null)
        {
            Type = type;
            Code = code;
            Password = password;
            SessionName = sessionName ?? GetRandomSessionName();
            PlayerName = PlayerPrefs.GetString(PLAYERNAME);
        }

        public override string ToString()
        {
            return $"{Type}-{Code}-{Password}-{SessionName}-{PlayerName}";
        }
    }
}