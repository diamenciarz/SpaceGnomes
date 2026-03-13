using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ignores ally ships, projectiles, and missiles")] private bool ignoreAllies = false;
    [SerializeField] private List<HasEntityType.EntityType> targetEntityTypes = new List<HasEntityType.EntityType> { HasEntityType.EntityType.Ship, HasEntityType.EntityType.Wall };
    [SerializeField] private string poolId = "PlasmaBullet"; // Pool ID for this object

    private EntityTeam.Team myTeam;
    private bool isDestroyed = false;

    private void Start()
    {
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
        HasEntityType entityType = other.GetComponent<HasEntityType>();
        if (!entityType) return;

        if (ShouldIgnoreAlly(other, entityType))
        {
            Debug.Log("Ignoring ally collision with " + other.name);
            return;
        }
        if (targetEntityTypes.Contains(entityType.Type))
        {
            //Debug.Log(gameObject.name + " collided with " + other.gameObject.name);
            isDestroyed = true;
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
    private bool ShouldIgnoreAlly(Collider2D other, HasEntityType entityType)
    {
        if (!ignoreAllies) return false;
        
        EntityTeam.Team otherTeam = TeamManager.Instance.GetEntityTeam(other.gameObject);
        if (!TeamManager.Instance.IsAlly(myTeam, otherTeam)) return false;
        
        return GetIgnoredEntities().Contains(entityType.Type);
    }
    private HasEntityType.EntityType[] GetIgnoredEntities()
    {
        if (ignoreAllies)
        {
            return new HasEntityType.EntityType[]
            {
                HasEntityType.EntityType.Ship,
                HasEntityType.EntityType.Projectile,
                HasEntityType.EntityType.Missile
            };
        }
        return System.Array.Empty<HasEntityType.EntityType>();
    }
}