using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    public static GameObject FindClosestEntity(IEnumerable<GameObject> entities, Vector3 position)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in entities)
        {
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
