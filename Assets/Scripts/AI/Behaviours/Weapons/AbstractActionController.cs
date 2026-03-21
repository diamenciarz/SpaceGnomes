using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractActionController : MonoBehaviour
{
    public bool isControlledByPlayer;
    public abstract void SetAction(bool isOn);
    public abstract ShipAction GetActionType();
    public abstract void Detach();

}
