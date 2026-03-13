using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnSpawn : MonoBehaviour
{
    // This event is intended to be subscribed to by other components
    public event Action<GameObject> OnSpawned;
    // This method is intended to be overridden in derived classes
    public virtual void Activate()
    {
        Debug.Log("ActivateOnSpawn activated for " + gameObject.name);
        if (OnSpawned != null)
        {
            OnSpawned(gameObject);
        }
    }
}
