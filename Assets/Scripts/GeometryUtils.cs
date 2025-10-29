using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    public static GameObject FindClosestEntity(IEnumerable<GameObject> entities, Vector2 position)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in entities)
        {
            Vector2 entityPos = entity.transform.position;
            float distance = Vector2.Distance(position, entityPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = entity;
            }
        }
        return closest;
    }

    public static GameObject FindEntityAtClosestAngle(IEnumerable<GameObject> entities, Vector2 position, Vector2 direction)
    {
        GameObject closest = null;
        float minAngle = float.MaxValue;
        direction = direction.normalized;

        foreach (var entity in entities)
        {
            Vector2 entityPos = entity.transform.position;
            Vector2 toEntity = entityPos - position;
            if (toEntity == Vector2.zero) continue; // Skip if at exact position
            toEntity = toEntity.normalized;
            float angle = Vector2.Angle(direction, toEntity);
            if (angle < minAngle)
            {
                minAngle = angle;
                closest = entity;
            }
        }
        return closest;
    }
}
