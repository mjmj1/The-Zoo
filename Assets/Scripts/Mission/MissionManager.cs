using Gameplay;
using Players;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager instance;

        [SerializeField] private TeamProgressState state;

        [SerializeField] internal int hiderCountInitial;
        private int capturedCount;

        private void Awake()
        {
            instance = this;
            capturedCount = 0;
        }

        public void OnHiderCaptured()
        {
            capturedCount++;
            
            float norm = (float)capturedCount / hiderCountInitial;
            state.SetProgressRpc(TeamRole.Seeker, norm);
            Debug.Log($"[Seeker Progress] {capturedCount}/{hiderCountInitial} ({norm:P})");
        }
    }
}
