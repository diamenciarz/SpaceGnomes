using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HasEntityType;
using static UnityEngine.EventSystems.EventTrigger;

public class EntityCounter : MonoBehaviour
{
    // Singleton instance
    public static EntityCounter Instance { get; private set; }


    [Header("Trajectory Settings")]
    [SerializeField] private float savedTrajectoryPositions = 1000;
    [Header("Grid Settings")]
    // Grid cell size (tune to your query radii, e.g., 10f for distance-10 searches)
    public float CellSize = 10f;

    // Toggle to draw grid in Scene view (Gizmos)
    public bool EnableGridDebug = false;

    // Should only track active entities
    private Dictionary<System.Type, Dictionary<int, EntityTracker>> trackers =
        new Dictionary<System.Type, Dictionary<int, EntityTracker>>();

    // Helper class for per-type tracking
    private class EntityTracker
    {
        public HashSet<GameObject> AllEntities = new HashSet<GameObject>();
        public Dictionary<Vector2Int, HashSet<GameObject>> Cells = new Dictionary<Vector2Int, HashSet<GameObject>>();
        public Dictionary<GameObject, Vector2Int> EntityToCell = new Dictionary<GameObject, Vector2Int>();
    }
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

        // Initialize trackers for all entity types
        trackers[typeof(EntityType)] = new Dictionary<int, EntityTracker>();
        foreach (EntityType type in System.Enum.GetValues(typeof(EntityType)))
        {
            trackers[typeof(EntityType)][(int)type] = new EntityTracker();
        }

        // Initialize trackers for all force types
        trackers[typeof(ForceManager.ForceType)] = new Dictionary<int, EntityTracker>();
        foreach (ForceManager.ForceType type in System.Enum.GetValues(typeof(ForceManager.ForceType)))
        {
            trackers[typeof(ForceManager.ForceType)][(int)type] = new EntityTracker();
        }
    }

    private void Register(GameObject obj, System.Type enumType, int enumValue)
    {
        AttachPositionTracker(obj);
        Vector2 pos = obj.transform.position;
        Vector2Int cell = GetCellKey(pos);

        var tracker = trackers[enumType][enumValue];
        tracker.AllEntities.Add(obj);
        AddToCell(tracker, obj, cell);
        tracker.EntityToCell[obj] = cell;
    }

    private void Unregister(GameObject obj, System.Type enumType, int enumValue)
    {
        var tracker = trackers[enumType][enumValue];

        if (tracker.EntityToCell.TryGetValue(obj, out Vector2Int cell))
        {
            RemoveFromCell(tracker, obj, cell);
            tracker.EntityToCell.Remove(obj);
        }
        tracker.AllEntities.Remove(obj);
        RemovePositionTracker(obj);
    }

    // Call this when an entity moves to update its spatial bucket
    public void UpdateEntityPosition(GameObject obj)
    {
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType != null)
        {
            UpdatePosition(obj, typeof(EntityType), (int)hasEntityType.Type);
        }

        // Also update force position if applicable
        ForceProperty fp = obj.GetComponent<ForceProperty>();
        if (fp != null)
        {
            UpdatePosition(obj, typeof(ForceManager.ForceType), (int)fp.forceType);
        }
    }

    private void UpdatePosition(GameObject obj, System.Type enumType, int enumValue)
    {
        var tracker = trackers[enumType][enumValue];

        if (tracker.EntityToCell.TryGetValue(obj, out Vector2Int oldCell))
        {
            Vector2 pos = obj.transform.position;
            Vector2Int newCell = GetCellKey(pos);

            if (newCell != oldCell)
            {
                RemoveFromCell(tracker, obj, oldCell);
                AddToCell(tracker, obj, newCell);
                tracker.EntityToCell[obj] = newCell;
            }
        }
    }

    private List<GameObject> GetNearby(System.Type enumType, int enumValue, Vector2 center, float radius)
    {
        var tracker = trackers[enumType][enumValue];
        var candidates = new HashSet<GameObject>();

        // Find cells that intersect the query circle
        Vector2Int minCell = GetCellKey(center - Vector2.one * radius);
        Vector2Int maxCell = GetCellKey(center + Vector2.one * radius);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cellKey = new Vector2Int(x, y);
                if (tracker.Cells.TryGetValue(cellKey, out HashSet<GameObject> cellEntities))
                {
                    foreach (var entity in cellEntities)
                    {
                        candidates.Add(entity);
                    }
                }
            }
        }

        // Filter by exact distance
        var nearby = new List<GameObject>();
        foreach (var entity in candidates)
        {
            float dist = Vector2.Distance(center, entity.transform.position);
            if (dist <= radius)
            {
                nearby.Add(entity);
            }
        }

        return nearby;
    }

    public void RegisterEntity(GameObject obj)
    {
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType != null)
        {
            Register(obj, typeof(EntityType), (int)hasEntityType.Type);
        }
        else
        {
            Debug.LogWarning($"GameObject {obj.name} has no HasEntityType component!");
        }
    }

    public void RegisterForceEntity(GameObject obj, ForceManager.ForceType type)
    {
        Register(obj, typeof(ForceManager.ForceType), (int)type);
    }

    public void UnregisterEntity(GameObject obj)
    {
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType != null)
        {
            Unregister(obj, typeof(EntityType), (int)hasEntityType.Type);
        }
        else
        {
            Debug.LogWarning($"GameObject {obj.name} has no HasEntityType component!");
        }
    }

    public void UnregisterForceEntity(GameObject obj, ForceManager.ForceType type)
    {
        Unregister(obj, typeof(ForceManager.ForceType), (int)type);
    }

    // It looks like this method is not needed since we handle force updates in UpdateEntityPosition
    //public void UpdateForceEntityPosition(GameObject obj, ForceManager.ForceType type)
    //{
    //    UpdatePosition(obj, typeof(ForceManager.ForceType), (int)type);
    //}

    public List<GameObject> GetEntities(EntityType type)
    {
        return new List<GameObject>(trackers[typeof(EntityType)][(int)type].AllEntities);
    }

    // New: Get entities of type within radius of center (2D circle query)
    public List<GameObject> GetNearbyEntities(EntityType type, Vector2 center, float radius)
    {
        return GetNearby(typeof(EntityType), (int)type, center, radius);
    }

    public List<GameObject> GetNearbyForceEntities(ForceManager.ForceType type, Vector2 center, float radius)
    {
        return GetNearby(typeof(ForceManager.ForceType), (int)type, center, radius);
    }

    // Updated: Use spatial query for closest (more efficient)
    public GameObject FindClosestEntity(EntityType type, Vector3 position)
    {
        Vector2 pos2D = new Vector2(position.x, position.y);
        float minDist = float.MaxValue;
        GameObject closest = null;

        // Use a reasonable radius to limit search; adjust as needed (e.g., map size / 10)
        float searchRadius = 50f; // Tune this based on your map
        foreach (var entity in GetNearbyEntities(type, pos2D, searchRadius))
        {
            float dist = Vector2.Distance(pos2D, entity.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = entity;
            }
        }

        // Fallback to full scan if no nearby (rare)
        if (closest == null)
        {
            closest = GeometryUtils.FindClosestEntity(trackers[typeof(EntityType)][(int)type].AllEntities, position);
        }

        return closest;
    }

    // Debug visualization - draws bounding boxes for occupied cells
    private void OnDrawGizmos()
    {
        if (!EnableGridDebug) return;

        // Collect all unique occupied cells and their total entity counts
        var cellCounts = new Dictionary<Vector2Int, int>();
        foreach (var typeTrackers in trackers.Values)
        {
            foreach (var tracker in typeTrackers.Values)
            {
                foreach (var cell in tracker.Cells.Keys)
                {
                    cellCounts[cell] = cellCounts.GetValueOrDefault(cell, 0) + tracker.Cells[cell].Count;
                }
            }
        }

        if (cellCounts.Count == 0) return;

        int maxCount = cellCounts.Values.Max();

        foreach (var cell in cellCounts.Keys)
        {
            int count = cellCounts[cell];
            float t = maxCount > 1 ? (float)(count - 1) / (maxCount - 1) : 0f;
            Color baseColor = Color.Lerp(Color.green, Color.red, t);
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f);

            Vector3 center = new Vector3(cell.x * CellSize + CellSize / 2f, cell.y * CellSize + CellSize / 2f, 0f);
            Vector3 size = new Vector3(CellSize, CellSize, 0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
    // Helpers
    private Vector2Int GetCellKey(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / CellSize),
            Mathf.FloorToInt(pos.y / CellSize)
        );
    }
    private void AddToCell(EntityTracker tracker, GameObject obj, Vector2Int cell)
    {
        if (!tracker.Cells.TryGetValue(cell, out HashSet<GameObject> cellSet))
        {
            cellSet = new HashSet<GameObject>();
            tracker.Cells[cell] = cellSet;
        }
        cellSet.Add(obj);
    }
    private void RemoveFromCell(EntityTracker tracker, GameObject obj, Vector2Int cell)
    {
        if (tracker.Cells.TryGetValue(cell, out HashSet<GameObject> cellSet))
        {
            cellSet.Remove(obj);
            if (cellSet.Count == 0)
            {
                tracker.Cells.Remove(cell); // Clean up empty cells
            }
        }
    }
    private void AttachPositionTracker(GameObject obj)
    {
        if (obj.GetComponent<Trajectory>() == null)
        {
            obj.AddComponent<Trajectory>();
        }
    }
    private void RemovePositionTracker(GameObject obj)
    {
        var trajectory = obj.GetComponent<Trajectory>();
        if (trajectory != null)
        {
            Destroy(trajectory);
        }
    }
}