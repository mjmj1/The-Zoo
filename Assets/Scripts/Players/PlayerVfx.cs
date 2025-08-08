using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffectPrefab;

    public void HitEffect()
    {
        hitEffectPrefab.Play();
    }
}
