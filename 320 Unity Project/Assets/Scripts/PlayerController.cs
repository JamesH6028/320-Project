using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    LayerMask _groundMask;
    [SerializeField]
    private float _groundingOffset;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private Camera _camera;

    
    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _grounded;

    void Start()
    {
        _grounded = true;
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
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

        // keep player on ground for slopes
        if (_grounded)
        {
            _rb.linearVelocity = Vector3.zero;

            RaycastHit hit;
            if (Physics.Raycast(position, -transform.up, out hit, _groundingOffset, _groundMask))
            {
                position.y = hit.point.y;
                position.y += _groundingOffset;
            }
        }

        // only updates when needed
        if (_moveInput != Vector2.zero)
        {
            _rb.Move(position, rotation);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _grounded)
        {
            _grounded = false;
            _rb.AddForce(transform.up * _jumpForce);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((_groundMask & (1 << collision.gameObject.layer)) != 0)
        {
            float deltaAngle = Mathf.Abs(Vector3.Angle(Vector3.up, collision.contacts[0].normal));
            if (deltaAngle < 90)
            {
                _grounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((_groundMask & (1 << collision.gameObject.layer)) != 0)
        {
            if (!Physics.Raycast(transform.position, -transform.up, out RaycastHit _, _groundingOffset, _groundMask))
            {
                _grounded = false;
            }
        }
    }
}
