using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Pan Constants")]
    public float maxSpeed = 20.0f;
    public float minSpeed = 0.5f;
    public float shiftMultiplier = 2.0f;

    [Header("Orbit Constants")]
    public float keyboardRotateSpeed = 40.0f;
    public float mouseRotateSpeed = 2f;

    [Header("Zoom Constants")]
    public float zoomSpeed = 0.1f;
    public float zoomLerpTime = 0.2f;
    public float minZoomDistance = 0.5f;
    public float maxZoomDistance = 80.0f;

    [Header("References")]
    public GameObject mainCam;

    // Events
    public delegate void MouseCapture(bool state);
    public event MouseCapture OnMouseCaptureChange;

    // Fields
    private Vector3 _initialCameraPos;
    private Vector3 _targetCameraPos;
    private float _timeSinceZoom = 0.0f;

    private float _currentZoomDistance;

    private bool _orbiting = false;

    public void Start()
    {
        _targetCameraPos = mainCam.transform.localPosition;
    }

    public void Update()
    {
        Orbit();
        Zoom();
        Pan();
    }

    private void Orbit()
    {
        // MMB
        if (Mouse.current.middleButton.isPressed)
        {
            if (!_orbiting)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Notify observers that mouse is now not free
                OnMouseCaptureChange(false);

                _orbiting = true;
            }

            // Construct mouse vector
            Vector2 mouseInput = Mouse.current.delta.ReadValue() * 0.15f;

            // Work out new angle from input
            Vector3 newAngles = new Vector3(0, transform.eulerAngles.y + (mouseInput.x * mouseRotateSpeed), transform.eulerAngles.z + (-mouseInput.y * mouseRotateSpeed));

            // Clamp vertical tilt
            newAngles.z = Mathf.Clamp(newAngles.z, 0, 88);

            transform.eulerAngles = newAngles;
        }
        else
        {
            if (_orbiting)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Notify observers that mouse is now free
                OnMouseCaptureChange(true);

                _orbiting = false;
            }
        }

        // Keyboard rotate
        if (Keyboard.current[Key.Q].isPressed)
        {
            transform.Rotate(new Vector3(0, -1.0f * Time.deltaTime * keyboardRotateSpeed, 0), Space.World);
        }
        else if (Keyboard.current[Key.E].isPressed)
        {
            transform.Rotate(new Vector3(0, Time.deltaTime * keyboardRotateSpeed, 0), Space.World);
        }
    }

    private void Pan()
    {
        //Keyboard move
        Vector2 inputVector = GetKeyboardInput();

        float zoomFraction = _currentZoomDistance / (maxZoomDistance - minZoomDistance);

        float speed = Mathf.Lerp(minSpeed, maxSpeed, zoomFraction);

        inputVector *= speed;

        if (Keyboard.current[Key.LeftShift].isPressed)
        {
            inputVector *= shiftMultiplier;
        }

        inputVector *= Time.deltaTime;

        Vector3 resultant = new Vector3
        {
            x = (Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * inputVector.y) + (Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * inputVector.x),
            z = -(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * inputVector.y) + (Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * inputVector.x)
        };

        transform.Translate(resultant, Space.World);
    }

    private void Zoom()
    {
        float mouseScrollDelta = Mouse.current.scroll.y.ReadValue() * 0.01f;

        // Calculate new zoom if necessary
        if (mouseScrollDelta != 0)
        {
            _initialCameraPos = mainCam.transform.localPosition;

            _targetCameraPos = _initialCameraPos;
            _targetCameraPos.x -= mouseScrollDelta * zoomSpeed * (_targetCameraPos.x + 2.0f);
            _targetCameraPos.x = Mathf.Clamp(_targetCameraPos.x, minZoomDistance, maxZoomDistance);

            _timeSinceZoom = 0.0f;
        }

        // Apply zoom
        if (_targetCameraPos != mainCam.transform.localPosition)
        {
            _timeSinceZoom += Time.deltaTime;

            mainCam.transform.localPosition = Vector3.Lerp(_initialCameraPos, _targetCameraPos, _timeSinceZoom / zoomLerpTime);
        }

        _currentZoomDistance = mainCam.transform.localPosition.x;
    }

    // Used in Pan
    private Vector2 GetKeyboardInput()
    {
        Vector2 p_Velocity = new Vector2();
        if (Keyboard.current[Key.W].isPressed)
        {
            p_Velocity += new Vector2(0, -1);
        }
        if (Keyboard.current[Key.S].isPressed)
        {
            p_Velocity += new Vector2(0, 1);
        }
        if (Keyboard.current[Key.A].isPressed)
        {
            p_Velocity += new Vector2(-1, 0);
        }
        if (Keyboard.current[Key.D].isPressed)
        {
            p_Velocity += new Vector2(1, 0);
        }
        return p_Velocity.normalized;
    }
}