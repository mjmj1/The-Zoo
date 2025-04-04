using Static;
using UnityEditor;
using UnityEngine;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        public string SessionName {
            get => _sessionName ?? _prevSessionName;
            set
            {
                _sessionName = value;
                _prevSessionName = _sessionName;
            }
        }
        private string _prevSessionName;
        private string _sessionName;
        
        
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxPlayers { get; set; }

        private void Start()
        {
            var session = Manage.Session();
            
            _sessionName = session.Name;
            _prevSessionName = session.Name;
            
            
        }
        
        public void Save()
        {
            Manage.ConnectionManager().UpdateSessionAsync(SessionName, Password, MaxPlayers, IsPrivate);
        }
    }
}