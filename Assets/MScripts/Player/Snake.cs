using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The head of the player's snake
/// Manage the movement
/// There are two different moving mode:
/// Regular movement (_freeMovingMode = false) : control as a wheel
/// Free movement (_freeMovingMode = true) : orient to the control directions
/// </summary>
public class Snake : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] InputAction _movementControls;
    [SerializeField] InputAction _speedControl;
    [SerializeField] InputAction _startStopControl;

    [SerializeField] float _speed = 1.5f;
    [SerializeField] float _rotationSpeed = 350f;

    [SerializeField] bool _freeMovingMode = false;
    [SerializeField] bool _isMoving;

    float _halfSpeed = 0;

    float horizontal = 0f;
    float freeRotationTreshold = 1f;


    private void OnEnable()
    {
        _movementControls.Enable();
        _speedControl.Enable();
        _startStopControl.Enable();
    }

    public void OnDisable()
    {
        _movementControls.Disable();
        _speedControl.Disable();
        _startStopControl.Disable();
    }

    private void Awake()
    {
        _startStopControl.performed += SwitchMoving;
    }

    // Update is called once per frame
    void Update()
    {
        if (_freeMovingMode)
        {
            horizontal = GetFreeMovHorizontal(_movementControls.ReadValue<Vector2>());
        }
        else
        {
            horizontal = _movementControls.ReadValue<Vector2>().x;
        }

        _halfSpeed = _speedControl.ReadValue<float>();
    }

    void FixedUpdate()
    {
        if (!_isMoving)
            return;

        if (_halfSpeed != 0)
        {
            transform.Translate(Vector2.up * _speed * 0.3f * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(Vector3.forward * horizontal * -1 * _rotationSpeed * 0.3f * Time.fixedDeltaTime);
        }
        else
        {
            transform.Translate(Vector2.up * _speed * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(Vector3.forward * horizontal * -1 * _rotationSpeed * Time.fixedDeltaTime);
        }
    }

    float GetFreeMovHorizontal(Vector2 direction)
    {
        float horizontal;

        if (direction == Vector2.zero)
        {
            horizontal = 0f;
            return horizontal;
        }

        // wasd -> desired control angle
        // transform.rotation -> actual current angle
        float rotationAngle = Vector2.SignedAngle(new Vector2(transform.rotation.z, transform.rotation.w), Vector2.right);
        float controlAngle = Vector2.SignedAngle(direction, Vector2.right);

        // magic conversation
        float convertedRotationAngle = -2f * rotationAngle + 90;

        float angle = convertedRotationAngle - controlAngle;
        bool invertDirection = (angle < 360 && angle > 180) || (angle < -180 && angle > -360);

        // if they are close enough - horizontal = 0
        if (angle > freeRotationTreshold)
            horizontal = -1;
        else if (angle < -freeRotationTreshold)
            horizontal = 1;
        else
            horizontal = 0f;

        horizontal *= invertDirection ? -1 : 1;

        return horizontal;
    }

    private void SwitchMoving(InputAction.CallbackContext context)
    {
        _isMoving = !_isMoving;
    }
}
