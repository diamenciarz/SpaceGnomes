using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : ActivateOnSpawn
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string poolId = ""; // Pool ID for this bullet

    private float timer;

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
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
}
