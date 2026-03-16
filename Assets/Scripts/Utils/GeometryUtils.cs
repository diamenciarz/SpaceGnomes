using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using static EntityTeam;
using static Line2D;
using System;

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
    public static List<GameObject> GetVisibleAlliesInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<GameObject> objects = GetVisibleObjectsInCone(cone, myTeam, sensorType);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies;
    }
    public static List<GameObject> GetVisibleEnemiesInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<GameObject> objects = GetVisibleObjectsInCone(cone, myTeam, sensorType);
        //foreach (var item in objects)
        //{
        //    Debug.Log("Seeing " + item.gameObject.name);
        //}
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies;
    }
    public static List<GameObject> GetVisibleAlliesInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<GameObject> objects = GetVisibleObjectsInCone(cone, myTeam, entityTypes, sensorType);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies;
    }
    public static List<GameObject> GetVisibleEnemiesInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<GameObject> objects = GetVisibleObjectsInCone(cone, myTeam, entityTypes, sensorType);
        //foreach (var item in objects)
        //{
        //    Debug.Log("Seeing " + item.gameObject.name);
        //}
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(obj), myTeam))
            {
                enemies.Add(obj);
            }
        }
        return enemies;
    }
    public static List<GameObject> GetVisibleObjectsInCone(Cone cone, Team myTeam, SensorType sensorType=SensorType.Camera)
    {
        List<GameObject> visibleObjects = new List<GameObject>();
        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleObject(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!visibleObjects.Contains(hit.Value.collider.gameObject))
            {
                visibleObjects.Add(hit.Value.collider.gameObject);
            }
        }
        return visibleObjects;
    }
    public static List<GameObject> GetVisibleObjectsInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<GameObject> visibleObjects = new List<GameObject>();
        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleEntity(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, entityTypes, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!visibleObjects.Contains(hit.Value.collider.gameObject))
            {
                visibleObjects.Add(hit.Value.collider.gameObject);
            }
        }
        return visibleObjects;
    }
    public static GameObject GetClosestVisibleAllyInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleEntity(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, entityTypes, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance)
            {
                if (TeamManager.Instance.IsAlly(myTeam, TeamManager.Instance.GetEntityTeam(hit.Value.collider.gameObject)))
                {
                    closestHit = hit;
                }
            }
        }
        return closestHit.Value.collider.gameObject;
    }
    public static GameObject GetClosestVisibleAllyInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleObject(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance)
            {
                if (TeamManager.Instance.IsAlly(myTeam, TeamManager.Instance.GetEntityTeam(hit.Value.collider.gameObject)))
                {
                    closestHit = hit;
                }
            }
        }
        return closestHit.Value.collider.gameObject;
    }
    public static GameObject GetClosestVisibleEnemyInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleEntity(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, entityTypes, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance)
            {
                if(TeamManager.Instance.IsEnemy(myTeam, TeamManager.Instance.GetEntityTeam(hit.Value.collider.gameObject)))
                {
                    closestHit = hit;
                }
            }
        }
        return closestHit.Value.collider.gameObject;
    }
    public static GameObject GetClosestVisibleEnemyInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleObject(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance)
            {
                if (TeamManager.Instance.IsEnemy(myTeam, TeamManager.Instance.GetEntityTeam(hit.Value.collider.gameObject)))
                {
                    closestHit = hit;
                }
            }
        }
        return closestHit.Value.collider.gameObject;
    }
    public static GameObject GetClosestVisibleObjectInCone(Cone cone, Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleEntity(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, entityTypes, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance) closestHit = hit;
        }
        return closestHit.Value.collider.gameObject;
    }
    public static GameObject GetClosestVisibleObjectInCone(Cone cone, Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D? closestHit = null;

        float RAYS_PER_DEGREE = 0.5f;
        int rayCount = Mathf.CeilToInt(cone.angle * RAYS_PER_DEGREE);
        float angleStep = cone.angle / rayCount;
        float startAngle = -cone.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * cone.direction;
            RaycastHit2D? hit = GetFirstVisibleObject(cone.origin + rayDirection * cone.maxDistance, cone.origin, myTeam, cone.maxDistance, sensorType);
            if (!hit.HasValue) continue;
            if (!closestHit.HasValue || hit.Value.distance < closestHit.Value.distance) closestHit = hit;
        }
        return closestHit.Value.collider.gameObject;
    }
    public static List<GameObject> GetVisibleAllies(Vector2 from, Team myTeam, List<HasEntityType.EntityType> entityTypes, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera, GameObject ignoreObject = null)
    {
        List<GameObject> allies = TeamManager.Instance.GetNearbyAllies(from, myTeam, entityTypes, maxDistance);
        return KeepVisibleObjects(from, allies, myTeam, maxDistance, sensorType, ignoreObject);
    }
    public static List<GameObject> GetVisibleEnemies(Vector2 from, Team myTeam, List<HasEntityType.EntityType> entityTypes, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera, GameObject ignoreObject = null)
    {
        List<GameObject> enemies = TeamManager.Instance.GetNearbyEnemies(from, myTeam, entityTypes, maxDistance);
        return KeepVisibleObjects(from, enemies, myTeam, maxDistance, sensorType, ignoreObject);
    }
    public static List<GameObject> GetVisibleObjects(Vector2 from, Team myTeam, List<HasEntityType.EntityType> entityTypes, float maxDistance=float.MaxValue, SensorType sensorType = SensorType.Camera, GameObject ignoreObject = null)
    {
        List<GameObject> entities = EntityCounter.Instance.GetNearbyEntities(from, entityTypes, maxDistance);
        return KeepVisibleObjects(from, entities, myTeam, maxDistance, sensorType, ignoreObject);
    }
    public static List<GameObject> KeepObjectsInCone(Cone cone, List<GameObject> gameObjects)
    {
        List<GameObject> objectsInCone = new List<GameObject>();
        foreach (GameObject obj in gameObjects)
        {
            if (IsObjectInCone(obj, cone))
            {
                objectsInCone.Add(obj);
            }
        }
        return objectsInCone;
    }
    public static List<GameObject> KeepVisibleObjects(Vector2 from, List<GameObject> gameObjects, Team myTeam, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera, GameObject ignoreObject=null)
    {
        List<GameObject> visibleObjects = new List<GameObject>();
        foreach (GameObject obj in gameObjects)
        {
            if (IsObjectVisible(obj, from, myTeam, maxDistance, sensorType, ignoreObject))
            {
                visibleObjects.Add(obj);
            }
        }
        return visibleObjects;
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
    public static bool PointBetweenTwoPoints(Vector2 point, Vector2 A, Vector2 B)
    {
        float AB = Vector2.Distance(A, B);
        float pA = Vector2.Distance(point, A);
        float pB = Vector2.Distance(point, B);
        return Mathf.Abs((pA + pB) - AB) < AB * 0.01f;
        //return Mathf.Approximately(AB, pA + pB);
    }
    public static RaycastHit2D? GetFirstVisibleObject(Vector2 to, Vector2 from, EntityTeam.Team myTeam, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D[] hits = RaycastInLine(to, from, maxDistance, sensorType);
        if (hits.Length == 0) return null;

        RaycastHit2D[] enemyHits = RemoveAllies(hits, myTeam);
        // Select the closest enemy hit
        RaycastHit2D? closestHit = null;
        foreach (RaycastHit2D hit in enemyHits)
        {
            if (closestHit.HasValue && hit.distance >= closestHit.Value.distance) continue;
            closestHit = hit;
        }
        return closestHit;
    }
    public static RaycastHit2D? GetFirstVisibleEntity(Vector2 to, Vector2 from, EntityTeam.Team myTeam, HasEntityType.EntityType[] entityTypes, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera)
    {
        RaycastHit2D[] hits = RaycastInLine(to, from, maxDistance, sensorType);
        if (hits.Length == 0) return null;

        RaycastHit2D[] enemyHits = RemoveAllies(hits, myTeam);
        // Select the closest enemy hit
        RaycastHit2D? closestHit = null;
        foreach (RaycastHit2D hit in enemyHits)
        {
            if (closestHit.HasValue && hit.distance >= closestHit.Value.distance) continue;
            HasEntityType entityTypeComponent = hit.collider.GetComponent<HasEntityType>();
            if (entityTypeComponent == null) continue;
            if (!entityTypes.Contains(entityTypeComponent.Type)) continue;
            closestHit = hit;
        }
        return closestHit;
    }
    public static bool IsObjectVisible(GameObject target, Vector2 from, EntityTeam.Team myTeam, float maxDistance=float.MaxValue, SensorType sensorType = SensorType.Camera, GameObject ignoreObject = null)
    {
        // There is no need to check for allies using Raycast, as they can be checked using distance alone
        RaycastHit2D[] hits = RaycastInLine(target.transform.position, from, maxDistance, sensorType);
        float distanceToTarget = -1;
        // Find distance to target, so it can be compared with other objects in the ray's path
        foreach (RaycastHit2D hit in hits)
        {
            //Debug.DrawLine(from, hit.point, Color.white);
            if (hit.collider.gameObject.Equals(target))
            {
                distanceToTarget = hit.distance;
            }
        }
        if (distanceToTarget == -1) return false;

        // Allies do not block the view
        RaycastHit2D[] enemyHits = RemoveAllies(hits, myTeam, ignoreObject);
        foreach (RaycastHit2D hit in enemyHits)
        {
            if (hit.distance < distanceToTarget-0.001f)
            {
                return false;
            }
        }
        return true;
    }
    private static RaycastHit2D[] RemoveAllies(RaycastHit2D[] hits, Team myTeam, GameObject ignoreObject = null)
    {
        List<RaycastHit2D> nonAllies = new List<RaycastHit2D>();
        foreach (RaycastHit2D hit in hits)
        {
            // Remove ignored object
            if (ignoreObject != null && hit.transform.gameObject.Equals(ignoreObject)) continue;
            
            Team objectTeam = TeamManager.Instance.GetEntityTeam(hit.transform.gameObject);
            // Remove allies -> do not add them to the enemies list
            if (!TeamManager.Instance.IsAlly(objectTeam, myTeam))
            {
                nonAllies.Add(hit);
            }
        }
        return nonAllies.ToArray();
    }
    public static RaycastHit2D[] RaycastInDir(Vector2 from, Vector2 dir, float maxDistance = float.MaxValue, SensorType sensorType = SensorType.Camera)
    {
        return Physics2D.RaycastAll(from, dir, maxDistance, GetSensorMask(sensorType));
    }
    public static RaycastHit2D[] RaycastInLine(Vector2 to, Vector2 from, float maxDistance=float.MaxValue, SensorType sensorType=SensorType.Camera)
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
    public struct CollidingPoints
    {
        public Vector2 toPoint;
        public Vector2 fromPoint;
    }

    public static CollidingPoints CalculateClosestDistanceBetweenColliders(GameObject to, GameObject from)
    {
        Collider2D colliderFrom = GeometryUtils.GetNonCompositeCollider(from);
        Collider2D colliderTo = GeometryUtils.GetNonCompositeCollider(to);
        CollidingPoints points = new CollidingPoints();
        if (colliderTo == null || colliderFrom == null)
        {
            points.toPoint = to.gameObject.transform.position;
            points.fromPoint = from.gameObject.transform.position;
            return points;
        }
        points.toPoint = colliderTo.ClosestPoint(colliderFrom.gameObject.transform.position);
        points.fromPoint = colliderFrom.ClosestPoint(colliderTo.gameObject.transform.position);
        return points;
    }
    public static Vector2 CalculateVectorBetweenColliderEdges(GameObject to, GameObject from)
    {
        CollidingPoints points = CalculateClosestDistanceBetweenColliders(to, from);
        return points.toPoint - points.fromPoint;
    }
    public static GameObject FindClosestEntityToPosition(IEnumerable<GameObject> entities, Vector2 position, float minRange = 0f, float maxRange = float.MaxValue)
    {
        GameObject tempObject = EntityCounter.Instance.GetDummyPointObject();
        tempObject.transform.position = position;
        return FindClosestEntityToObject(entities, tempObject, minRange, maxRange);
    }
    public static GameObject FindClosestEntityToObject(IEnumerable<GameObject> entities, GameObject obj, float minRange = 0f, float maxRange = float.MaxValue)
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var entity in entities)
        {
            Vector2 entityPos = entity.transform.position;
            CollidingPoints collidingPoints = CalculateClosestDistanceBetweenColliders(obj, entity);
            float distance = Vector2.Distance(collidingPoints.fromPoint, collidingPoints.toPoint);
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
    public static Vector2 CalculateTrajectoryHitCoordinates(Trajectory targetTrajectory, Vector2 startingPosition, float projectileSpeed, int maxIterations=5)
    {
        if (projectileSpeed <= 0f) return targetTrajectory.GetCurrentPosition();

        Vector2 targetPosition = targetTrajectory.GetCurrentPosition();
        float distanceToTarget = (targetPosition - startingPosition).magnitude;
        float deltaTime = distanceToTarget / projectileSpeed;
        for (int i = 0; i < maxIterations; i++)
        {
            if (distanceToTarget <= 0.05f) return targetPosition;
            targetPosition = targetTrajectory.ExtrapolateFuturePosition(deltaTime);
            distanceToTarget = (targetPosition - startingPosition).magnitude;
            deltaTime = distanceToTarget / projectileSpeed;
        }
        return targetPosition;
    }
    public static Collider2D GetNonCompositeCollider(GameObject obj)
    {
        Collider2D[] collider2Ds = obj.GetComponents<Collider2D>();
        foreach(Collider2D collider in collider2Ds)
        {
            if(collider.usedByComposite) continue;
            return collider;
        }
        return null;
    }
    public static List<Vector2> GetForwardmostPoints(Vector2[] points, Vector2 direction, int collisionCheckPointCount)
    {
        if (collisionCheckPointCount >= points.Length) return points.ToList();

        List<Vector2> centeredPoints = GetCenteredPoints(points);
        // Use centered points for the dot product

        List<Vector2> forwardmostPoints = new List<Vector2> { points[0] };
        List<float> angles = new List<float> { Vector2.Angle(centeredPoints[0], direction.normalized) };
        for (int i = 1; i < points.Length; i++)
        {
            bool wasAdded = false;
            float angle = Vector2.Angle(centeredPoints[i], direction.normalized);
            for (int j = 0; j < forwardmostPoints.Count; j++)
            {
                if (angle < angles[j])
                {
                    wasAdded = true;

                    float putAngle = angle;
                    Vector2 putPoint = points[i];

                    float heldAngle = 0;
                    Vector2 heldPoint = Vector2.zero;

                    for (int k = j; k < forwardmostPoints.Count; k++)
                    {
                        heldAngle = angles[k];
                        heldPoint = forwardmostPoints[k];
                        angles[k] = putAngle;
                        forwardmostPoints[k] = putPoint;

                        putAngle = heldAngle;
                        putPoint = heldPoint;

                    }
                    if (forwardmostPoints.Count < collisionCheckPointCount)
                    {
                        angles.Insert(angles.Count, putAngle);
                        forwardmostPoints.Insert(forwardmostPoints.Count, putPoint);
                    }
                    break;
                }
            }
            if (!wasAdded && forwardmostPoints.Count < collisionCheckPointCount)
            // If we have reached the end of the list without adding the point, add the new point to the list if there is still space
            {
                angles.Add(angle);
                forwardmostPoints.Add(points[i]);
            }
        }
        return forwardmostPoints;
    }
    public struct LineCrossingInfo
    {
        public Vector2 crossPoint;
        public float crossTimeA;
        public float crossTimeB;
        public Vector2 pointA;
        public Vector2 pointB;
    }
    public static LineCrossingInfo? CalculateLineCrossing(Vector2 startA, Vector2 velocityA, Vector2 startB, Vector2 velocityB, float maxTime = float.MaxValue)
    {
        // Find intersection of the lines: startA + velocityA * t and startB + velocityB * s
        Vector2 delta = startB - startA;

        // Determinant for 2D cross product
        float D = velocityA.x * velocityB.y - velocityB.x * velocityA.y;

        if (Mathf.Approximately(D, 0f))
        {
            // Lines are parallel, no intersection
            return null;
        }

        // Solve for t and s
        float t = (delta.x * velocityB.y - velocityB.x * delta.y) / D;
        float s = (delta.x * velocityA.y - velocityA.x * delta.y) / D;

        // Check if intersection is in the future (t > 0 and s > 0)
        if (t <= 0 || s <= 0)
        {
            return null;
        }

        // Check if at least one time is within maxTime
        if (t > maxTime && s > maxTime)
        {
            return null;
        }

        // Calculate the crossing point
        Vector2 crossPoint = startA + velocityA * t;

        return new LineCrossingInfo { crossPoint = crossPoint, crossTimeA = t, crossTimeB = s };
    }
    public struct ColliderPoints
    {
        public Vector2[] points;
    }
    public static ColliderPoints? CalculateColliderCrossection(Collider2D collider2D, Vector2 direction, bool getAllPoints=false)
    {
        direction = direction.normalized;
        List<Vector2> points = GetColliderPoints(collider2D, direction);

        if (points.Count == 0)
        {
            return null;
        }
        // Center the points at zero
        if (getAllPoints)
        {
            return new ColliderPoints { points = points.ToArray() };
        }

        Vector2[] frontBackPoints = CalculateCrossectionPoints(points, direction);
        Vector2[] leftRightPoints = CalculateCrossectionPoints(points, Vector2.Perpendicular(direction));

        return new ColliderPoints { points = new Vector2[4] { frontBackPoints[0], frontBackPoints[1], leftRightPoints[0], leftRightPoints[1] } };
    }
    private static List<Vector2> GetCenteredPoints(Vector2[] points)
    {
        Vector2 middlePoint = Vector2.zero;

        foreach (Vector2 point in points)
        {
            middlePoint += point;
        }
        middlePoint /= points.Length;

        List<Vector2> centeredPoints = new List<Vector2>();
        for (int i = 0; i < points.Length; i++)
        {
            centeredPoints.Add(points[i] - middlePoint);
        }
        return centeredPoints;
    }
    private static Vector2[] CalculateCrossectionPoints(List<Vector2> points, Vector2 direction)
    {
        float minDot = float.MaxValue;
        float maxDot = float.MinValue;
        Vector2 backPoint = points[0];
        Vector2 frontPoint = points[0];
        foreach (Vector2 point in points)
        {
            float dot = Vector2.Dot(point, direction);
            if (dot < minDot)
            {
                minDot = dot;
                backPoint = point;
            }
            if (dot > maxDot)
            {
                maxDot = dot;
                frontPoint = point;
            }
        }
        return new Vector2[]{ frontPoint, backPoint };
    }
    public static List<Vector2> GetClosestWallToPoint(List<Vector2> points, Vector2 point)
    {
        Vector2 closestPoint1 = points[points.Count-1];
        Vector2 closestPoint2 = points[0];
        float minDiff = Mathf.Abs(Vector2.Distance(closestPoint1, closestPoint2) - (Vector2.Distance(point, closestPoint1) + Vector2.Distance(point, closestPoint2)));
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];
            float dist1 = Vector2.Distance(p1, p2);
            float dist2 = Vector2.Distance(point, p1);
            float dist3 = Vector2.Distance(point, p2);
            float diff = Mathf.Abs(dist1 - (dist2+dist3));
            if (diff < minDiff)
            {
                minDiff = diff;
                closestPoint1 = p1;
                closestPoint2 = p2;
            }
        }
        return new List<Vector2>() { closestPoint1, closestPoint2 };
    }
    public static Line2D GetHitWallDirection(RaycastHit2D hit, Collider2D staticCollider2D, Vector2 hitDirection)
    /** Returns a line representing the wall hit by the raycast, with the direction used to generate points in a circle.
     * The line represents the edge of the wall that was hit, and is used to determine how to move along the wall to avoid a collision.
     * If the collider is a box or polygon, the line will be aligned with the edge that was hit. If it's a circle, the line will be perpendicular to the radius at the contact point.
     */
    {
        // Get the two points of the wall that are closest to the hitPoint
        List<Vector2> wallPoints = GeometryUtils.GetColliderPoints(staticCollider2D, hitDirection);
        List<Vector2> closestWallPoints = GetClosestWallToPoint(wallPoints, hit.point);
        
        Vector2 closestPoint1 = closestWallPoints[0];
        Vector2 closestPoint2 = closestWallPoints[1];

        Vector2 wall1 = closestPoint1 - closestPoint2;
        Vector2 wall2 = closestPoint2 - closestPoint1;
        if (Vector2.Angle(hitDirection, wall1) < 90)  // Like: /\
        {
            return new Line2D(closestPoint2, wall1);
        }
        else
        {
            return new Line2D(closestPoint1, wall2);
        }
    }
    private static List<Vector2> GetColliderPoints(Collider2D collider2D, Vector2 direction)
    {
        List<Vector2> points = new List<Vector2>();

        if (collider2D is CircleCollider2D circle)
        {
            Vector2 center = collider2D.transform.TransformPoint(circle.offset);
            float radius = circle.radius * Mathf.Max(collider2D.transform.lossyScale.x, collider2D.transform.lossyScale.y);
            points.Add(center + direction * radius);
            points.Add(center - direction * radius);
            Vector2 perpendicularDir = Vector2.Perpendicular(direction);
            points.Add(center + perpendicularDir * radius);
            points.Add(center - perpendicularDir * radius);
        }
        else if (collider2D is BoxCollider2D box)
        {
            Vector2 size = box.size;
            Vector2 scale = collider2D.transform.lossyScale;
            size = new Vector2(size.x * scale.x, size.y * scale.y);
            Vector2 halfSize = size / 2;
            Vector2 center = collider2D.transform.TransformPoint(box.offset);
            Vector2 right = collider2D.transform.right;
            Vector2 up = collider2D.transform.up;
            points.Add(center + halfSize.x * right + halfSize.y * up);
            points.Add(center + halfSize.x * right - halfSize.y * up);
            points.Add(center - halfSize.x * right - halfSize.y * up);
            points.Add(center - halfSize.x * right + halfSize.y * up);

        }
        else if (collider2D is PolygonCollider2D poly)
        {
            foreach (Vector2 point in poly.points)
            {
                points.Add(collider2D.transform.TransformPoint(point + poly.offset));
            }
        }
        else if (collider2D is CompositeCollider2D composite)
        {
            PhysicsShapeGroup2D shapeGroup = new PhysicsShapeGroup2D();
            int count = composite.GetShapes(shapeGroup);
            List<Vector2> vertices = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                shapeGroup.GetShapeVertices(i, vertices);
                for (int j = 0; j < vertices.Count; j++)
                {
                    points.Add(composite.transform.TransformPoint(vertices[j]));
                }
            }
        }
        else
        {
            // For other colliders, use bounds corners as approximation
            Bounds bounds = collider2D.bounds;
            points.Add(new Vector2(bounds.min.x, bounds.min.y));
            points.Add(new Vector2(bounds.min.x, bounds.max.y));
            points.Add(new Vector2(bounds.max.x, bounds.min.y));
            points.Add(new Vector2(bounds.max.x, bounds.max.y));
        }
        return points;
    }
    public static bool MovingTowardsThreat(
        Vector2 movePos, Vector2 moveDir,
        Vector2 threatPos, Vector2 threatDir, Vector2 movementCenter, Vector2 crossPoint)
    {
        if (threatDir.magnitude == 0) return true; // Static threat, always moving towards it
        Vector2 dV = crossPoint - movePos;
        Vector2 V = threatPos - movePos;

        // Compute the normal to the threat line (in 2D, rotate threatDir by 90 degrees)
        Vector2 threatNormal = new Vector2(-threatDir.y, threatDir.x); // Perpendicular to threatDir
        Vector2 signedDistanceStart = movementCenter + dV;
        Vector2 projectedLine = Project(threatPos - movementCenter, threatNormal);

        // Compute how the movement direction affects the distance
        // Project moveDir onto the threat line's normal
        float sameDirection = Vector2.Dot(moveDir, projectedLine);

        // Moving in the same direction as threat line normal means moving towards threat
        return sameDirection > 0;
    }
    public static Vector2 Project(Vector2 vector, Vector2 onto)
    {
        return Vector2.Dot(vector, onto) / Vector2.Dot(onto, onto) * onto;
    }
    public static Vector2 WorldCoordsToLocal(Vector2 worldCoords, UnityEngine.Transform coordsTransform)
    {
        Vector3 worldDir3 = new Vector3(worldCoords.x, worldCoords.y, 0f);
        Vector3 localDir3 = coordsTransform.InverseTransformDirection(worldDir3);
        return new Vector2(localDir3.x, localDir3.y);
    }
    public static Vector2 LocalCoordsToWorld(Vector2 localCoords, UnityEngine.Transform coordsTransform)
    {
        Vector3 localDir3 = new Vector3(localCoords.x, localCoords.y, 0f);
        Vector3 worldDir3 = coordsTransform.TransformDirection(localDir3);
        return new Vector2(worldDir3.x, worldDir3.y);
    }
}
