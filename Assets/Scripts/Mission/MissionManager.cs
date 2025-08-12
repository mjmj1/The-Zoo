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

        private int hiderCountInitial;
        private int capturedCount;

        private void Awake()
        {
            instance = this;
        }

        public void InitializeHiderCount()
        {
            hiderCountInitial = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var pe = client.PlayerObject?.GetComponent<PlayerEntity>();
                if (pe != null && pe.role.Value == PlayerEntity.Role.Hider)
                    hiderCountInitial++;
            }
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
