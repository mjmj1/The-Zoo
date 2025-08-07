using UnityEngine;

namespace Players
{
    public class PlayerSfx : MonoBehaviour
    {
        [SerializeField] internal AudioClip walkClip;
        [SerializeField] internal AudioClip jumpClip;
        [SerializeField] internal AudioClip attackClip;
        [SerializeField] internal AudioClip hitClip;
        [SerializeField] internal AudioClip spinClip;

        private AudioSource src;

        void Awake()
        {
            src = GetComponent<AudioSource>();
        }
    }
}
