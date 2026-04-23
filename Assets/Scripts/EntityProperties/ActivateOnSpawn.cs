using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component is designed to be attached to entities that are part of an object pooling system and need to perform an action immediately upon spawning.
/// </summary>
public class ActivateOnSpawn : MonoBehaviour
{
    // This event is intended to be subscribed to by other components
    public event Action<GameObject> OnSpawned;
    // This method is intended to be overridden in derived classes
    public virtual void Activate()
    {
        if (OnSpawned != null)
        {
            OnSpawned(gameObject);
        }
    }
}
