using System.Collections.Generic;
using Unity.Services.Multiplayer;
using static Static.Strings;

namespace Networks
{
    public class SessionOptionBuilder
    {
        private readonly Dictionary<string, PlayerProperty> _playerProperties = new();
        private readonly Dictionary<string, SessionProperty> _sessionProperties = new();
        private bool _isLock;
        private bool _isPrivate;
        private int _playerSlot = 8;
        private string _name = GetRandomSessionName();
        private string _password;

        public SessionOptionBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public SessionOptionBuilder PlayerSlot(int playerSlot)
        {
            _playerSlot = playerSlot;

            if (!_sessionProperties.TryAdd(PLAYERSLOT,
                    new SessionProperty(playerSlot.ToString())))
                _sessionProperties[PLAYERSLOT] = new SessionProperty(playerSlot.ToString());

            return this;
        }

        public SessionOptionBuilder Password(string password = null)
        {
            _password = password;

            if (!_sessionProperties.TryAdd(PASSWORD,
                    new SessionProperty(password, VisibilityPropertyOptions.Private)))
                _sessionProperties[PASSWORD] = new SessionProperty(password, VisibilityPropertyOptions.Private);

            return this;
        }

        public SessionOptionBuilder IsPrivate(bool isPrivate = false)
        {
            _isPrivate = isPrivate;
            return this;
        }

        public SessionOptionBuilder IsLock(bool isLock = false)
        {
            _isLock = isLock;
            return this;
        }

        public SessionOptionBuilder PlayerProperty(string key, PlayerProperty property)
        {
            _playerProperties.Add(key, property);
            return this;
        }

        public SessionOptions BuildCreate()
        {
            return new SessionOptions
            {
                Name = _name,
                MaxPlayers = 8,
                Password = _password,
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