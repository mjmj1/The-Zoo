using Players;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    PlanetGravity planetGravity;
    Transform plenetCenter;
    Rigidbody rb;
    Vector3 motion;
    
    void Start()
    {
        planetGravity = FindAnyObjectByType<PlanetGravity>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        
        plenetCenter = planetGravity.gameObject.transform;
        planetGravity.Subscribe(rb);
        ConnectFollowCamera();
    }

    void Update()
    {
        motion = Vector3.zero;
        motion.x = Input.GetAxisRaw("Horizontal");
        motion.z = Input.GetAxisRaw("Vertical");
        
        if (motion.magnitude > 0)
        {
            transform.position += motion * (moveSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        AlignToSurface();
    }
    
    private void OnDestroy()
    {
        planetGravity.Unsubscribe(rb);
    }
    
    void ConnectFollowCamera()
    {
        var cam = FindAnyObjectByType<FollowCamera>();

        if (cam != null)
        {
            cam.target = transform;
        }
    }
    
    private void AlignToSurface()
    {
        var gravityDirection = (transform.position - plenetCenter.position).normalized;
        var targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
