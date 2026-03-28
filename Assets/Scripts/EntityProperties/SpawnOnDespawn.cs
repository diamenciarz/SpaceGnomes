using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for spawning entities when the object is despawned from the pool.
/// It uses weighted random selection to determine which group of entities to spawn, and it can also target the closest visible enemy based on the provided sensors.
/// </summary>
public class SpawnOnDespawn : ActivateOnDespawn
{
    [SerializeField] private List<InstantiableGroup> instantiateGroups = new List<InstantiableGroup>();
    [SerializeField] private List<float> weights = new List<float>();
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector2 minOffset = Vector2.zero;
    [SerializeField] private Vector2 maxOffset = Vector2.zero;
    [SerializeField] private float minDirection = 0f;
    [SerializeField] private float maxDirection = 360f;
    [Header("Target Settings")]
    [SerializeField] List<AbstractSensor> sensors = new List<AbstractSensor>();

    /// <summary>
    /// This method is called when the object is despawned from the pool.
    /// </summary>
    public override void Activate()
    {
        int index = MathUtils.GetWeightedIndex(weights);
        InstantiableGroup selectedGroup = instantiateGroups[index];
        
        // Compute random offset and direction
        selectedGroup.Spawn(GetSpawnPosition(), GetSpawnRotation(), GetTarget());
    }
    private GameObject GetTarget()
    {
        List<GameObject> possibleTargets = new List<GameObject>();
        foreach (AbstractSensor sensor in sensors)
        {
            possibleTargets.Add(sensor.GetClosestVisibleEnemy());
        }
        return GeometryUtils.FindClosestEntityToObject(possibleTargets, gameObject);
    }
    private Quaternion GetSpawnRotation()
    {
        float randomDirection = Random.Range(minDirection, maxDirection);
        return spawnPoint.transform.rotation * Quaternion.Euler(0f, 0f, randomDirection);
    }
    private Vector2 GetSpawnPosition()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(minOffset.x, maxOffset.x),
            Random.Range(minOffset.y, maxOffset.y),
            0f
        );
        return spawnPoint.position + randomOffset;
    }
    private void OnValidate()
    {
        if(weights.Count > instantiateGroups.Count)
        {
            weights.RemoveRange(instantiateGroups.Count, weights.Count - instantiateGroups.Count);
        }
        if(weights.Count < instantiateGroups.Count)
        {
            while(weights.Count < instantiateGroups.Count) weights.Add(1f);
        }
    }
}
