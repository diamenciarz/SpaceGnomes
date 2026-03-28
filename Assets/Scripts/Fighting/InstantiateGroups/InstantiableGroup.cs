using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstantiableGroup : ScriptableObject
{
    [Serializable]
    public class Instantiable
    {
        /// <summary>
        /// The poolId to spawn from. This should correspond to a poolId in the ObjectPoolManager.
        /// </summary>
        public string poolId;
        /// <summary>
        /// If an object is not pooled, you can directly specify a prefab to spawn. This will bypass the pooling system and instantiate a new object each time.
        /// This should be a single object prefab, not a group prefab.
        /// </summary>
        public GameObject prefab;
    }
    
    protected class InstantiableData
    {
        public Instantiable instantiable;
        /// <summary>
        /// This is the angle in degrees from the forward direction of the base rotation.
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// This is the offset from the center position in the direction perpendicular to the forward direction of the base rotation.
        /// Positive values are to the right, negative values are to the left.
        /// </summary>
        public Vector2 offset;
    }
    protected abstract List<InstantiableData> GetObjectFormation();

    /// <summary>
    /// Spawns the group of objects at the specified position and rotation. Each object will be offset by the corresponding angular and sideways offsets.
    /// </summary>
    /// <param name="basePosition"></param>
    /// <param name="baseRotation"></param>
    /// <returns>A list of the spawned GameObjects.</returns>
    public List<GameObject> Spawn(Vector2 basePosition, Quaternion baseRotation) => Spawn(basePosition, baseRotation, null);
    public List<GameObject> Spawn(Vector2 basePosition, Quaternion baseRotation, GameObject target)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();
        foreach (InstantiableData objectData in GetObjectFormation())
        {
            Quaternion relativeRotation = baseRotation * objectData.rotation;
            Vector2 relativePosition = CalculatePositionInDirection(basePosition, objectData.offset, baseRotation);
            GameObject spawned = SpawnInstantiable(objectData.instantiable, relativePosition, relativeRotation);

            if(target && spawned.TryGetComponent<ISettableTarget>(out var settableTarget)) settableTarget.SetTarget(target);
            if (spawned) spawnedObjects.Add(spawned);
        }
        return spawnedObjects;
    }
    /// <summary>
    /// Calculates the position of an object given a base position, and an offset in the direction of the base rotation.
    /// </summary>
    /// <param name="basePosition"></param>
    /// <param name="offset"></param>
    /// <param name="baseRotation"></param>
    /// <returns></returns>
    private Vector2 CalculatePositionInDirection(Vector2 basePosition, Vector2 offset, Quaternion baseRotation)
    {
        Vector2 rotatedVector = GeometryUtils.RotateVector(offset, baseRotation.eulerAngles.z);
        return basePosition + rotatedVector;
    }
    private GameObject SpawnInstantiable(Instantiable instantiable, Vector3 position, Quaternion rotation)
    {
        if (instantiable.poolId == "" && !instantiable.prefab)
        {
            Debug.LogError("Instantiable has neither poolId nor prefab set. Skipping spawn.");
            return null;
        }
        if(instantiable.prefab)
        {
            return ObjectPoolManager.Instance.Spawn(instantiable.prefab, position, rotation);
        }
        return ObjectPoolManager.Instance.Spawn(instantiable.poolId, position, rotation);
    }
}
