using Gameplay;
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

        //public void OnHiderCaptured()
        //{
        //    capturedCount++;

        //    float norm = (float)capturedCount / hiderCountInitial;
        //    state.SetProgressRpc(TeamRole.Seeker, norm);
        //    Debug.Log($"[Seeker Progress] {capturedCount}/{hiderCountInitial} ({norm:P})");
        //}

        //public void OnFruitCollected()
        //{
        //    fruitCollected++;

        //    float norm = (float)fruitCollected / Mathf.Max(1, fruitTotal);
        //    state.SetProgressRpc(TeamRole.Hider, norm);
        //    Debug.Log($"[Hider Progress] {fruitCollected}/{fruitTotal} ({norm:P0})");
        //}
        private void OnHiderCaptured_Server()
        {
            capturedCount = Mathf.Min(capturedCount + 1, hiderCountInitial);
            if (state)
            {
                float norm = (float)capturedCount / hiderCountInitial;
                state.SetProgressRpc(TeamRole.Seeker, norm);
            }
            // Debug.Log($"[Seeker Progress] {capturedCount}/{hiderCountInitial}");
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
            // Debug.Log($"[Hider Progress] {fruitCollected}/{fruitTotal}");
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void OnFruitCollectedRpc(RpcParams _ = default)
        {
            OnFruitCollected_Server();
        }
    }
}
