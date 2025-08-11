using UnityEngine;

namespace Players
{
    public class PlayerVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem hitEffectPrefab;

        public void HitEffect()
        {
            hitEffectPrefab.Play();
        }
    }
}
