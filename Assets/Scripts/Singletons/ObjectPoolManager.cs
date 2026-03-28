using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using System;

public class ObjectPoolManager : AbstractSingleton<ObjectPoolManager>
{
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab; // Prefab to pool
        public int initialSize = 10; // Initial number of objects to create
        public int expansionSize = 5; // Number of objects to add if pool runs out
    }

    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolConfig> configMap = new Dictionary<string, PoolConfig>();
    private Transform poolParent; // Parent transform for inactive objects

    private bool initialized = false;

    protected override void Awake()
    {
        base.Awake(); // Call base singleton setup
        InitializePools();
    }
    private void InitializePools()
    {
        initialized = true;
        // Initialize pools
        poolParent = new GameObject("PooledObjects").transform;
        DontDestroyOnLoad(poolParent);

        foreach (PoolConfig config in poolConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"Pooled object is null: {config.prefab.name}");
                continue;
            }
            PooledObjectProperty property = config.prefab.GetComponent<PooledObjectProperty>();
            if (!property)
            {
                Debug.LogWarning($"Missing pool config: {config.prefab.name}");
                continue;
            }

            // Store config for expansion
            configMap[property.poolId] = config;

            // Create pool
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < config.initialSize; i++)
            {
                GameObject obj = CreatePooledObject(config.prefab);
                pool.Enqueue(obj);
            }
            pools[property.poolId] = pool;
        }
    }

    private GameObject CreatePooledObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        return obj;
    }

    public GameObject Spawn(GameObject obj, Vector3 position, Quaternion rotation)
    {
        if(!initialized) InitializePools();
        GameObject newObject = Instantiate(obj);
        return ActivateObject(newObject, position, rotation);
    }

    public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
    {
        if(!initialized) InitializePools();
        if (!pools.ContainsKey(poolId) || string.IsNullOrEmpty(poolId))
        {
            Debug.LogError($"No pool found for ID: {poolId}");
            return null;
        }

        Queue<GameObject> pool = pools[poolId];
        GameObject obj;

        // If pool is empty, expand it
        if (pool.Count == 0)
        {
            PoolConfig config = configMap[poolId];
            for (int i = 0; i < config.expansionSize; i++)
            {
                obj = CreatePooledObject(config.prefab);
                pool.Enqueue(obj);
            }
            // Debug.Log($"Expanded pool {poolId} by {config.expansionSize} objects");
        }

        // Dequeue and activate object
        obj = pool.Dequeue();
        return ActivateObject(obj, position, rotation);
    }
    public Func<float, float> GetObjectSpeed(string poolId)
    {
        if(!initialized) InitializePools();
        configMap.TryGetValue(poolId, out PoolConfig config);
        if (config == null) throw new Exception($"No pool config found for ID: {poolId}");
        return GetObjectSpeed(config.prefab);
    }
    public Func<float, float> GetObjectSpeed(GameObject obj)
    {
        if(!initialized) InitializePools();
        AutonomousMovementController bulletController = obj.GetComponent<AutonomousMovementController>();
        if (bulletController != null) return bulletController.VelocityFunction;

        // Handle other cases as needed
        return ((float a) => 0);
    }
    private GameObject ActivateObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        // Order, location, set as registered, register, set active, activate observers
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        EntityTypeProperty hasEntityType = obj.GetComponent<EntityTypeProperty>();
        if (hasEntityType)
        {
            hasEntityType.SetRegistered();
            EntityCounter.Instance.RegisterEntity(obj);
        }
        obj.SetActive(true);

        ActivateOnSpawned(obj);
        return obj;
    }
    private void ActivateOnSpawned(GameObject obj)
    {
        ActivateOnSpawn[] activators = obj.GetComponents<ActivateOnSpawn>();
        foreach (var activator in activators)
        {
            activator.Activate();
        }
    }
    private void ActivateOnDespawned(GameObject obj)
    {
        ActivateOnDespawn[] activators = obj.GetComponents<ActivateOnDespawn>();
        foreach (var activator in activators)
        {
            activator.Activate();
        }
    }

    /** Only objects with HasEntityType can be despawned **/
    public void Despawn(GameObject obj)
    {
        if(!initialized) InitializePools();
        ActivateOnDespawned(obj);
        EntityTypeProperty hasEntityType = obj.GetComponent<EntityTypeProperty>();
        if (hasEntityType)
        {
            EntityCounter.Instance.UnregisterEntity(obj);
        }
        Destroy(obj);
    }

    public void Despawn(GameObject obj, string poolId)
    {
        if(!initialized) InitializePools();
        if (!pools.ContainsKey(poolId))
        {
            Debug.LogError($"No pool found for ID: {poolId}. Destroying object.");
            Despawn(obj);
            return;
        }

        // Deactivate and return to pool
        ActivateOnDespawned(obj);
        ReturnObjToPool(obj, poolId);
    }

    private void ReturnObjToPool(GameObject obj, string poolId)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        EntityTypeProperty hasEntityType = obj.GetComponent<EntityTypeProperty>();
        if (hasEntityType)
        {
            EntityCounter.Instance.UnregisterEntity(obj);
        }
        pools[poolId].Enqueue(obj);
    }
}