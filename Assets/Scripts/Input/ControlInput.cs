using UnityEngine;
/// <summary>
/// ControlInput is an abstract base class that defines the interface for obtaining control input for movement and rotation.
/// </summary>
public abstract class ControlInput : MonoBehaviour
{
    public enum ControlVectorCoordinates
    {
        World,
        Local
    }
    public struct ControlInputData
    {
        public ControlInputData(Vector2 worldControlVector, float rotation, Vector2? targetVelocity)
        {
            this.controlVector = worldControlVector;
            this.rotation = rotation;
            this.targetVelocity = targetVelocity;
        }
        public Vector2 controlVector;
        public float rotation;
        public Vector2? targetVelocity;
    }
    public abstract ControlInputData GetControlInput(ControlVectorCoordinates mode, bool iAmVehicularController = false);
}