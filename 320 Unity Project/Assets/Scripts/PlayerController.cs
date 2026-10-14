using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _backwardsMoveSpeed;
    [SerializeField]
    private float _backwardsRotationSpeed;
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
    private float _biteRayHeight;
    [SerializeField]
    private float _biteRayLength;
    [SerializeField]
    private LayerMask _raycastInteractableLayer;
    [SerializeField]
    private Vector3 _hingeAnchor;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform _lookPoint;

    private Vector2 _moveInput;
    private Vector2 _prevMoveInput;
    private Rigidbody _rb;
    private HingeJoint _hj;

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

        if (_hj != null)
        {
            BiteMovementUpdate();
        }
        else
        {
            ForwardsMovementUpdate();
        }
        _prevMoveInput = _moveInput;
    }

    private void ForwardsMovementUpdate(bool rotSnapping = true)
    {
        // rotation
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation;
        if (
            rotSnapping && (
                _prevMoveInput == Vector2.zero ||
                Quaternion.Angle(transform.rotation, desiredRot) >= _minSnapAngle
        ))
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
    }

    private void BackwardsMovementUpdate(bool rotSnapping = true)
    {
        // rotation
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(-_moveInput.x, 0, -_moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation;
        if (
            rotSnapping && (
                _prevMoveInput == Vector2.zero ||
                Quaternion.Angle(transform.rotation, desiredRot) >= _minSnapAngle
        ))
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
                _backwardsRotationSpeed * Time.fixedDeltaTime);
        }

        // position
        Vector3 position = transform.position;
        position += -transform.forward * Vector2.ClampMagnitude(_moveInput, 1).magnitude * _backwardsMoveSpeed * Time.fixedDeltaTime;
        if (_moveInput != Vector2.zero)
        {
            _rb.MoveRotation(rotation);
            _rb.MovePosition(position);
        }
    }

    private void BiteMovementUpdate()
    {
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);

        if (Quaternion.Angle(transform.rotation, desiredRot) > 95)
        {
            BackwardsMovementUpdate(false);
        }
        else if (Quaternion.Angle(transform.rotation, desiredRot) < 85)
        {
            ForwardsMovementUpdate(false);
        }
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

        if (_hj != null)
        {
            _canJump = false;
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
        if (context.performed)
        {
            RaycastHit rayInfo;
            if (Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.up * _biteRayHeight,
                _playerCollider.transform.forward,
                out rayInfo,
                _biteRayLength,
                _raycastInteractableLayer
            ))
            {
                Rigidbody rbOther = rayInfo.collider.gameObject.GetComponent<Rigidbody>();
                _hj = gameObject.AddComponent<HingeJoint>();
                _hj.anchor = _hingeAnchor;
                _hj.axis = transform.up;
                _hj.connectedBody = rbOther;
                _hj.enableCollision = true;
            }
        }
        else if (context.canceled)
        {
            if (_hj != null)
            {
                Destroy(_hj);
                _hj = null;
            }
        }
    }

    // From Shaffe on https://discussions.unity.com/t/copy-a-component-at-runtime/71172
    private T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
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
        // can jump
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

        // grounded
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

        // bite
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.up * _biteRayHeight,
            _playerCollider.transform.position + _playerCollider.transform.up * _biteRayHeight + _playerCollider.transform.forward * _biteRayLength
            );
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            transform.TransformPoint(_hingeAnchor),
            transform.TransformPoint(_hingeAnchor) + transform.up * 0.25f
            );
    }
}
