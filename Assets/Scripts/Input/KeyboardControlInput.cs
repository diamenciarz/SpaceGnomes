using Unity.VisualScripting;
using UnityEngine;
using static AIControlInput;

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

    private Vector2 controlVector = Vector2.zero;
    private bool calculatedControlThisFrame = false;

    void Update()
    {
        calculatedControlThisFrame = false;
    }

    public override float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!calculatedControlThisFrame) UpdateControlVector(controlScheme);
        bool shouldUseLocal = mode == ControlVectorCoordinates.Local && !iAmVehicularController;
        if(!shouldUseLocal) Debug.Log("Using world");
        return shouldUseLocal ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).y : controlVector.y;
    }
    public override float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!calculatedControlThisFrame) UpdateControlVector(controlScheme);
        bool shouldUseLocal = mode == ControlVectorCoordinates.Local && !iAmVehicularController;
        if(!shouldUseLocal) Debug.Log("Using world");
        return shouldUseLocal ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).x : controlVector.x;
    }
    private void UpdateControlVector(ControlScheme controlScheme)
    {
        if (controlScheme == ControlScheme.WASD)
        {
            controlVector = new Vector2(Input.GetAxisRaw("HorizontalKeys"), Input.GetAxisRaw("VerticalKeys"));
            Debug.Log($"Control Vector: {controlVector}, local control vector: {GeometryUtils.WorldCoordsToLocal(controlVector, transform)}");
        }
        else
        {
            controlVector = new Vector2(Input.GetAxisRaw("HorizontalArrows"), Input.GetAxisRaw("VerticalArrows"));
        }
    }
     public override float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (rotationScheme == RotationScheme.Keyboard)
        {
            return GetKeyboardRotationInput(mode);
        }
        else
        {
            return GetMouseRotationInput();
        }
    }
    private float GetKeyboardRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
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
        Vector2 worldMousePosition = GeometryUtils.GetMousePosition();
        Vector2 directionToMouse = worldMousePosition - (Vector2)transform.position;
        float angleDifference = Vector2.SignedAngle(directionToMouse, transform.up);

        //if(Mathf.Abs(angleDifference) < 1f) return 0f; // Avoid jitter when the angle difference is very small
        float finalAngle = Mathf.Clamp(angleDifference / 180f, -1f, 1f);
        return finalAngle;
    }
}
