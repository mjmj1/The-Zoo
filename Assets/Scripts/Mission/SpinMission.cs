using EventHandler;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class SpinMission : NetworkBehaviour
    {
        private bool isSpinning;
        private float spinTimer;

        private void OnEnable()
        {
            GamePlayEventHandler.PlayerSpined += OnPlayerSpinedChange;
        }

        private void OnDisable()
        {
            GamePlayEventHandler.PlayerSpined -= OnPlayerSpinedChange;
        }

        private void OnPlayerSpinedChange(bool isSpinning)
        {
            this.isSpinning = isSpinning;
            if (!isSpinning)
            {
                spinTimer = 0f;
            }
        }

        private void Update()
        {
            if (!isSpinning) return;

            spinTimer += Time.deltaTime;
            if (!(spinTimer >= 1.0f)) return;

            spinTimer = 0f;

            if (MissionManager.instance != null)
            {
                MissionManager.instance.AddSpinCountServerRpc();
            }
        }
    }
}
