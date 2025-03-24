#if UNITY_EDITOR
using Players;
using UI;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     The custom editor for the <see cref="PlayerCubeController" /> component.
/// </summary>
[CustomEditor(typeof(PlayerCubeController), true)]
public class PlayerCubeControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_MouseSensitivity;
    private SerializedProperty m_MoveSpeed;
    private SerializedProperty m_RotationSpeed;

    public override void OnEnable()
    {
        m_MoveSpeed = serializedObject.FindProperty(nameof(PlayerCubeController.moveSpeed));
        m_RotationSpeed = serializedObject.FindProperty(nameof(PlayerCubeController.rotationSpeed));
        m_MouseSensitivity = serializedObject.FindProperty(nameof(PlayerCubeController.mouseSensitivity));
        base.OnEnable();
    }

    private void DisplayPlayerCubeControllerProperties()
    {
        EditorGUILayout.PropertyField(m_MoveSpeed);
        EditorGUILayout.PropertyField(m_RotationSpeed);
        EditorGUILayout.PropertyField(m_MouseSensitivity);
    }

    public override void OnInspectorGUI()
    {
        var playerCubeController = target as PlayerCubeController;

        void SetExpanded(bool expanded)
        {
            playerCubeController.playerCubeControllerPropertiesVisible = expanded;
        }

        ;
        DrawFoldOutGroup<PlayerCubeController>(playerCubeController.GetType(), DisplayPlayerCubeControllerProperties,
            playerCubeController.playerCubeControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif

public class PlayerCubeController : NetworkTransform
{
#if UNITY_EDITOR
    // These bool properties ensure that any expanded or collapsed property views
    // within the inspector view will be saved and restored the next time the
    // asset/prefab is viewed.
    public bool playerCubeControllerPropertiesVisible;
#endif
    public float moveSpeed = 5f;
    public float rotationSpeed = 50f;
    public float mouseSensitivity = 3f;

    private PlanetGravity _planetGravity;
    private Transform _plenetCenter;

    private Quaternion _previousRotation;
    private Rigidbody _rb;


    private void Start()
    {
        if (!IsOwner) return;
        
        _planetGravity = FindAnyObjectByType<PlanetGravity>();
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        ConnectFollowCamera();

        if (!_planetGravity) return;

        _plenetCenter = _planetGravity.gameObject.transform;
        _planetGravity.Subscribe(_rb);
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        if (!_planetGravity) return;
        
        if (UIManager.IsCursorLocked()) return;
        
        LookAround();
        AlignToSurface();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        if (!_planetGravity) return;
        
        if (UIManager.IsCursorLocked()) return;
        
        CharacterMovement();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        _planetGravity.Unsubscribe(_rb);
    }
    
    private void ConnectFollowCamera()
    {
        var cam = FindAnyObjectByType<FollowCamera>();

        if (cam != null) cam.target = transform;
    }

    private void CharacterMovement()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        var moveDirection = transform.forward * v + transform.right * h;
        moveDirection.Normalize();

        _rb.MovePosition(_rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime));

        if (h != 0 || v != 0) return;
        
        _rb.linearVelocity = Vector3.zero;
        transform.rotation = _previousRotation;
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

    private void LookAround()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }
}