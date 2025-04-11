using System.Collections.Generic;
using Static;
using UnityEngine;
using static Static.Strings;

namespace UI.GameSetup
{
    public class SetupData<T>
    {
        public T Cache;
        public T Origin;

        public bool IsDirty => !EqualityComparer<T>.Default.Equals(Origin, Cache);

        public SetupData(T origin)
        {
            Origin = origin;
            Cache = default;
        }

        public void Set(T value)
        {
            Cache = value;

            Debug.Log($"Set({Cache}) IsDirty : {IsDirty}");
        }

        public void Save()
        {
            if (IsDirty)
                Origin = Cache;

            Reset();
        }

        public void Reset()
        {
            Cache = default;
        }

        public override string ToString()
        {
            return $"{Cache} {Origin} {IsDirty}";
        }
    }

    public class GameSetupController : MonoBehaviour
    {
        public string Code { get; private set; }
        public SetupData<bool> IsPrivate { get; private set; }
        public SetupData<string> Password { get; private set; }
        public SetupData<int> PlayerSlot { get; private set; }
        public SetupData<string> SessionName { get; private set; }

        public bool HasDirty =>
            SessionName.IsDirty ||
            Password.IsDirty ||
            IsPrivate.IsDirty ||
            PlayerSlot.IsDirty;

        private void Start()
        {
            var session = Manage.Session();

            Code = session.Code;

            SessionName = new SetupData<string>(session.Name);
            IsPrivate = new SetupData<bool>(session.IsPrivate);

            Password = new SetupData<string>(
                session.Properties.TryGetValue(PASSWORD, out var pwProp) ? pwProp.Value : ""
            );

            var slot = session.Properties.TryGetValue(PLAYERSLOT, out var slotProp)
                ? slotProp.Value
                : session.MaxPlayers.ToString();

            PlayerSlot = new SetupData<int>(int.Parse(slot));
        }

        public void Save()
        {
            var sessionNameCache = SessionName.IsDirty ? SessionName.Cache : null;
            var passwordCache = Password.IsDirty ? Password.Cache : null;
            var isPrivateCache = IsPrivate.IsDirty ? IsPrivate.Cache : (bool?)null;
            var playerSlotCache = PlayerSlot.IsDirty ? PlayerSlot.Cache : (int?)null;

            Manage.ConnectionManager().UpdateSessionAsync(
                sessionNameCache,
                passwordCache,
                isPrivateCache,
                playerSlotCache
            );

            SessionName.Save();
            Password.Save();
            IsPrivate.Save();
            PlayerSlot.Save();
        }

        public void Clear()
        {
            SessionName.Reset();
            Password.Reset();
            IsPrivate.Reset();
            PlayerSlot.Reset();
        }
    }
}