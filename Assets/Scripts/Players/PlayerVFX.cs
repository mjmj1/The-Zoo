using Scriptable;
using UnityEngine;
using UnityEngine.Serialization;

namespace Players
{
    public class PlayerVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem pickupEffect;

        internal void HitEffect()
        {
            hitEffect.Play();
        }

        public void PickupEffect()
        {
            pickupEffect.Play();
        }
    }
}
