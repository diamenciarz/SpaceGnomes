using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    public static GameObject FindClosestEntity(IEnumerable<GameObject> entities, Vector2 position, float minRange = 0f, float maxRange = float.MaxValue)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in entities)
        {
            Vector2 entityPos = entity.transform.position;
            float distance = Vector2.Distance(position, entityPos);
            if (distance >= minRange && distance <= maxRange && distance < minDistance)
            {
                minDistance = distance;
                closest = entity;
            }
        }
        return closest;
    }

    public static GameObject FindEntityAtClosestAngle(IEnumerable<GameObject> entities, Vector2 position, Vector2 direction, float minRange = 0f, float maxRange = float.MaxValue)
    {
        GameObject closest = null;
        float minAngle = float.MaxValue;
        direction = direction.normalized;

        foreach (var entity in entities)
        {
            Vector2 entityPos = entity.transform.position;
            Vector2 toEntity = entityPos - position;
            float distance = toEntity.magnitude;
            if (distance >= minRange && distance <= maxRange && toEntity != Vector2.zero)
            {
                toEntity = toEntity.normalized;
                float angle = Vector2.Angle(direction, toEntity);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    closest = entity;
                }
            }
        }
        return closest;
    }
}
