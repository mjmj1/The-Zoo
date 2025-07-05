using System;
using Networks;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace UI.GameSetup
{
    public class GameSetupController : MonoBehaviour
    {
        public GameOptionField<bool> IsPrivate { get; private set; }
        public GameOptionField<string> Password { get; private set; }
        public GameOptionField<int> PlayerSlot { get; private set; }
        public GameOptionField<string> SessionName { get; private set; }

        public void Reset()
        {
            IsPrivate.Reset();
            Password.Reset();
            SessionName.Reset();
            PlayerSlot.Reset();
        }

        public void Start()
        {
            if (NetworkManager.Singleton.CurrentSessionOwner !=
                NetworkManager.Singleton.LocalClientId) return;

            Initialize();
        }

        public void Initialize()
        {
            var info = ConnectionManager.Instance.CurrentSession;

            IsPrivate = new GameOptionField<bool>(info.IsPrivate);
            SessionName = new GameOptionField<string>(info.Name);

            info.Properties.TryGetValue(Util.PASSWORD, out var password);
            Password = password != null
                ? new GameOptionField<string>(password.Value)
                : new GameOptionField<string>(string.Empty);

            info.Properties.TryGetValue(Util.PLAYERSLOT, out var playerSlot);
            PlayerSlot = playerSlot != null
                ? new GameOptionField<int>(Convert.ToInt32(playerSlot.Value))
                : new GameOptionField<int>(8);

            print($"{IsPrivate.Original} {SessionName.Original} {Password.Original} {PlayerSlot.Original}");
        }

        public void Apply()
        {
            IsPrivate.Apply();
            Password.Apply();
            SessionName.Apply();
            PlayerSlot.Apply();
        }

        public void Save()
        {
            Apply();

            ConnectionManager.Instance.UpdateSessionAsync(SessionName, Password, IsPrivate,
                PlayerSlot);
        }
    }
}