using UnityEngine;
using System.Collections.Generic;

public class EntityCounter : MonoBehaviour
{
    // Singleton instance
    public static EntityCounter Instance { get; private set; }

    private Dictionary<HasEntityType.EntityType, HashSet<GameObject>> activeEntities = new Dictionary<HasEntityType.EntityType, HashSet<GameObject>>();

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize dictionary for all entity types
        foreach (HasEntityType.EntityType type in System.Enum.GetValues(typeof(HasEntityType.EntityType)))
        {
            activeEntities[type] = new HashSet<GameObject>();
        }
    }

    // Add an entity to the counter
    public void RegisterEntity(GameObject obj)
    {
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType != null)
        {
            activeEntities[hasEntityType.Type].Add(obj);
        }
        else
        {
            Debug.LogWarning($"GameObject {obj.name} has no HealthManager component!");
        }
    }

    // Remove an entity from the counter
    public void UnregisterEntity(GameObject obj)
    {
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType != null)
        {
            activeEntities[hasEntityType.Type].Remove(obj);
        }
    }

    // Get all active entities of a specific type
    public IEnumerable<GameObject> GetEntities(HasEntityType.EntityType type)
    {
        return activeEntities[type];
    }

    // Find the closest entity of a specific type to a given position
    public GameObject FindClosestEntity(HasEntityType.EntityType type, Vector3 position)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in activeEntities[type])
        {
            if (entity.activeInHierarchy)
            {
                float distance = Vector3.Distance(position, entity.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }
}