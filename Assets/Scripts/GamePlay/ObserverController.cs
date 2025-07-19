using UnityEngine;

namespace GamePlay
{
    public class ObserverController : MonoBehaviour
    {
        public float mouseSensitivity = 0.1f;
        public float minPitch = -10f;
        public float maxPitch = 20f;
        public float Pitch { get; private set; }

        private float moveSpeed;
    }
}