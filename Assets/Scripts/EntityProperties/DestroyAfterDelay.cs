using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PooledObjectProperty))]
public class DestroyAfterDelay : ActivateOnSpawn
{
    [SerializeField] private float lifetime = 3f;

    private PooledObjectProperty pooledObjectProperty;
    private float timer;

    private void Awake()
    {
        pooledObjectProperty = GetComponent<PooledObjectProperty>();
    }
    public override void Activate()
    {
        timer = 0f;
        //Debug.Log("Enabled: " + gameObject.name);
    }

    private void Update()
    {
        // Update lifetime timer
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Debug.Log("Despawning after delay: " + gameObject.name);
            ObjectPoolManager.Instance.Despawn(gameObject, pooledObjectProperty.poolId);
        }
    }
}
