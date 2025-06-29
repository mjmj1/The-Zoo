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
        public string PlayerName { get; private set; }
        public string Code { get; private set; }
        public string Password { get; private set; }
        public string SessionName { get; private set; }
        public int PlayerSlot { get; private set; }
        public bool IsPrivate { get; private set; }

        public ConnectionData(ConnectionType type, string code = null, string password = null,
            string sessionName = null, int playerSlot = 8, bool isPrivate = false)
        {
            Type = type;
            PlayerName = "";
            Code = code;
            Password = password;
            SessionName = sessionName;
            PlayerSlot = playerSlot;
            IsPrivate = isPrivate;
        }
    }
}