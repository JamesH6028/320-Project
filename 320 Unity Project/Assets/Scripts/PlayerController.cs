using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _groundingOffset;
    [SerializeField]
    private Vector3 _groundingCastHalfSize;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private PhysicsMaterial _zeroFrictionMaterial;
    [SerializeField]
    private Collider _playerCollider;
    [SerializeField]
    private Camera _camera;

    private Vector2 _moveInput;
    private Rigidbody _rb;

    private bool _grounded;
    private bool _jumping;

    void Start()
    {
        _grounded = true;
        _jumping = false;
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        // check if grounded
        SetGrounded(Physics.BoxCast(_playerCollider.transform.position, _groundingCastHalfSize, -_playerCollider.transform.up, _playerCollider.transform.rotation, _groundingOffset));

        // rotation
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        Quaternion cameraFwrdRot = Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRot * cameraFwrdRot,
            _rotationSpeed * Time.fixedDeltaTime);

        // position
        Vector3 position = transform.position;
        position += transform.forward * Vector2.ClampMagnitude(_moveInput, 1).magnitude * _moveSpeed * Time.fixedDeltaTime;

        if (_grounded && !_jumping)
        {
            _rb.linearVelocity = Vector3.zero;

            // keep player on ground for slopes
            // RaycastHit hit;
            // if (Physics.Raycast(position, -transform.up, out hit, _groundingOffset))
            // {
            //     position.y = hit.point.y;
            //     position.y += _groundingOffset;
            // }
        }

        // only updates when needed
        if (_moveInput != Vector2.zero)
        {
            _rb.MoveRotation(rotation);
            _rb.MovePosition(position);
        }
    }

    private void SetGrounded(bool grounded)
    {
        if(_grounded == grounded)
        {
            return;
        }

        _grounded = grounded;
        // so running into walls has zero friction when in the air
        _playerCollider.material = grounded ? null: _zeroFrictionMaterial;
        // if on the ground, the player is not jumping
        if (grounded)
        {
            SetJumping(false);
        }
    }

    private void SetJumping(bool jumping)
    {
        if(_jumping == jumping)
        {
            return;
        }

        _jumping = jumping;
        // so running into walls has zero friction when in the air
        _playerCollider.material = jumping ? _zeroFrictionMaterial : null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _grounded && !_jumping)
        {
            //SetGrounded(false);
            _rb.AddForce(transform.up * _jumpForce);
            SetJumping(true);
        }
    }

    void OnDrawGizmosSelected()
    {
        //_playerCollider.transform.position, Vector3.zero,-_playerCollider.transform.up, _playerCollider.transform.rotation, _groundingOffset
        Gizmos.color = Color.red;
        Gizmos.matrix = 
        Matrix4x4.Translate(_playerCollider.transform.position) * 
        Matrix4x4.Rotate(_playerCollider.transform.rotation) * 
        Matrix4x4.Translate(-_playerCollider.transform.position);
        Gizmos.DrawWireCube(
            _playerCollider.transform.position + -_playerCollider.transform.up * _groundingOffset,
            _groundingCastHalfSize * 2
        );
    }
}
