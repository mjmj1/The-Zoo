using UnityEngine;

namespace Players
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 5, -10); // 카메라 위치 오프셋
        public float smoothSpeed = 5f; // 이동 속도

        private void LateUpdate()
        {
            if (!target) return;

            // 목표 위치 계산
            var desiredPosition = target.position + offset;

            // 부드러운 이동 적용
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // 항상 플레이어를 바라보도록 설정
            transform.LookAt(target);
        }
    }
}
