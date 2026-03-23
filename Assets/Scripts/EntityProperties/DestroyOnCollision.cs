using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This script allows an object to be automatically despawned when it collides with certain types of entities.
/// </summary>
[RequireComponent(typeof(Collider2D))]

public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ignores ally ships, projectiles, and missiles")] private bool ignoreAllies = false;
    [SerializeField] private List<EntityType.EntityType> targetEntityTypes = new List<EntityType.EntityType> { EntityType.EntityType.Ship, EntityType.EntityType.Wall };

    private PooledObjectProperty pooledObjectProperty;
    private EntityTeam.Team myTeam;
    private bool isDestroyed = false;
    private void Start()
    {
        pooledObjectProperty = GetComponent<PooledObjectProperty>();
        EntityTeam entityTeam = GetComponent<EntityTeam>();
        myTeam = entityTeam ? entityTeam.team : EntityTeam.Team.Neutral;
    }
    private void OnEnable()
    {
        isDestroyed = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        EntityType entityType = other.GetComponent<EntityType>();
        if (!entityType) return;

        if (ShouldIgnoreAlly(other, entityType))
        {
            return;
        }
        if (targetEntityTypes.Contains(entityType.Type))
        {
            //Debug.Log(gameObject.name + " collided with " + other.gameObject.name);
            isDestroyed = true;
            if (pooledObjectProperty == null)
            {
                ObjectPoolManager.Instance.Despawn(gameObject);
                return;
            }
            ObjectPoolManager.Instance.Despawn(gameObject, pooledObjectProperty.poolId);
        }
    }
    private bool ShouldIgnoreAlly(Collider2D other, EntityType entityType)
    {
        if (!ignoreAllies) return false;
        
        EntityTeam.Team otherTeam = TeamManager.Instance.GetEntityTeam(other.gameObject);
        if (!TeamManager.Instance.IsAlly(myTeam, otherTeam)) return false;
        
        return GetIgnoredEntities().Contains(entityType.Type);
    }
    private EntityType.EntityType[] GetIgnoredEntities()
    {
        if (ignoreAllies)
        {
            return new EntityType.EntityType[]
            {
                EntityType.EntityType.Ship,
                EntityType.EntityType.Projectile,
                EntityType.EntityType.Missile
            };
        }
        return System.Array.Empty<EntityType.EntityType>();
    }
}