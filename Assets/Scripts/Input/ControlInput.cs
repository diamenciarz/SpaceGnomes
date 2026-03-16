using UnityEngine;
using static AIControlInput;

public abstract class ControlInput : MonoBehaviour
{
    public abstract float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false);
    public abstract float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false);
    public abstract float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false);
    
}