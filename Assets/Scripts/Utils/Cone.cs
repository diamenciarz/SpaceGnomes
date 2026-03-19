using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EntityTeam;
using static GeometryUtils;

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

    public List<GameObject> GetVisibleAlliesInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    public List<GameObject> GetVisibleEnemiesInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    public List<GameObject> GetVisibleAlliesInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    public List<GameObject> GetVisibleEnemiesInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .Select(hit => hit.collider.gameObject)
                   .Distinct()
                   .ToList();
    }

    public List<GameObject> GetVisibleObjectsInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Select(hit => hit.collider.gameObject).Distinct().ToList();
    }

    public List<GameObject> GetVisibleObjectsInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Select(hit => hit.collider.gameObject).Distinct().ToList();
    }

    public GameObject GetClosestVisibleAllyInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault().collider.gameObject;
    }

    public GameObject GetClosestVisibleAllyInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        return hits.Where(hit => TeamManager.Instance.IsAlly(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault().collider.gameObject;
    }

    public GameObject GetClosestVisibleEnemyInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    public GameObject GetClosestVisibleEnemyInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);

        RaycastHit2D bestHit = hits.Where(hit => TeamManager.Instance.IsEnemy(TeamManager.Instance.GetEntityTeam(hit.collider.gameObject), myTeam))
                   .OrderBy(hit => hit.distance)
                   .FirstOrDefault();
        if (!bestHit) return null;
        return bestHit.collider.gameObject;
    }

    public GameObject GetClosestVisibleObjectInCone(Team myTeam, HasEntityType.EntityType[] entityTypes, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = this.GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleEntity(end, origin, team, entityTypes, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.OrderBy(hit => hit.distance).FirstOrDefault();
        return bestHit.collider.gameObject;
    }

    public GameObject GetClosestVisibleObjectInCone(Team myTeam, SensorType sensorType = SensorType.Camera)
    {
        List<RaycastHit2D> hits = GetAllHitsInCone((end, origin, team, dist, sensor) => GetFirstVisibleObject(end, origin, team, dist, sensor), myTeam, this.maxDistance, sensorType);
        RaycastHit2D bestHit = hits.OrderBy(hit => hit.distance).FirstOrDefault();
        return bestHit.collider.gameObject;
    }

    public List<GameObject> KeepObjectsInCone(List<GameObject> gameObjects)
    {
        return gameObjects.Where(obj => IsObjectInCone(obj)).ToList();
    }

    public bool IsObjectInCone(GameObject target, bool drawDebugLines = false)
    {
        Collider2D collider = target.GetComponent<Collider2D>();
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
            targetPosition = collider.ClosestPoint(this.origin + this.direction.normalized * this.maxDistance);
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

    public bool IsPositionInCone(Vector2 targetPosition)
    {
        Vector2 toTarget = targetPosition - this.origin;
        bool positionOutsideConeRadius = toTarget.magnitude > this.maxDistance;
        if (positionOutsideConeRadius) return false;

        float angleToTarget = Vector2.Angle(this.direction, toTarget);
        return angleToTarget < this.angle / 2;
    }
}
