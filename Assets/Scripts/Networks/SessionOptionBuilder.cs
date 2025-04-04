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
        private int _maxPlayers = 8;
        private string _name = GetRandomSessionName();
        private string _password;

        public SessionOptionBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public SessionOptionBuilder MaxPlayers(int maxPlayers)
        {
            _maxPlayers = maxPlayers;
            return this;
        }

        public SessionOptionBuilder Password(string password = null)
        {
            _password = password;

            if (password != null)
                _sessionProperties.Add(PASSWORD, new SessionProperty(password, VisibilityPropertyOptions.Private));
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
                MaxPlayers = _maxPlayers,
                Password = _password,
                IsPrivate = _isPrivate,
                PlayerProperties = _playerProperties
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