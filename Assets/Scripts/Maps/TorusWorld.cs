using UnityEngine;

public class TorusWorld : MonoBehaviour
{
    public static TorusWorld Instance { get; private set; }
    
    public float sizeX = 30f;
    public float sizeZ = 30f;

    public float HalfX => sizeX * 0.5f;
    public float HalfZ => sizeZ * 0.5f;

    internal InfinitePlaneVisual PlaneVisual;

    void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        PlaneVisual = GetComponent<InfinitePlaneVisual>();
    }

    public Vector3 WrapXZ(Vector3 pos)
    {
        pos.x = Mathf.Repeat(pos.x + HalfX, sizeX) - HalfX;
        pos.z = Mathf.Repeat(pos.z + HalfZ, sizeZ) - HalfZ;
        return pos;
    }
}
