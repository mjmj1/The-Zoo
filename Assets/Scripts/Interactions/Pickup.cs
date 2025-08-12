using Scriptable;
using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public class Pickup : NetworkBehaviour
    {
        public NetworkVariable<bool> consumed = new();

        [SerializeField] private SfxData sfxData;
        [SerializeField] private ParticleSystem vfxData;

        public void PickUp()
        {
            AudioManager.Instance.PlaySfx(sfxData.clip, transform.position, sfxData.volume, sfxData.pitch);

            vfxData.Play();
        }
    }
}