using Static;
using UnityEngine;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        private bool? _isPrivate;
        private string _password;
        private int? _playerSlot;
        private string _sessionName;

        public string SessionName
        {
            get => _sessionName;
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _sessionName = value;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _password = value;
            }
        }

        public bool? IsPrivate
        {
            get => _isPrivate;
            set
            {
                if (value == null) return;

                _isPrivate = value;
            }
        }

        public int? PlayerSlot
        {
            get => _playerSlot;
            set
            {
                if (value == null) return;

                _playerSlot = value;
            }
        }

        private void Start()
        {
            var session = Manage.Session();

            _sessionName = session.Name;
            IsPrivate = session.IsPrivate;
        }

        public void Clear()
        {
            _sessionName = null;
            IsPrivate = null;
            PlayerSlot = null;
            Password = null;
        }

        public void Save()
        {
            Manage.ConnectionManager().UpdateSessionAsync(SessionName, Password, IsPrivate, PlayerSlot);

            Clear();
        }
    }
}