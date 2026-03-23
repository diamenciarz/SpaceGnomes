using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Instances")]
    [SerializeField] List<string> objectsToGenerate;
    [SerializeField] List<float> weights;
    [SerializeField] List<float> maxVelocities;
    [SerializeField] int OBSTACLE_LIMIT = 100;

    [Header("Generation settings")]
    [SerializeField] float delay = 10;
    [SerializeField][Range(0, 10)][Tooltip("The minimum number of objects generated after each delay")] int minGeneratedObjects = 2;
    [SerializeField][Range(1, 20)][Tooltip("The maximum number of objects generated after each delay")] int maxGeneratedObjects = 5;
    [Header("Generated coordinate settings")]
    [SerializeField][Range(0,100)][Tooltip("The distance in game units that the generated objects will spawn outside the map")] float outOfMapOffset = 10;
    [SerializeField][Range(0, 90)][Tooltip("The +- deviation from the default direction in which objects will fly")] float generatedAngleSpread = 60f;
    
    
    private Coroutine generationCoroutine;

    private void Start()
    {
        generationCoroutine = StartCoroutine(GenerationLoop());
    }
    private IEnumerator GenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);

            int n = Random.Range(minGeneratedObjects, maxGeneratedObjects);
            for (int i = 0; i < n; i++)
            {
                GenerateRandomObj();
            }
        }
    }
    private void GenerateRandomObj()
    {
        if (EntityCounter.Instance.GetEntities(EntityTypeProperty.EntityType.SpaceDebris).Count > OBSTACLE_LIMIT)
        {
            return;
        }

        int objectIndex = MathUtils.GetWeightedIndex(weights);
        string poolId = objectsToGenerate[objectIndex];
        GenerationSide side = GetRandomSide();
        ObjectPoolManager.Instance.Spawn(poolId, GetRandomPosition(side), GetRandomDirection(side));
    }
    enum GenerationSide { Top, Bottom, Left, Right }
    private GenerationSide GetRandomSide()
    {
        float sum = MapInformation.Instance.mapWidth + MapInformation.Instance.mapHeight;
        bool generateFromCeil = Random.Range(0f, sum) > MapInformation.Instance.mapWidth;
        if (generateFromCeil)
        {
            if (Random.Range(0, 2) == 0) return GenerationSide.Top;
            return GenerationSide.Bottom;
        }
        else
        {
            if (Random.Range(0, 2) == 0) return GenerationSide.Right;
            return GenerationSide.Left;
        }
    }
    private Vector2 GetRandomPosition(GenerationSide side)
    {
        if(side == GenerationSide.Top)
        {
            return MapInformation.Instance.GetMapPercentagePosition(Random.Range(0, 1f), 1) + new Vector2(0, outOfMapOffset);
        }
        else if(side == GenerationSide.Bottom)
        {
            return MapInformation.Instance.GetMapPercentagePosition(Random.Range(0, 1f), 0) - new Vector2(0, outOfMapOffset);
        }
        else if(side == GenerationSide.Right)
        {
            return MapInformation.Instance.GetMapPercentagePosition(1, Random.Range(0, 1f)) + new Vector2(outOfMapOffset, 0);
        }
        else // Left
        {
            return MapInformation.Instance.GetMapPercentagePosition(0, Random.Range(0, 1f)) - new Vector2(outOfMapOffset, 0);
        }
    }
    private Quaternion GetRandomDirection(GenerationSide side)
    {
        float defaultDirection = GetDefaultDirection(side);
        return Quaternion.Euler(0, 0, Random.Range(defaultDirection - generatedAngleSpread, defaultDirection + generatedAngleSpread));
    }
    private float GetDefaultDirection(GenerationSide side)
    {
        if (side == GenerationSide.Top) return 180;
        if (side == GenerationSide.Bottom) return 0;
        if (side == GenerationSide.Right) return 90;
        if (side == GenerationSide.Left) return 270;
        return 0;
    }
    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public virtual void OnBeforeSerialize()
    {
        ControlListLengths();
    }
    private void ControlListLengths()
    {
        if (weights.Count < objectsToGenerate.Count)
        {
            weights.Add(0);
        }
        if (weights.Count > objectsToGenerate.Count)
        {
            weights.RemoveAt(weights.Count - 1);
        }
        if (maxVelocities.Count < objectsToGenerate.Count)
        {
            maxVelocities.Add(0);
        }
        if (maxVelocities.Count > objectsToGenerate.Count)
        {
            maxVelocities.RemoveAt(maxVelocities.Count - 1);
        }
        if (minGeneratedObjects > maxGeneratedObjects)
        {
            minGeneratedObjects = maxGeneratedObjects;
        }
    }
    #endregion
}
