using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EntityTeam))]
public class DamageOnTouch : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField][Tooltip("Only deal damage to objects with these entity types. Will damage nothing if left empty.")] 
    List<HasEntityType.EntityType> damageEntityTypes = new();

    private EntityTeam entityTeam;

    private void Start()
    {
        entityTeam = GetComponent<EntityTeam>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (damageEntityTypes.Count == 0) return;
        
        Health healthManager = other.GetComponent<Health>();
        if (healthManager == null) return;
        
        HasEntityType otherEntityType = other.GetComponent<HasEntityType>();
        if (otherEntityType == null) return; // Only damage objects with an Entity Type
        
        if (!damageEntityTypes.Contains(otherEntityType.Type)) return;
        
        if (TeamManager.Instance.IsEnemy(entityTeam.team, healthManager.team))
        {
            healthManager.TakeDamage(damage);
        }
    }
}