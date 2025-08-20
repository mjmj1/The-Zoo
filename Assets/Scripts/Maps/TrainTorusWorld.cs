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
            pos.x = Mathf.Repeat(pos.x + HalfX, sizeX) - HalfX;
            pos.z = Mathf.Repeat(pos.z + HalfZ, sizeZ) - HalfZ;
            return pos;
        }
    }
}
