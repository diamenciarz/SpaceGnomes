using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static EntityTeam;
using static UnityEngine.GraphicsBuffer;

public static class GeometryUtils
{
    public enum SensorType
    {
        Camera,
        Radar
    }
    public class Cone
    {
        public Vector2 origin;
        public Vector2 direction;
        public float angle; // in degrees
        public float maxDistance;
        public Cone(Vector2 origin, Vector2 direction, float angle, float maxDistance)
        {
            this.origin = origin;
            this.direction = direction.normalized;
            this.angle = angle;
            this.maxDistance = maxDistance;
        }
    }
    public static GameObject[] GetVisibleAlliesInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        GameObject[] objects = GetVisibleObjectsInCone(cone, myTeam, sensorType);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies.ToArray();
    }
    public static GameObject[] GetVisibleEnemiesInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        GameObject[] objects = GetVisibleObjectsInCone(cone, myTeam, sensorType);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies.ToArray();
    }
    public static GameObject[] GetVisibleObjectsInCone(Cone cone, Team myTeam, SensorType sensorType=SensorType.Camera)
    {
        List<GameObject> visibleObjects = new List<GameObject>();
        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE); // One ray per degree
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            GameObject hitObject = GetFirstVisibleObject(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, cone.maxDistance, sensorType);
            if (hitObject != null && !visibleObjects.Contains(hitObject))
            {
                visibleObjects.Add(hitObject);
            }
        }
        return visibleObjects.ToArray();
    }
    public static GameObject[] KeepObjectsInCone(Cone cone, GameObject[] gameObjects)
    {
        List<GameObject> objectsInCone = new List<GameObject>();
        foreach (GameObject obj in gameObjects)
        {
            if (IsObjectInCone(obj, cone))
            {
                objectsInCone.Add(obj);
            }
        }
        return objectsInCone.ToArray();
    }
    public static Vector2 AngleToDirectionVector(float zAngle)
    {
        float radians = zAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
    public static float DirectionVectorToAngle(Vector2 direction)
    {
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
    public static bool IsObjectInCone(GameObject target, Cone cone)
    {
        Collider2D collider = target.GetComponent<Collider2D>();
        Vector2 targetPosition = Vector2.zero;
        if (collider)
        {
            targetPosition = collider.ClosestPoint(cone.origin);
            //Debug.DrawLine(cone.origin, targetPosition, Color.green);
        }
        else
        {
            targetPosition = target.transform.position;
        }
        Vector2 toTarget = targetPosition - cone.origin;
        float distanceToTarget = toTarget.magnitude;
        if (distanceToTarget > cone.maxDistance)
        {
            return false;
        }
        // Here we find a point that is the closest to the middle of the cone's direction
        if (collider)
        {
            targetPosition = collider.ClosestPoint(cone.origin + cone.direction.normalized * cone.maxDistance);
            //Debug.DrawLine(cone.origin, targetPosition, Color.green);
        }
        else
        {
            targetPosition = target.transform.position;
        }

        toTarget = targetPosition - cone.origin;
        float angleToTarget = Vector2.Angle(cone.direction, toTarget);
        if (angleToTarget > cone.angle / 2)
        {
            return false;
        }
        return true;
    }
    public static GameObject GetFirstVisibleObject(Vector2 to, Vector2 from, EntityTeam.Team myTeam, float maxDistance=float.MaxValue, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D[] hits = GetVisibleObjects(to, from, maxDistance, sensorType);
        RaycastHit2D[] enemyHits = RemoveAllies(hits, myTeam);
        // Select the closest enemy hit
        float minDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (RaycastHit2D hit in enemyHits)
        {
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestObject = hit.transform.gameObject;
                //Debug.DrawLine(from, hit.point, Color.yellow);
            }
        }
        return closestObject;
    }
    public static bool IsObjectVisible(GameObject target, Vector2 from, EntityTeam.Team myTeam, float maxDistance=float.MaxValue, SensorType sensorType = SensorType.Camera)
    {
        // There is no need to check for allies using Raycast, as they can be checked using distance alone
        RaycastHit2D[] hits = GetVisibleObjects(target.transform.position, from, maxDistance, sensorType);
        float distanceToTarget = -1;
        // Find distance to target, so it can be compared with other objects in the ray's path
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.gameObject.Equals(target))
            {
                distanceToTarget = hit.distance;
            }
        }
        if (distanceToTarget == -1) return false;

        // Allies do not block the view
        RaycastHit2D[] enemyHits = RemoveAllies(hits, myTeam);
        foreach (RaycastHit2D hit in enemyHits)
        {
            if (hit.distance < distanceToTarget)
            {
                return false;
            }
        }
        return true;
    }
    private static RaycastHit2D[] RemoveAllies(RaycastHit2D[] hits, Team myTeam)
    {
        List<RaycastHit2D> nonAllies = new List<RaycastHit2D>();
        foreach (RaycastHit2D hit in hits)
        {
            Team objectTeam = TeamManager.Instance.GetEntityTeam(hit.transform.gameObject);
            // Remove allies -> do not add them to the enemies list
            if (!TeamManager.Instance.IsAlly(objectTeam, myTeam))
            {
                nonAllies.Add(hit);
            }
        }
        return nonAllies.ToArray();
    }
    public static RaycastHit2D[] GetVisibleObjects(Vector2 to, Vector2 from, float maxDistance=float.MaxValue, SensorType sensorType=SensorType.Camera)
    {
        return Physics2D.RaycastAll(from, to-from, maxDistance, GetSensorMask(sensorType));
    }
    private static int GetSensorMask(SensorType sensorType)
    {
        switch (sensorType)
        {
            case SensorType.Camera:
                return LayerMask.GetMask(new string[] { "VisionBlocking" });
            case SensorType.Radar:
                return LayerMask.GetMask(new string[] { "VisionBlocking", "RadarBlocking" });
            default:
                return LayerMask.GetMask(new string[] { "VisionBlocking", "RadarBlocking" });
        }
    }
    public static Vector2 GetMousePosition(bool worldCoords = true)
    {
        if (worldCoords)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return new Vector2(worldPos.x, worldPos.y);
        }
        else
        {
            return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    public static float ClampAngle180(float angle)
    {
        // If an Euler angle is outside the range <-180,180> bring it into it
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
    public static Vector2 CalculateVectorBetweenColliderEdges(GameObject to, GameObject from)
    {
        Collider2D colliderB = from.GetComponent<Collider2D>();
        Collider2D colliderA = to.GetComponent<Collider2D>();
        if (colliderA == null || colliderB == null)
        {
            return to.transform.position - from.transform.position;
        }
        Vector2 closestPointA = colliderA.ClosestPoint(colliderB.transform.position);
        Vector2 closestPointB = colliderB.ClosestPoint(colliderA.transform.position);
        //Debug.DrawLine(closestPointA, closestPointB, Color.cyan);

        return closestPointB - closestPointA;
    }
    public static GameObject FindClosestEntityToPosition(IEnumerable<GameObject> entities, Vector2 position, float minRange = 0f, float maxRange = float.MaxValue)
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
