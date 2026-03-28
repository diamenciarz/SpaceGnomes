using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script allows an object to be automatically despawned after a certain amount of time has passed since it was spawned.
/// </summary>
public class DespawnAfterDelay : ActivateOnSpawn
{
    [SerializeField] private float lifetime = 3f;

    private PooledObjectProperty pooledObjectProperty;
    private float timer;
    private bool isDestroyed = false;

    private void Awake()
    {
        pooledObjectProperty = GetComponent<PooledObjectProperty>();
    }
    public override void Activate()
    {
        timer = 0f;
    }

    private void Update()
    {
        if(isDestroyed) return;
        // Update lifetime timer
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            isDestroyed = true;
            if (pooledObjectProperty == null)
            {
                ObjectPoolManager.Instance.Despawn(gameObject);
                return;
            }
            ObjectPoolManager.Instance.Despawn(gameObject, pooledObjectProperty.poolId);
        }
    }
}
