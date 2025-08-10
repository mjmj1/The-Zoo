using UnityEngine;

namespace Players
{
    public class PlayerVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem hitEffectPrefab;

        internal void HitEffect()
        {
            hitEffectPrefab.Play();
        }
    }
}
