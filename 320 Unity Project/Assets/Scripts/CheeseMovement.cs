using UnityEngine;

public class CheeseMovement : MonoBehaviour
{
    [SerializeField]
    private float _amplitude;
    [SerializeField]
    private float _frequency;
    [SerializeField]
    private float _rotateSpeed;

    private Vector3 _initialPosition;
    private Vector3 _cheeseRotation;

    void Start()
    {
        _initialPosition = transform.position;
        _cheeseRotation = new Vector3(0.0f, _rotateSpeed, 0.0f);
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(_initialPosition.x, 
            Mathf.Sin(Time.fixedTime * _frequency) * _amplitude + _initialPosition.y,
            _initialPosition.z);
        transform.Rotate(_cheeseRotation * Time.fixedDeltaTime);
    }
}
