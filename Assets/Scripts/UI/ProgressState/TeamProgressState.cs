using Unity.Netcode;
using UnityEngine;

namespace ProgressState
{
    public enum TeamRole : byte
    {
        Hider = 0,
        Seeker = 1
    }

    public class TeamProgressState : NetworkBehaviour
    {
        [Range(0f, 1f)][SerializeField] private float initialHider = 0f;
        [Range(0f, 1f)][SerializeField] private float initialSeeker = 0f;

        public NetworkVariable<float> HiderProgress { get; } =
            new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> SeekerProgress { get; } =
            new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                HiderProgress.Value = Mathf.Clamp01(initialHider);
                SeekerProgress.Value = Mathf.Clamp01(initialSeeker);
            }
        }

        private void AddProgress_Server(TeamRole team, float delta)
        {
            switch (team)
            {
                case TeamRole.Hider:
                    HiderProgress.Value = Mathf.Clamp01(HiderProgress.Value + delta);
                    break;
                case TeamRole.Seeker:
                    SeekerProgress.Value = Mathf.Clamp01(SeekerProgress.Value + delta);
                    break;
            }
        }

        private void SetProgress_Server(TeamRole team, float v)
        {
            switch (team)
            {
                case TeamRole.Hider:
                    HiderProgress.Value = Mathf.Clamp01(v);
                    break;
                case TeamRole.Seeker:
                    SeekerProgress.Value = Mathf.Clamp01(v);
                    break;
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void AddProgressRpc(TeamRole team, float delta, RpcParams rpcParams = default)
        {
            AddProgress_Server(team, delta);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void SetProgressRpc(TeamRole team, float value01, RpcParams rpcParams = default)
        {
            SetProgress_Server(team, value01);
        }
    }
}
