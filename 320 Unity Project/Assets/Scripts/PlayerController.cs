using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Collider _playerCollider;

    private Rigidbody _rb;

    public Collider PlayerCollider => _playerCollider;

    #region Movement
    [Serializable]
    private class Movement
    {
        public float _moveSpeed;
        public float _rotationSpeed;
        public float _biteRotationSpeed;
        public float _backwardsMoveSpeed;
        public float _backwardsRotationSpeed;
        public float _minSnapAngle;
    }
    [SerializeField] private Movement _movement;

    private Vector2 _moveInput;
    private Vector2 _prevMoveInput;
    #endregion

    #region Jumping
    [Serializable]
    private class Jumping
    {
        public Vector3 _groundingCheckDistance;
        public Vector3 _canJumpCheckDistance;
        public float _jumpForce;
        public PhysicsMaterial _zeroFrictionMaterial;
    }
    [SerializeField] private Jumping _jumping;

    private bool _grounded;
    private bool _canJump;
    #endregion

    #region Biting
    [Serializable]
    private class Biting
    {
        public float _biteRayHeight;
        public float _biteRayLength;
        public LayerMask _raycastInteractableLayer;
        public Vector3 _hingeAnchor;
    }
    [SerializeField] private Biting _biting;

    private HingeJoint _hj;
    #endregion

    #region Camera
    [Serializable]
    private class CameraControls
    {
        public Camera _camera;
        public Transform _lookPoint;
    }
    [SerializeField] private CameraControls _camera;

    private Vector2 _lookMin = new Vector2(.25f, -.25f);
    private Vector2 _lookMax = new Vector2(2.25f, -2.25f);
    private Vector2 _lookStep;
    private float _zoomCount = 50.0f;
    private float _zoomMax = 50.0f;
    #endregion

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

        if (_moveInput == Vector2.zero)
        {
            ResetVelocity(_rb);
        }
        else
        {
            if (_hj != null)
            {
                BiteMovementUpdate();
            }
            else
            {
                ForwardsMovementUpdate();
            }
        }
        _prevMoveInput = _moveInput;

    }

    private void ForwardsMovementUpdate(bool rotSnapping = true, float rotationSpeed = default)
    {
        if(rotationSpeed == default)
        {
            rotationSpeed = _movement._rotationSpeed;
        }

        // rotation
        Vector3 cameraForward = new Vector3(_camera._camera.transform.forward.x, 0, _camera._camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation;
        if (
            rotSnapping && (
                _prevMoveInput == Vector2.zero ||
                Quaternion.Angle(transform.rotation, desiredRot) >= _movement._minSnapAngle
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
                rotationSpeed * Time.fixedDeltaTime);
        }

        // position
        Vector3 position = transform.position;
        position += transform.forward * Vector2.ClampMagnitude(_moveInput, 1).magnitude * _movement._moveSpeed * Time.fixedDeltaTime;
        if (_moveInput != Vector2.zero)
        {
            _rb.MoveRotation(rotation);
            _rb.MovePosition(position);
        }
    }

    private void BackwardsMovementUpdate(bool rotSnapping = true, float rotationSpeed = default)
    {
        if(rotationSpeed == default)
        {
            rotationSpeed = _movement._backwardsRotationSpeed;
        }

        // rotation
        Vector3 cameraForward = new Vector3(_camera._camera.transform.forward.x, 0, _camera._camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(-_moveInput.x, 0, -_moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);
        Quaternion rotation;
        if (
            rotSnapping && (
                _prevMoveInput == Vector2.zero ||
                Quaternion.Angle(transform.rotation, desiredRot) >= _movement._minSnapAngle
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
                rotationSpeed * Time.fixedDeltaTime);
        }

        // position
        Vector3 position = transform.position;
        position += -transform.forward * Vector2.ClampMagnitude(_moveInput, 1).magnitude * _movement._backwardsMoveSpeed * Time.fixedDeltaTime;
        if (_moveInput != Vector2.zero)
        {
            _rb.MoveRotation(rotation);
            _rb.MovePosition(position);
        }
    }

    private void BiteMovementUpdate()
    {
        Vector3 cameraForward = new Vector3(_camera._camera.transform.forward.x, 0, _camera._camera.transform.forward.z).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(new Vector3(_moveInput.x, 0, _moveInput.y), Vector3.up);
        desiredRot *= Quaternion.LookRotation(cameraForward, Vector3.up);

        if (Quaternion.Angle(transform.rotation, desiredRot) > 95)
        {
            BackwardsMovementUpdate(false, _movement._biteRotationSpeed);
        }
        else if (Quaternion.Angle(transform.rotation, desiredRot) < 85)
        {
            ForwardsMovementUpdate(false, _movement._biteRotationSpeed);
        }
    }

    private bool GroundedRayCasts()
    {
        return Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._groundingCheckDistance.z,
                -_playerCollider.transform.up,
                _jumping._groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._groundingCheckDistance.z,
                -_playerCollider.transform.up,
                _jumping._groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * _jumping._groundingCheckDistance.x,
                -_playerCollider.transform.up,
                _jumping._groundingCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._groundingCheckDistance.x,
                -_playerCollider.transform.up,
                _jumping._groundingCheckDistance.y
        );
    }

    private bool CanJumpRayCasts()
    {
        return Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._canJumpCheckDistance.z,
                -_playerCollider.transform.up,
                _jumping._canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._canJumpCheckDistance.z,
                -_playerCollider.transform.up,
                _jumping._canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * _jumping._canJumpCheckDistance.x,
                -_playerCollider.transform.up,
                _jumping._canJumpCheckDistance.y) ||
            Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._canJumpCheckDistance.x,
                -_playerCollider.transform.up,
                _jumping._canJumpCheckDistance.y);
    }

    private void SetGrounded(bool grounded)
    {
        if (_grounded == grounded)
        {
            return;
        }

        _grounded = grounded;
        // so running into walls has zero friction when in the air
        _playerCollider.material = grounded ? null : _jumping._zeroFrictionMaterial;
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
            _rb.AddForce(transform.up * _jumping._jumpForce);
            SetCanJump(false);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit rayInfo;
            if (Physics.Raycast(
                _playerCollider.transform.position + _playerCollider.transform.up * _biting._biteRayHeight,
                _playerCollider.transform.forward,
                out rayInfo,
                _biting._biteRayLength,
                _biting._raycastInteractableLayer
            ))
            {
                Rigidbody rbOther = rayInfo.collider.gameObject.GetComponent<Rigidbody>();
                _hj = gameObject.AddComponent<HingeJoint>();
                _hj.anchor = _biting._hingeAnchor;
                _hj.axis = transform.up;
                _hj.connectedBody = rbOther;
                _hj.enableCollision = true;
            }
        }
        else if (context.canceled)
        {
            DestroyHingeJoint();
        }
    }

    private void DestroyHingeJoint()
    {
        if (_hj != null)
        {
            ResetVelocity(_hj.connectedBody);
            ResetVelocity(_rb);
            Destroy(_hj);
            _hj = null;
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

    private void ResetVelocity(Rigidbody rigidbody, bool resetLinearY = false)
    {
        rigidbody.linearVelocity = Vector3.Scale(rigidbody.linearVelocity, resetLinearY ? Vector3.zero : Vector3.up);
        rigidbody.angularVelocity = Vector3.zero;
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
        _camera._lookPoint.SetLocalPositionAndRotation(new Vector3(0.0f, lookY, lookZ), Quaternion.identity);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed) ResetLevel();
    }

    private void ResetLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    void OnDrawGizmosSelected()
    {
        // can jump
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._canJumpCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._canJumpCheckDistance.z + -_playerCollider.transform.up * _jumping._canJumpCheckDistance.y
            );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._canJumpCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._canJumpCheckDistance.z + -_playerCollider.transform.up * _jumping._canJumpCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * _jumping._canJumpCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * _jumping._canJumpCheckDistance.x + -_playerCollider.transform.up * _jumping._canJumpCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._canJumpCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._canJumpCheckDistance.x + -_playerCollider.transform.up * _jumping._canJumpCheckDistance.y
        );

        // grounded
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._groundingCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * _jumping._groundingCheckDistance.z + -_playerCollider.transform.up * _jumping._groundingCheckDistance.y
            );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._groundingCheckDistance.z,
            _playerCollider.transform.position + _playerCollider.transform.forward * -_jumping._groundingCheckDistance.z + -_playerCollider.transform.up * _jumping._groundingCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * _jumping._groundingCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * _jumping._groundingCheckDistance.x + -_playerCollider.transform.up * _jumping._groundingCheckDistance.y
        );
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._groundingCheckDistance.x,
            _playerCollider.transform.position + _playerCollider.transform.right * -_jumping._groundingCheckDistance.x + -_playerCollider.transform.up * _jumping._groundingCheckDistance.y
        );

        // bite
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            _playerCollider.transform.position + _playerCollider.transform.up * _biting._biteRayHeight,
            _playerCollider.transform.position + _playerCollider.transform.up * _biting._biteRayHeight + _playerCollider.transform.forward * _biting._biteRayLength
            );
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            transform.TransformPoint(_biting._hingeAnchor),
            transform.TransformPoint(_biting._hingeAnchor) + transform.up * 0.25f
            );
    }
}
