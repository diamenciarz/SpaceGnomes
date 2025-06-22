using UnityEngine;

public abstract class ShipControlInput : MonoBehaviour
{
    public abstract float GetThrustInput();
    public abstract float GetSteerInput();
}