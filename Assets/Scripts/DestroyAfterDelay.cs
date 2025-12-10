using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string poolId = ""; // Pool ID for this bullet

    private float timer;
    private void OnEnable()
    {
        //Debug.Log("Enabled: " + gameObject.name);
        timer = 0f;
    }

    private void Update()
    {
        // Update lifetime timer
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            //Debug.Log("Despawning after delay: " + gameObject.name);
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
}
