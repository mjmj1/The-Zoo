using Gameplay;
using GamePlay;
using Players;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : NetworkBehaviour
    {
        public static MissionManager instance;

        [SerializeField] private TeamProgressState state;

        [SerializeField] internal int hiderCountInitial;
        [SerializeField] internal int fruitTotal;
        private int capturedCount;
        private int fruitCollected;

        private void Awake()
        {
            instance = this;
            capturedCount = 0;
            fruitCollected = 0;
        }

        private void OnHiderCaptured_Server()
        {
            capturedCount = Mathf.Min(capturedCount + 1, hiderCountInitial);
            if (state)
            {
                float norm = (float)capturedCount / hiderCountInitial;
                state.SetProgressRpc(TeamRole.Seeker, norm);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnHiderCapturedRpc(RpcParams _ = default)
        {
            OnHiderCaptured_Server();
        }

        private void OnFruitCollected_Server()
        {
            fruitCollected = Mathf.Min(fruitCollected + 1, fruitTotal);
            if (state)
            {
                float norm = (float)fruitCollected / fruitTotal;
                state.SetProgressRpc(TeamRole.Hider, norm);
            }

            if (PlayManager.Instance.isGameStarted.Value && fruitCollected >= fruitTotal)
            {
                PlayManager.Instance.isGameStarted.Value = false;
                PlayManager.Instance.ShowResultRpc(false);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnFruitCollectedRpc(RpcParams _ = default)
        {
            OnFruitCollected_Server();
        }
    }
}
