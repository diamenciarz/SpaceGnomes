using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActivateOnDespawn : MonoBehaviour
{
    // This event is intended to be subscribed to by other components
    public event Action<GameObject> OnDespawned;
    // This method is intended to be overridden in derived classes
    public virtual void Activate()
    {
        Debug.Log("ActivateOnDespawn activated for " + gameObject.name);
        if (OnDespawned != null)
        {
            OnDespawned(gameObject);
        }
    }
}
