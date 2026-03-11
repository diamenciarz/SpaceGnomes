using Unity.VisualScripting;
using UnityEngine;

public class KeyboardControlInput : ShipControlInput
{
    public enum ControlScheme
    {
        WASD,
        ArrowKeys
    }
    public ControlScheme controlScheme = ControlScheme.WASD;
    public override float GetVerticalInput()
    {
        if (controlScheme == ControlScheme.WASD)
        {
            return Input.GetAxisRaw("VerticalKeys");
        }
        else
        {
            return Input.GetAxisRaw("VerticalArrows");
        }
    }
    public override float GetHorizontalInput()
    {
        if (controlScheme == ControlScheme.WASD)
        {
            return Input.GetAxisRaw("HorizontalKeys");
        }
        else
        {
            return Input.GetAxisRaw("HorizontalArrows");
        }
    }
    public override float GetRotationInput()
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
}
