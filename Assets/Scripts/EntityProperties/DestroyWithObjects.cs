using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This script allows an object to be automatically despawned when any of the specified objects are despawned.
/// It subscribes to the OnDespawned event of ActivateOnDespawn components on the specified objects,
/// and calls its own DestroyTogether method when those events are triggered.
/// This is useful for creating dependencies between objects, such as a projectile that should be destroyed when the shooter is destroyed,
/// or a visual effect that should be removed when the associated object is removed.
/// The script also provides methods to add or remove objects from the dependency list at runtime, allowing for dynamic relationships between objects.
/// </summary>
public class DestroyWithObjects : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsSubscribedTo;

    private PooledObjectProperty pooledObjectProperty;
    private bool isDestroyed = false;

    void Start()
    {
        pooledObjectProperty = GetComponent<PooledObjectProperty>();
        objectsSubscribedTo.ForEach(obj => SubscribeToObject(obj));
    }
    public void SubscribeToObject(GameObject obj)
    {
        if (objectsSubscribedTo.Contains(obj))
        {
            Debug.LogError($"Attempted to subscribe to object {obj.name} which is already in the dependency list.");
            return;
        }
        objectsSubscribedTo.Add(obj);
        ActivateOnDespawn[] despawnComponents = obj.GetComponents<ActivateOnDespawn>();
        if (despawnComponents.Length == 0)
        {
            despawnComponents = new ActivateOnDespawn[] { obj.AddComponent<ActivateOnDespawn>() };
        }
        despawnComponents[0].OnDespawned += DestroyTogether;
    }
    public void UnsubscribeFromObject(GameObject obj)
    {
        if (!objectsSubscribedTo.Contains(obj))
        {
            Debug.LogError($"Attempted to unsubscribe from object {obj.name} which is not in the dependency list.");
            return;
        }
        objectsSubscribedTo.Remove(obj);
        ActivateOnDespawn[] despawnComponents = obj.GetComponents<ActivateOnDespawn>();
        foreach (ActivateOnDespawn script in despawnComponents)
        {
            script.OnDespawned -= DestroyTogether;
        }
    }
    public void DestroyTogether(GameObject other)
    {
        if (isDestroyed) return; // Prevent multiple calls if multiple objects are despawned around the same time
        isDestroyed = true;

        if (pooledObjectProperty == null)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
            return;
        }
        ObjectPoolManager.Instance.Despawn(gameObject, pooledObjectProperty.poolId);
    }

}
