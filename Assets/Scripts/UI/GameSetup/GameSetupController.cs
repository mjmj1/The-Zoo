using Static;
using UnityEngine;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        public string SessionName {get; set;}
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
        public int MaxPlayers { get; set; }

        public void Save()
        {
            Manage.ConnectionManager().UpdateSessionAsync(SessionName, Password, MaxPlayers, IsPrivate);
        }
    }
}