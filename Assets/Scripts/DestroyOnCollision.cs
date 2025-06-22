using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField] private List<HealthManager.EntityType> targetEntityTypes = new List<HealthManager.EntityType> { HealthManager.EntityType.Ship, HealthManager.EntityType.Wall };
    [SerializeField] private string poolId = "PlasmaBullet"; // Pool ID for this object

    private void OnTriggerEnter2D(Collider2D other)
    {
        HealthManager healthManager = other.GetComponent<HealthManager>();
        if (healthManager != null && targetEntityTypes.Contains(healthManager.Type))
        {
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
}