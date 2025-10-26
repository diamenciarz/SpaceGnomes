using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EntityCounter : MonoBehaviour
{
    // Singleton instance
    public static EntityCounter Instance { get; private set; }

    // Should only track active entities
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
        return new List<GameObject>(activeEntities[type]);
    }

    // Find the closest entity of a specific type to a given position
    public GameObject FindClosestEntity(HasEntityType.EntityType type, Vector3 position)
    {
        return GeometryUtils.FindClosestEntity(activeEntities[type], position);
    }
}