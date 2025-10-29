using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyWithObjects : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToDependOn;
    void Start()
    {
        foreach (GameObject obj in objectsToDependOn)
        {
            SubscribeToObject(obj);
        }
    }
    public void AddObjectToDependOn(GameObject obj)
    {
        objectsToDependOn.Add(obj);
        SubscribeToObject(obj);
    }
    private void SubscribeToObject(GameObject obj)
    {
        ActivateOnDespawn[] despawnComponents = obj.GetComponents<ActivateOnDespawn>();
        if (despawnComponents.Length == 0)
        {
            despawnComponents = new ActivateOnDespawn[] { obj.AddComponent<ActivateOnDespawn>() };
        }
        despawnComponents[0].OnDespawned += DestroyTogether;
    }
    public void RemoveObjectToDependOn(GameObject obj)
    {
        objectsToDependOn.Remove(obj);
        UnsubscribeFromObject(obj);
    }
    public void UnsubscribeFromObject(GameObject obj)
    {
        ActivateOnDespawn[] despawnComponents = obj.GetComponents<ActivateOnDespawn>();
        foreach (ActivateOnDespawn script in despawnComponents)
        {
            script.OnDespawned -= DestroyTogether;
        }
    }
    public void DestroyTogether(GameObject other)
    {
        
        ObjectPoolManager.Instance.Despawn(gameObject);
    }

}
