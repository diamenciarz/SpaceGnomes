using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField] private List<HasEntityType.EntityType> targetEntityTypes = new List<HasEntityType.EntityType> { HasEntityType.EntityType.Ship, HasEntityType.EntityType.Wall };
    [SerializeField] private string poolId = "PlasmaBullet"; // Pool ID for this object

    private void OnTriggerEnter2D(Collider2D other)
    {
        HasEntityType hasEntityType = other.GetComponent<HasEntityType>();
        if (hasEntityType != null && targetEntityTypes.Contains(hasEntityType.Type))
        {
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
}