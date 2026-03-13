using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector2 CalculateAverageVector(List<Vector2> vectors)
    {
        return CalculateAverageVector(vectors, 0, vectors.Count);
    }
    public static Vector2 CalculateAverageVector(List<Vector2> vectors, int startIndex, int endIndex)
    {
        if (vectors.Count == 0) return Vector2.zero;
        Vector2 sum = Vector2.zero;
        vectors.GetRange(startIndex, endIndex).ForEach(v => sum += v);
        return sum / vectors.Count;
    }
    public static Vector2 CalculateAverageAcceleration(List<Vector2> velocities, List<float> deltaTimes)
    {
        return CalculateAverageAcceleration(velocities, deltaTimes, 0, velocities.Count);
    }
    public static Vector2 CalculateAverageAcceleration(List<Vector2> velocities, List<float> deltaTimes, int startIndex, int count)
    {
        // Skip the first deltaTime
        // Take first difference v1 - v0
        if (velocities.Count == 0 || deltaTimes.Count == 0) return Vector2.zero;
        Vector2 accSum = Vector2.zero;
        Vector2 lastV = velocities[startIndex];
        for (int i = 1; i < count; i++)
        {
            accSum += (velocities[i] - lastV) / deltaTimes[i];
            lastV = velocities[i];
        }
        return accSum / (count-1-startIndex);
    }
}
