using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// KeyboardControlInput is a concrete implementation of ControlInput that captures player input from the keyboard for movement and rotation.
/// </summary>
public class KeyboardControlInput : ControlInput
{
    public enum ControlScheme
    {
        WASD,
        ArrowKeys
    }
    public enum RotationScheme
    {
        Keyboard,
        Mouse
    }
    public ControlScheme controlScheme = ControlScheme.WASD;
    public RotationScheme rotationScheme = RotationScheme.Keyboard;

    private Vector2 worldControlVector = Vector2.zero;
    private bool calculatedControlThisFrame = false;

    void Update()
    {
        calculatedControlThisFrame = false;
    }
    /// <summary>
    /// When I am asked for local by a vehicular controller, I will return world coordinates instead.
    /// </summary>
    public override ControlInputData GetControlInput(ControlVectorCoordinates mode, bool iAmVehicularController = false)
    {
        if (!calculatedControlThisFrame) UpdateControlVector(controlScheme);
        bool shouldUseLocal = mode == ControlVectorCoordinates.Local && !iAmVehicularController;
        Vector2 translatedControlVector = shouldUseLocal ? GeometryUtils.WorldCoordsToLocal(worldControlVector, transform) : worldControlVector;
        return new ControlInputData(translatedControlVector, GetRotationInput(), null);
    }
    private void UpdateControlVector(ControlScheme controlScheme)
    {
        if (controlScheme == ControlScheme.WASD)
        {
            worldControlVector = new Vector2(Input.GetAxisRaw("HorizontalKeys"), Input.GetAxisRaw("VerticalKeys"));
        }
        else
        {
            worldControlVector = new Vector2(Input.GetAxisRaw("HorizontalArrows"), Input.GetAxisRaw("VerticalArrows"));
        }
    }
     public float GetRotationInput()
    {
        if (rotationScheme == RotationScheme.Keyboard)
        {
            return GetKeyboardRotationInput();
        }
        else
        {
            return GetMouseRotationInput();
        }
    }
    private float GetKeyboardRotationInput()
    {
        if (controlScheme == ControlScheme.WASD)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                return -1f;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Keypad7))
            {
                return -1f;
            }
            else if (Input.GetKey(KeyCode.Keypad9))
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
    }
    private float GetMouseRotationInput()
    {
        Vector2 worldMousePosition = CameraInformation.Instance.GetMousePosition();
        Vector2 directionToMouse = worldMousePosition - (Vector2)transform.position;
        float angleDifference = Vector2.SignedAngle(directionToMouse, transform.up);

        //if(Mathf.Abs(angleDifference) < 1f) return 0f; // Avoid jitter when the angle difference is very small
        float finalAngle = Mathf.Clamp(angleDifference / 180f, -1f, 1f);
        return finalAngle;
    }
}
