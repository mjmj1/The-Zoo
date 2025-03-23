using Players;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 50f;
    public float mouseSensitivity = 3f;
    
    PlanetGravity _planetGravity;
    Transform _plenetCenter;
    Rigidbody _rb;
    
    Quaternion _previousRotation;
    
    void Start()
    {
        _planetGravity = FindAnyObjectByType<PlanetGravity>();
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        
        _plenetCenter = _planetGravity.gameObject.transform;
        _planetGravity.Subscribe(_rb);
        ConnectFollowCamera();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        LookAround();
        AlignToSurface();
    }

    void FixedUpdate()
    {
        CharacterMovement();
    }
    
    private void OnDestroy()
    {
        _planetGravity.Unsubscribe(_rb);
    }
    
    void ConnectFollowCamera()
    {
        var cam = FindAnyObjectByType<FollowCamera>();

        if (cam != null)
        {
            cam.target = transform;
        }
    }

    void CharacterMovement()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");
        
        var moveDirection = transform.forward * v + transform.right * h;
        moveDirection.Normalize();

        _rb.MovePosition(_rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime));
        
        if (h == 0 && v == 0)
        {
            _rb.linearVelocity = Vector3.zero;
            transform.rotation = _previousRotation;
        }
    }
    private void AlignToSurface()
    {
        var gravityDirection = (transform.position - _plenetCenter.position).normalized;
        
        var targetRotation = Quaternion.FromToRotation(
            transform.up, gravityDirection) * transform.rotation;
        _previousRotation = Quaternion.Slerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = _previousRotation;
    }
    
    void LookAround()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }
}
