using UnityEngine;

namespace Maps
{
    public class TrainTorusWorld : MonoBehaviour
    {
        public float sizeX = 50f;
        public float sizeZ = 50f;

        public float HalfX => sizeX * 0.5f;
        public float HalfZ => sizeZ * 0.5f;

        public Vector3 WrapXZ(Vector3 pos)
        {
            // 맵의 중심을 고려하여 상대 위치 계산
            var relativePos = pos - transform.position;
            relativePos.x = Mathf.Repeat(relativePos.x + HalfX, sizeX) - HalfX;
            relativePos.z = Mathf.Repeat(relativePos.z + HalfZ, sizeZ) - HalfZ;
            return transform.position + relativePos;
        }
    }
}
