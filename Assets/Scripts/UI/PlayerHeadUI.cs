using UnityEngine;

namespace UI
{
    public class PlayerHeadUI : MonoBehaviour
    {
        Camera cam;

        void Awake() => cam = Camera.main;

        void LateUpdate()
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.transform.position);
        }
    }
}
