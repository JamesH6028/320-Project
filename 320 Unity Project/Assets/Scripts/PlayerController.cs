using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _minSnapAngle;

    [SerializeField]
    private Vector3 _groundingCheckDistance;
    [SerializeField]
    private Vector3 _canJumpCheckDistance;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private PhysicsMaterial _zeroFrictionMaterial;

    [SerializeField]
    private Collider _playerCollider;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform _lookPoint;

    private Vector2 _moveInput;
    private Vector2 _prevMoveInput;
    private Rigidbody _rb;

    private Vector2 _lookMin = new Vector2(.25f, -.25f);
    private Vector2 _lookMax = new Vector2(2.25f, -2.25f);
    private Vector2 _lookStep;
    private float _zoomCount = 50.0f;
    private float _zoomMax = 50.0f;

    private bool _grounded;
    private bool _canJump;

    void Start()
    {
        _grounded = true;
        _canJump = false;
        _prevMoveInput = Vector2.zero;
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        _lookStep = new Vector2((_lookMax.x - _lookMin.x) / _zoomMax * -1.0f, (_lookMax.y - _lookMin.y) / _zoomMax * -1.0f);
        Zoom();
    }

    private void FixedUpdate()
    {
        // check if grounded
        SetGrounded(GroundedRayCasts());
        // check if can jump
        SetCanJump(CanJumpRayCasts());

        // rotation
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation;
        if (_prevMoveInput == Vector2.zero || 
            Quaternion.Angle(transform.rotation, desiredRot) >= _minSnapAngle)
        {
            rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRot,
                360);
        }
        else
        {
            rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRot,
                _rotationSpeed * Time.fixedDeltaTime);
        }

        // position
        Vector3 position = transform.position;
        position += transform.forward * Vector2.ClampMagnitude(_moveInput, 1).magnitude * _moveSpeed * Time.fixedDeltaTime;
        if (_moveInput != Vector2.zero)
        {
            _rb.MoveRotation(rotation);
            _rb.MovePosition(position);
        }

        // if (_grounded && !_jumping)
        // {
        //     //_rb.linearVelocity = Vector3.zero;

        //     // keep player on ground for slopes
        //     // RaycastHit hit;
        //     // if (Physics.Raycast(position, -transform.up, out hit, _groundingOffset))
        //     // {
        //     //     position.y = hit.point.y;
        //     //     position.y += _groundingOffset;
        //     // }
        // }

        // only updates when needed
        

        _prevMoveInput = _moveInput;
    }

    private bool GroundedRayCasts()
    {
        return Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * _groundingCheckDistance.z,
                -_playerCollider.transform.up,
                _groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * -_groundingCheckDistance.z,
                -_playerCollider.transform.up,
                _groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * _groundingCheckDistance.x,
                -_playerCollider.transform.up,
                _groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * -_groundingCheckDistance.x,
                -_playerCollider.transform.up,
                _groundingCheckDistance.y
        );
    }

    private bool CanJumpRayCasts()
    {
        return Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * _canJumpCheckDistance.z,
                -_playerCollider.transform.up,
                _canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * -_canJumpCheckDistance.z,
                -_playerCollider.transform.up,
                _canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * _canJumpCheckDistance.x,
                -_playerCollider.transform.up,
                _canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * -_canJumpCheckDistance.x,
                -_playerCollider.transform.up,
                _canJumpCheckDistance.y);
    }

    private void SetGrounded(bool grounded)
    {
        if (_grounded == grounded)
        {
            return;
        }

        _grounded = grounded;
        // so running into walls has zero friction when in the air
        _playerCollider.material = grounded ? null : _zeroFrictionMaterial;
    }

    private void SetCanJump(bool canJump)
    {
        if (_canJump == canJump)
        {
            return;
        }

        _canJump = canJump;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _canJump)
        {
            _rb.linearVelocity = new Vector3(_rb.angularVelocity.x, 0, _rb.angularVelocity.x);
            _rb.AddForce(transform.up * _jumpForce);
            SetCanJump(false);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {

    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        float zoom = context.ReadValue<Vector2>().y;
        if (zoom == 0.0f) return;
        _zoomCount += zoom;
        if (_zoomCount > _zoomMax)
        {
            _zoomCount = _zoomMax;
            return;
        }
        else if (_zoomCount < 0.0f)
        {
            _zoomCount = 0.0f;
            return;
        }
        Zoom();
    }

    private void Zoom()
    {
        float lookY = (_lookStep.x * _zoomCount) + _lookMax.x;
        float lookZ = (_lookStep.y * _zoomCount) + _lookMax.y;
        _lookPoint.SetLocalPositionAndRotation(new Vector3(0.0f, lookY, lookZ), Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * _canJumpCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * _canJumpCheckDistance.z + -_playerCollider.transform.up * _canJumpCheckDistance.y
            );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * -_canJumpCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * -_canJumpCheckDistance.z + -_playerCollider.transform.up * _canJumpCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * _canJumpCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * _canJumpCheckDistance.x + -_playerCollider.transform.up * _canJumpCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * -_canJumpCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * -_canJumpCheckDistance.x + -_playerCollider.transform.up * _canJumpCheckDistance.y
        );

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * _groundingCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * _groundingCheckDistance.z + -_playerCollider.transform.up * _groundingCheckDistance.y
            );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * -_groundingCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * -_groundingCheckDistance.z + -_playerCollider.transform.up * _groundingCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * _groundingCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * _groundingCheckDistance.x + -_playerCollider.transform.up * _groundingCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * -_groundingCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * -_groundingCheckDistance.x + -_playerCollider.transform.up * _groundingCheckDistance.y
        );
    }
}
