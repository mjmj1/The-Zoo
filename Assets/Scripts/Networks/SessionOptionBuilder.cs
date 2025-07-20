using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Utils;

namespace Networks
{
    public class SessionOptionBuilder
    {
        private readonly Dictionary<string, PlayerProperty> _playerProperties = new();
        private readonly Dictionary<string, SessionProperty> _sessionProperties = new();

        private bool _isPrivate;

        private string _name;
        private string _password;

        private int _playerSlot = 8;

        public SessionOptionBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public SessionOptionBuilder Password(string password = null)
        {
            _password = password;

            var prop = new SessionProperty(password, VisibilityPropertyOptions.Private);

            if (!_sessionProperties.TryAdd(Util.PASSWORD, prop)) _sessionProperties[Util.PASSWORD] = prop;

            return this;
        }

        public SessionOptionBuilder PlayerSlot(int playerSlot)
        {
            _playerSlot = playerSlot;

            var prop = new SessionProperty(playerSlot.ToString());

            if (!_sessionProperties.TryAdd(Util.PLAYERSLOT, prop)) _sessionProperties[Util.PLAYERSLOT] = prop;

            return this;
        }

        public SessionOptionBuilder IsPrivate(bool isPrivate = false)
        {
            _isPrivate = isPrivate;
            return this;
        }

        public SessionOptionBuilder PlayerProperty(string key, string value)
        {
            var prop = new PlayerProperty(value, VisibilityPropertyOptions.Member);

            _playerProperties.Add(key, prop);

            return this;
        }

        public SessionOptions BuildCreate()
        {
            return new SessionOptions
            {
                Name = _name,
                Password = _password,
                MaxPlayers = 8,
                IsPrivate = _isPrivate,
                PlayerProperties = _playerProperties,
                SessionProperties = _sessionProperties
            }.WithDistributedAuthorityNetwork();
        }

        public JoinSessionOptions BuildJoin()
        {
            return new JoinSessionOptions
            {
                Password = _password,
                PlayerProperties = _playerProperties
            };
        }
    }
}