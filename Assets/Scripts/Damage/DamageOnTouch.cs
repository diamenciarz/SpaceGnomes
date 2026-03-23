using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EntityTeam))]
public class DamageOnTouch : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField][Tooltip("Only deal damage to objects with these entity types. Will damage nothing if left empty.")] 
    List<EntityType.EntityType> damageEntityTypes = new();

    private EntityTeam entityTeam;

    private void Start()
    {
        entityTeam = GetComponent<EntityTeam>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (damageEntityTypes.Count == 0) return;
        
        Health health = other.GetComponent<Health>();
        if (health == null) return;
        
        EntityType otherEntityType = other.GetComponent<EntityType>();
        if (otherEntityType == null) return; // Only damage objects with an Entity Type
        
        if (!damageEntityTypes.Contains(otherEntityType.Type)) return;
        
        if (TeamManager.Instance.IsEnemy(entityTeam.team, health.team))
        {
            health.TakeDamage(damage);
        }
    }
}