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
    Player player;

    [SerializeField] InputAction _movementControls;
    [SerializeField] InputAction _speedControl;
    [SerializeField] InputAction _startStopControl;

    [SerializeField] float speed = 1.5f;
    [SerializeField] float rotationSpeed = 350f;

    [SerializeField] bool freeMovingMode = true;
    [SerializeField] bool isMoving = true;

    float slowSpeed = 0;
    float slowSpeedRatio = 0.3f;

    float horizontalRotation = 0f;
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

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject.GetComponent<Player>();
    }

        // Update is called once per frame
    void Update()
    {
        if (freeMovingMode)
        {
            horizontalRotation = GetFreeMoveHorizontal(_movementControls.ReadValue<Vector2>());
        }
        else
        {
            horizontalRotation = _movementControls.ReadValue<Vector2>().x;
        }

        slowSpeed = _speedControl.ReadValue<float>();
    }

    void FixedUpdate()
    {
        if (!isMoving)
            return;

        if (slowSpeed != 0)
        {
            transform.Translate(Vector2.up * speed * slowSpeedRatio * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(Vector3.forward * horizontalRotation * -1 * rotationSpeed * slowSpeedRatio * Time.fixedDeltaTime);
        }
        else
        {
            transform.Translate(Vector2.up * speed * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(Vector3.forward * horizontalRotation * -1 * rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private float GetFreeMoveHorizontal(Vector2 direction)
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
        isMoving = !isMoving;
    }
}
