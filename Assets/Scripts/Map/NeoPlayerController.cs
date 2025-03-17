using Static;
using System.Collections;
using UnityEngine;

public class NeoPlayerController : MonoBehaviour
{
    public Transform WorldCenter;

    public float moveSpeed = 10f;
    public float mouseSensitivity = 10f;

    private Rigidbody _rb;

    private float _horizontal;
    private float _vertical;

    private float _yaw;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (WorldCenter == null)
        {
            StartCoroutine(GetWorldObject());
        }
        else
        {
            align_to_surface();

            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity; ;

            _horizontal = Input.GetAxisRaw("Horizontal");
            _vertical = Input.GetAxisRaw("Vertical");

            if (_horizontal == 0 && _vertical == 0) _rb.linearVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        handle_rotation();

        var moveDirection = (transform.forward * _vertical + transform.right * _horizontal).normalized;
        _rb.MovePosition(_rb.position + moveDirection * (moveSpeed * Time.deltaTime));
    }

    void handle_rotation()
    {
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
    }

    void align_to_surface()
    {
        var gravityDirection = (transform.position - WorldCenter.position).normalized;
        var targetRotation = Quaternion.FromToRotation(transform.up, gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rb.maxLinearVelocity * Time.deltaTime);
    }

    IEnumerator GetWorldObject()
    {
        yield return new WaitForSeconds(1);
        WorldCenter = GameObject.FindWithTag(Strings.WORLD).gameObject.GetComponent<Transform>();
    }
}