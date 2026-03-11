using UnityEngine;
using static AIChaseInput;

public abstract class ShipControlInput : MonoBehaviour
{
    public abstract float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local);
    public abstract float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local);
    public abstract float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local);
    
}