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
            JoinByCode
        }

        public ConnectionType Type { get; private set; }
        public string SessionName { get; private set; }
        public string PlayerName { get; private set; }
        public string Code { get; private set; }
        public string Password { get; private set; }
        public bool IsPrivate { get; private set; }
        public int PlayerSlot { get; private set; }

        public ConnectionData(ConnectionType type, string code = null, string password = null,
            string sessionName = null, bool isPrivate = false, int playerSlot = 8)
        {
            Type = type;
            SessionName = sessionName ?? GetRandomSessionName();
            PlayerName = PlayerPrefs.GetString(PLAYERNAME);
            Code = code;
            Password = password;
            IsPrivate = isPrivate;
            PlayerSlot = playerSlot;
        }
    }
}