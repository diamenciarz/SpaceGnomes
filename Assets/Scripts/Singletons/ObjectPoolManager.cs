using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using System;

public class ObjectPoolManager : AbstractSingleton<ObjectPoolManager>
{
    [System.Serializable]
    public class PoolConfig
    {
        public string poolId; // Unique identifier for the pool
        public GameObject prefab; // Prefab to pool
        public int initialSize = 10; // Initial number of objects to create
        public int expansionSize = 5; // Number of objects to add if pool runs out
    }

    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolConfig> configMap = new Dictionary<string, PoolConfig>();
    private Transform poolParent; // Parent transform for inactive objects

    protected override void Awake()
    {
        base.Awake(); // Call base singleton setup

        // Initialize pools
        poolParent = new GameObject("PooledObjects").transform;
        DontDestroyOnLoad(poolParent);

        foreach (var config in poolConfigs)
        {
            if (config.prefab == null || string.IsNullOrEmpty(config.poolId))
            {
                Debug.LogWarning($"Invalid pool config: {config.poolId}");
                continue;
            }

            // Store config for expansion
            configMap[config.poolId] = config;

            // Create pool
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < config.initialSize; i++)
            {
                GameObject obj = CreatePooledObject(config.prefab);
                pool.Enqueue(obj);
            }
            pools[config.poolId] = pool;
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
        GameObject newObject = Instantiate(obj);
        return ActivateObject(newObject, position, rotation);
    }

    public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
    {
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
    public float GetInitialObjectSpeed(string poolId)
    {
        configMap.TryGetValue(poolId, out PoolConfig config);
        if (config == null) throw new Exception($"No pool config found for ID: {poolId}");
        return GetInitialObjectSpeed(config.prefab);
    }
    public float GetInitialObjectSpeed(GameObject obj)
    {
        BulletController bulletController = obj.GetComponent<BulletController>();
        if (bulletController != null) return bulletController.InitialVelocity;

        // Handle other cases as needed

        return 0;
    }
    private GameObject ActivateObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        // Order, location, set as registered, register, set active, activate observers
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
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
        ActivateOnDespawned(obj);
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType)
        {
            EntityCounter.Instance.UnregisterEntity(obj);
        }
        Destroy(obj);
    }

    public void Despawn(GameObject obj, string poolId)
    {
        if (!pools.ContainsKey(poolId))
        {
            Debug.LogWarning($"No pool found for ID: {poolId}. Destroying object.");
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
        HasEntityType hasEntityType = obj.GetComponent<HasEntityType>();
        if (hasEntityType)
        {
            EntityCounter.Instance.UnregisterEntity(obj);
        }
        pools[poolId].Enqueue(obj);
    }
}