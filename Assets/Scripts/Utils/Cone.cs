using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EntityTeam;
using static GeometryUtils;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Cone
{
    public Vector2 origin;
    public Vector2 direction;
    public float zMiddleAngle;
    public float angle; // in degrees
    public float maxDistance;

    /// <summary>
    /// Initializes a new instance of the Cone class with the specified origin, direction, angle, and maximum distance.
    /// </summary>
    /// <param name="origin">The origin point of the cone.</param>
    /// <param name="direction">The direction vector of the cone exactly halfway through the angle.</param>
    /// <param name="coneAngle">The angle of the cone in degrees.</param>
    /// <param name="maxDistance">The maximum distance from the origin.</param>
    public Cone(Vector2 origin, Vector2 direction, float coneAngle, float maxDistance)
    {
        this.origin = origin;
        this.zMiddleAngle = GeometryUtils.DirectionVectorToAngle(direction);
        this.direction = direction.normalized;
        this.angle = coneAngle;
        this.maxDistance = maxDistance;
    }
    public Cone(Vector2 origin, float zMiddleAngle, float coneAngle, float maxDistance)
    {
        this.origin = origin;
        this.zMiddleAngle = zMiddleAngle;
        this.direction = GeometryUtils.AngleToDirectionVector(zMiddleAngle);
        this.angle = coneAngle;
        this.maxDistance = maxDistance;

    }
    public override string ToString()
    {
        return $"Cone(origin: {origin}, direction: {direction}, angle: {angle}, maxDistance: {maxDistance})";
    }

    public float CalculateRelativePositionAngle(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float myAngle = GeometryUtils.ClampAngle180(this.zMiddleAngle);
        return GeometryUtils.ClampAngle180(targetAngle - myAngle);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="relativePositionAngle"></param>
    /// <returns></returns>
    public float ClampAngleToCone(float relativePositionAngle)
    {
        float halfConeAngle = this.angle / 2;
        bool positionOutsideCone = Mathf.Abs(relativePositionAngle) > halfConeAngle;
        float clampedAngle = positionOutsideCone ? Mathf.Sign(relativePositionAngle) * halfConeAngle : relativePositionAngle;
        return clampedAngle;
    }

    static float RAYS_PER_DEGREE = 0.5f;
    private List<RaycastHit2D> GetAllHitsInCone(System.Func<Vector2, Vector2, Team, float, SensorType, RaycastHit2D?> raycastFunc, Team myTeam, float maxDistance, SensorType sensorType)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        int rayCount = Mathf.CeilToInt(this.angle * RAYS_PER_DEGREE);
        float angleStep = this.angle / rayCount;
        float startAngle = -this.angle / 2;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * this.direction;
            RaycastHit2D? hit = raycastFunc(this.origin + rayDirection * maxDistance, this.origin, myTeam, maxDistance, sensorType);
            if (hit.HasValue)
            {
                hits.Add(hit.Value);
            }
        }
        return hits;
    }

    // Get all visible
    /// <summary>
    /// Retrieves a list of visible enemy GameObjects within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of GameObjects that are enemies and visible within the cone.</returns>
    public List<GameObject> GetVisibleEnemiesInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    /// <summary>
    /// Retrieves a list of visible enemy GameObjects within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of GameObjects that are enemies and visible within the cone.</returns>
    public List<GameObject> GetVisibleEnemiesInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    /// <summary>
    /// Retrieves a list of visible ally GameObjects within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of GameObjects that are allies and visible within the cone.</returns>
    public List<GameObject> GetVisibleAlliesInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    /// <summary>
    /// Retrieves a list of visible ally GameObjects of specified entity types within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="entityTypes">An array of entity types to filter the allies by.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of GameObjects that are allies of the specified types and visible within the cone.</returns>
    public List<GameObject> GetVisibleAlliesInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    /// <summary>
    /// Retrieves a list of all visible GameObjects within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of all GameObjects that are visible within the cone.</returns>
    public List<GameObject> GetVisibleObjectsInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Select(hit => hit.collider.gameObject).Distinct().ToList();
    }

    /// <summary>
    /// Retrieves a list of visible GameObjects of specified entity types within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="entityTypes">An array of entity types to filter the objects by.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>A list of GameObjects of the specified types that are visible within the cone.</returns>
    public List<GameObject> GetVisibleObjectsInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Select(hit => hit.collider.gameObject).Distinct().ToList();
    }

    // Get closest
    /// <summary>
    /// Finds the closest GameObject from the provided list that is within the cone, regardless of visibility. Uses the closest point in the collider if available, otherwise uses the object's position.
    /// </summary>
    /// <param name="gameObjects"> The list of GameObjects to check against the cone.</param>
    /// <returns></returns>
    public GameObject GetClosestObjectInCone(List<GameObject> gameObjects, ConeDistance coneDistance = ConeDistance.ClosestDistance)
    {
        return (GameObject)GetClosest(gameObjects.Cast<object>(), obj => Distance((GameObject)obj, coneDistance), obj => IsObjectInCone((GameObject)obj));
    }

    /// <summary>
    /// Finds the closest position from the provided list that is within the cone. Uses the position directly since it's already a Vector2.
    /// </summary>
    /// <param name="positions">The list of positions to check against the cone.</param>
    /// <returns></returns>
    public Vector2? GetClosestPositionInCone(List<Vector2> positions, ConeDistance coneDistance = ConeDistance.ClosestDistance)
    {
        var result = GetClosest(positions.Cast<object>(), pos => Distance((Vector2)pos, coneDistance), pos => IsPositionInCone((Vector2)pos));
        return result != null ? (Vector2?)result : null;
    }
    /// <summary>
    /// Finds the closest GameObject from the provided list based on the specified distance metric (closest distance or smallest angle), regardless of visibility. Uses the closest point in the collider if available, otherwise uses the object's position.
    /// </summary>
    /// <param name="gameObjects"></param>
    /// <param name="coneDistance"></param>
    /// <returns></returns>
    public GameObject GetClosestObject(List<GameObject> gameObjects, ConeDistance coneDistance = ConeDistance.ClosestDistance)
    {
        return (GameObject)GetClosest(gameObjects.Cast<object>(), obj => Distance((GameObject)obj, coneDistance), null);
    }
    /// <summary>
    /// Finds the closest position from the provided list based on the specified distance metric (closest distance or smallest angle), regardless of visibility. Uses the position directly since it's already a Vector2.
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="coneDistance"></param>
    /// <returns></returns>
    public Vector2? GetClosestPosition(List<Vector2> positions, ConeDistance coneDistance = ConeDistance.ClosestDistance)
    {
        var result = GetClosest(positions.Cast<object>(), pos => Distance((Vector2)pos, coneDistance), null);
        return result != null ? (Vector2?)result : null;
    }

    private object GetClosest(IEnumerable<object> items, Func<object, float> distanceFunc, Func<object, bool> filterFunc)
    {
        object closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (var item in items)
        {
            if (filterFunc != null && !filterFunc(item)) continue;
            float distance = distanceFunc(item);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = item;
            }
        }
        return closest;
    }

    /// <summary>
    /// Finds the closest visible ally GameObject of specified entity types within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="entityTypes">An array of entity types to filter the allies by.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible ally GameObject of the specified types, or null if no allies are found.</returns>
    public GameObject GetClosestVisibleAllyInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    /// <summary>
    /// Finds the closest visible ally GameObject within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible ally GameObject, or null if no allies are found.</returns>
    public GameObject GetClosestVisibleAllyInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    /// <summary>
    /// Finds the closest visible enemy GameObject of specified entity types within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="entityTypes">An array of entity types to filter the enemies by.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible enemy GameObject of the specified types, or null if no enemies are found.</returns>
    public GameObject GetClosestVisibleEnemyInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    /// <summary>
    /// Finds the closest visible enemy GameObject within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible enemy GameObject, or null if no enemies are found.</returns>
    public GameObject GetClosestVisibleEnemyInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);

        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    /// <summary>
    /// Finds the closest visible GameObject of specified entity types within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="entityTypes">An array of entity types to filter the objects by.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible GameObject of the specified types, or null if no objects are found.</returns>
    public GameObject GetClosestVisibleObjectInCone(Team myTeam, EntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = this.GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.OrderBy(hit => hit.distance).FirstOrDefault();
        return bestHit.collider.gameObject;
    }

    /// <summary>
    /// Finds the closest visible GameObject within the cone for the specified team using the given sensor type.
    /// </summary>
    /// <param name="myTeam">The team of the entity performing the visibility check.</param>
    /// <param name="sensorType">The type of sensor used for detection. Defaults to Camera.</param>
    /// <returns>The closest visible GameObject, or null if no objects are found.</returns>
    public GameObject GetClosestVisibleObjectInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.OrderBy(hit => hit.distance).FirstOrDefault();
        return bestHit.collider.gameObject;
    }

    public enum ConeDistance
    {
        ClosestDistance,
        SmallestAngle
    }
    public float Distance(GameObject obj, ConeDistance coneDistance)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        Vector2 targetPosition = collider ? collider.ClosestPoint(this.origin) : (Vector2)obj.transform.position;
        return Distance(targetPosition, coneDistance);
    }
    public float Distance(Vector2 targetPosition, ConeDistance coneDistance)
    {
        if (coneDistance == ConeDistance.ClosestDistance)
        {
            return (targetPosition - this.origin).magnitude;
        }
        else // ConeDistance.SmallestAngle
        {
            Vector2 toTarget = targetPosition - this.origin;
            float angleToTarget = Vector2.Angle(this.direction, toTarget);
            return angleToTarget;
        }
    }

    /// <summary>
    /// Filters the provided list of GameObjects to include only those within the cone.
    /// </summary>
    /// <param name="gameObjects">The list of GameObjects to filter.</param>
    /// <returns>A new list containing only the GameObjects that are within the cone.</returns>
    public List<GameObject> KeepObjectsInCone(List<GameObject> gameObjects)
    {
        return gameObjects.Where(obj => IsObjectInCone(obj)).ToList();
    }

    /// <summary>
    /// Determines whether the specified GameObject is within the cone.
    /// </summary>
    /// <param name="target">The GameObject to check.</param>
    /// <param name="drawDebugLines">If true, draws debug lines for visualization. Defaults to false.</param>
    /// <returns>True if the GameObject is within the cone; otherwise, false.</returns>
    public bool IsObjectInCone(GameObject target, bool drawDebugLines = false)
    {
        Collider2D collider = target.GetComponent<Collider2D>();
        // Here we check if any point (closest to origin) is in the cone.
        Vector2 targetPosition = Vector2.zero;
        if (collider)
        {
            targetPosition = collider.ClosestPoint(this.origin);
            if(drawDebugLines) Debug.DrawLine(this.origin, targetPosition, Color.green);
        }
        else
        {
            targetPosition = target.transform.position;
        }
        Vector2 toTarget = targetPosition - this.origin;
        bool positionOutsideConeRadius = toTarget.magnitude > this.maxDistance;
        if (positionOutsideConeRadius) return false;
        
        // Here we find a point that is the closest to the middle of the cone's direction
        if (collider)
        {
            Vector2 coneMiddlePoint = this.origin + this.direction.normalized * toTarget.magnitude;
            targetPosition = collider.ClosestPoint(coneMiddlePoint);
            if (drawDebugLines) Debug.DrawLine(this.origin, targetPosition, Color.green);
        }
        else
        {
            targetPosition = target.transform.position;
        }

        toTarget = targetPosition - this.origin;
        float angleToTarget = Vector2.Angle(this.direction, toTarget);
        return angleToTarget < this.angle / 2;
    }

    /// <summary>
    /// Determines whether the specified position is within the cone.
    /// </summary>
    /// <param name="targetPosition">The position to check.</param>
    /// <returns>True if the position is within the cone; otherwise, false.</returns>
    public bool IsPositionInCone(Vector2 targetPosition)
    {
        Vector2 toTarget = targetPosition - this.origin;
        bool positionOutsideConeRadius = toTarget.magnitude > this.maxDistance;
        if (positionOutsideConeRadius) return false;

        float angleToTarget = Vector2.Angle(this.direction, toTarget);
        return angleToTarget < this.angle / 2;
    }
    public void DebugDisplayCone(Color color)
    {
        // DrawRays with RAYS_PER_DEGREE spacing
        int rayCount = Mathf.CeilToInt(this.angle * RAYS_PER_DEGREE);
        Debug.Log($"Drawing {rayCount} rays given angle {angle}");
        for (int i = 0; i < rayCount; i++)
        {
            float angle = this.zMiddleAngle - this.angle/2 + (i*(1/RAYS_PER_DEGREE));
            Vector2 direction = GeometryUtils.AngleToDirectionVector(angle);
            float radius = maxDistance != Mathf.Infinity ? maxDistance : 10;
            Debug.DrawRay(this.origin, direction * radius, color);
        }
    }
}
