using UnityEngine;

namespace Players
{
    public class PlayerVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem hitEffect;

        internal void HitEffect()
        {
            hitEffect.Play();
        }
    }
}