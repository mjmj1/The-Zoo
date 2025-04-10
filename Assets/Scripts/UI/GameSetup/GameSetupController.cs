using Static;
using UnityEngine;
using static Static.Strings;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        public string SessionName { get; set; }
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
        public int PlayerSlot { get; set; }

        private void Start()
        {
            var session = Manage.Session();
            
            SessionName = session.Name;
            Password = session.Properties[PASSWORD].Value;
            IsPrivate = session.IsPrivate;
            PlayerSlot = session.MaxPlayers;
        }

        public void Clear()
        {
            SessionName = null;
            Password = null;
        }

        public void Save()
        {
            Manage.ConnectionManager().UpdateSessionAsync(SessionName, Password, IsPrivate, PlayerSlot);

            Clear();
        }
    }
}