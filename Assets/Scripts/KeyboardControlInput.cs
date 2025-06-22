using UnityEngine;

public class KeyboardControlInput : ShipControlInput
{
    public override float GetThrustInput()
    {
        return Input.GetAxisRaw("Vertical");
    }
    public override float GetSteerInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }
}
