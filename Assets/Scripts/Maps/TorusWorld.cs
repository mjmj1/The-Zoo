using UnityEngine;

namespace Maps
{
    public class TorusWorld : MonoBehaviour
    {
        public static TorusWorld Instance { get; private set; }
    
        public float sizeX = 50f;
        public float sizeZ = 50f;

        public float HalfX => sizeX * 0.5f;
        public float HalfZ => sizeZ * 0.5f;

        internal InfiniteTile tile;

        void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
        
            tile = GetComponent<InfiniteTile>();
        }

        public Vector3 WrapXZ(Vector3 pos)
        {
            pos.x = Mathf.Repeat(pos.x + HalfX, sizeX) - HalfX;
            pos.z = Mathf.Repeat(pos.z + HalfZ, sizeZ) - HalfZ;
            return pos;
        }
    }
}
