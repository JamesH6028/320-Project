using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 5f;
    [SerializeField]
    LayerMask _groundMask;
    [SerializeField]
    private float _groundingOffset = 1.0f;
    [SerializeField]
    private float _jumpForce = 100f;
    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _grounded;
    [SerializeField]
    private Camera _camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _grounded = true;
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        _rb.angularVelocity = Vector3.zero;

        transform.forward = new Vector3(_camera.transform.forward.x, transform.forward.y, _camera.transform.forward.z);

        Vector3 position = transform.position;

        position += transform.forward * _moveInput.y * _moveSpeed * Time.fixedDeltaTime;
        position += transform.right * _moveInput.x * _moveSpeed * Time.fixedDeltaTime;

        if (_grounded)
        {
            _rb.linearVelocity = Vector3.zero;

            RaycastHit hit;
            if (Physics.Raycast(position, -transform.up, out hit, Mathf.Infinity, _groundMask))
            {
                position = hit.point;
                position.y += _groundingOffset;
            }
        }

        _rb.MovePosition(position);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && _grounded)
        {
            _grounded = false;
            _rb.AddForce(transform.up * _jumpForce);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if((_groundMask & (1 << collision.gameObject.layer)) != 0)
        {
            _grounded = true;
        }
    }
}
