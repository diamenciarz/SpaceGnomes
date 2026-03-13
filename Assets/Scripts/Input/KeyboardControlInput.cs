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
    public ControlScheme controlScheme = ControlScheme.WASD;
    public override float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
    {
        if (controlScheme == ControlScheme.WASD)
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("HorizontalKeys"), Input.GetAxisRaw("VerticalKeys"));
            return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(inputVector, transform).y  : inputVector.y;
        }
        else
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("HorizontalArrows"), Input.GetAxisRaw("VerticalArrows"));
            return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(inputVector, transform).y : inputVector.y;
        }
    }
    public override float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
    {
        if (controlScheme == ControlScheme.WASD)
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("HorizontalKeys"), Input.GetAxisRaw("VerticalKeys"));
            return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(inputVector, transform).x : inputVector.x;
        }
        else
        {
            Vector2 inputVector = new Vector2(Input.GetAxisRaw("HorizontalArrows"), Input.GetAxisRaw("VerticalArrows"));
            return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(inputVector, transform).x : inputVector.x;
        }
    }
    public override float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
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
            if (Input.GetKey(KeyCode.Home))
            {
                return -1f;
            }
            else if (Input.GetKey(KeyCode.PageUp))
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
    }
}
