using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _gravityAcceleration;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _maxForwardSpeed;
    [SerializeField] private float _maxBackwardSpeed;
    [SerializeField] private float _maxStrafeSpeed;
    [SerializeField] private float _jumpSpeed;
    [SerializeField] private float _maxLookUpAngle;
    [SerializeField] private float _maxLookDownAngle;
    [SerializeField] private float _lookSensitivity;

    private CharacterController _controller;
    private Transform           _head;
    private Vector2             _lookInput;
    private Vector2             _moveInput;
    private bool                _jumpInput;
    private Vector3             _velocityHor;
    private Vector3             _velocityVer;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _head       = GetComponentInChildren<Camera>().transform;
        _lookInput  = Vector2.zero;
        _moveInput  = Vector2.zero;
        _jumpInput  = false;
    }

    public void SetLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }

    public void SetJumpInput(bool jumpInput)
    {
        _jumpInput = jumpInput;
    }

    void Update()
    {
        UpdateBodyRotation();
        UpdateHeadRotation();
        UpdateVelocityHor();
        UpdateVelocityVer();
        UpdatePosition();
    }

    private void UpdateBodyRotation()
    {
        transform.Rotate(0f, _lookInput.x * _lookSensitivity, 0f);
    }

    public void UpdateHeadRotation()
    {
        Vector3 headRotation = _head.localEulerAngles;

        headRotation.x -= _lookInput.y * _lookSensitivity;

        if (headRotation.x > 180f)
            headRotation.x = Mathf.Max(_maxLookUpAngle, headRotation.x);
        else
            headRotation.x = Mathf.Min(_maxLookDownAngle, headRotation.x);

        _head.localEulerAngles = headRotation;
    }

    private void UpdateVelocityHor()
    {
        if (_moveInput.y >= 0f)
            _velocityHor.z = _moveInput.y * _maxForwardSpeed;
        else
            _velocityHor.z = _moveInput.y * _maxBackwardSpeed;

        _velocityHor.x = _moveInput.x * _maxStrafeSpeed;

        if (_moveInput.y >= 0f && _velocityHor.magnitude > _maxForwardSpeed)
            _velocityHor = _velocityHor.normalized * _maxForwardSpeed;
        else if  (_moveInput.y < 0f && _velocityHor.magnitude > _maxBackwardSpeed)
            _velocityHor = _velocityHor.normalized * _maxBackwardSpeed;
    }

    private void UpdateVelocityVer()
    {
        if (_jumpInput && _controller.isGrounded)
            _velocityVer.y = _jumpSpeed;
        else if (_controller.isGrounded)
            _velocityVer.y = -1f;
        else if (_velocityVer.y > -_maxFallSpeed)
        {
            _velocityVer.y += _gravityAcceleration * Time.deltaTime;
            _velocityVer.y = Mathf.Max(_velocityVer.y, -_maxFallSpeed);
        }
    }

    private void UpdatePosition()
    {
        Vector3 motion = (_velocityHor + _velocityVer) * Time.deltaTime;
        motion = transform.TransformVector(motion);

        _controller.Move(motion);
    }

}
