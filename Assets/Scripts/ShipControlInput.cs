using UnityEngine;

public abstract class ShipControlInput : MonoBehaviour
{
    public abstract float GetVerticalInput();
    public abstract float GetHorizontalInput();
    public abstract float GetRotationInput();
    
}