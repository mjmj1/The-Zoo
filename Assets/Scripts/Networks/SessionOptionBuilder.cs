using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Utils;

namespace Networks
{
    public class SessionOptionBuilder
    {
        private readonly Dictionary<string, PlayerProperty> playerProperties = new();
        private readonly Dictionary<string, SessionProperty> sessionProperties = new();

        private bool isPrivate;

        private string name;
        private string password;

        private int playerSlot = 8;

        public SessionOptionBuilder Name(string _name)
        {
            this.name = _name;
            return this;
        }

        public SessionOptionBuilder Password(string _password = null)
        {
            this.password = _password;

            var prop = new SessionProperty(_password, VisibilityPropertyOptions.Private);

            if (!sessionProperties.TryAdd(Util.PASSWORD, prop)) sessionProperties[Util.PASSWORD] = prop;

            return this;
        }

        public SessionOptionBuilder PlayerSlot(int _playerSlot)
        {
            this.playerSlot = _playerSlot;

            var prop = new SessionProperty(_playerSlot.ToString());

            if (!sessionProperties.TryAdd(Util.PLAYERSLOT, prop)) sessionProperties[Util.PLAYERSLOT] = prop;

            return this;
        }

        public SessionOptionBuilder IsPrivate(bool _isPrivate = false)
        {
            this.isPrivate = _isPrivate;
            return this;
        }

        public SessionOptionBuilder PlayerProperty(string key, string value)
        {
            var prop = new PlayerProperty(value, VisibilityPropertyOptions.Member);

            playerProperties.Add(key, prop);

            return this;
        }

        public SessionOptions BuildCreate()
        {
            return new SessionOptions
            {
                Name = name,
                Password = password,
                MaxPlayers = 4,
                IsPrivate = isPrivate,
                PlayerProperties = playerProperties,
                SessionProperties = sessionProperties
            }.WithDistributedAuthorityNetwork();
        }

        public JoinSessionOptions BuildJoin()
        {
            return new JoinSessionOptions
            {
                Password = password,
                PlayerProperties = playerProperties
            };
        }
    }
}