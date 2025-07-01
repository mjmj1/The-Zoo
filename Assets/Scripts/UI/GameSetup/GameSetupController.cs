using Networks;
using UnityEngine;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        public string Code { get; set; }
        public bool? IsPrivate { get; set; }
        public string Password { get; set; }
        public int PlayerSlot { get; set; }
        public string SessionName { get; set; }

        public void Save()
        {
            ConnectionManager.instance.UpdateSessionAsync(SessionName, Password, IsPrivate, PlayerSlot);
        }

        public void Reset()
        {
            IsPrivate = null;
            Password = string.Empty;
            SessionName = string.Empty;
            PlayerSlot = -1;
        }
    }
}