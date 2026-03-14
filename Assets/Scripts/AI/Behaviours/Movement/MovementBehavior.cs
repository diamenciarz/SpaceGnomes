using System;
using System.Collections.Generic;
using UnityEngine;
using static HasEntityType;

public abstract class MovementBehavior : ScriptableObject
{
    public class MovementBehaviorData
    {
        public Transform transform;
        public GameObject gameObject;
        public EntityTeam myTeam;
        public Rigidbody2D myRigidbody2D;
        public Trajectory myTrajectory;
    }

    public enum ChaseMode
    {
        DirectlyToTarget,
        ExtrapolateTrajectory
    }
    [Header("Distance settings")]
    [SerializeField][Range(1f, 100f)][Tooltip("Only chase entities within this distance")] protected float chaseRange = 20f;
    [SerializeField][Range(0f, 100f)][Tooltip("Will stop chasing an entity once below that distance")] protected float stopChaseRange = 6;
    [SerializeField][Range(0f, 100f)][Tooltip("The minimum distance to keep from avoided entities")] protected float retreatRange = 3;
    [SerializeField][Tooltip("Whether to only retreat from entities that are also enemies, or to retreat from any entity regardless of team")]
    protected bool onlyRetreatFromEnemies = true;
    [SerializeField][Range(0, 1)] protected float maxAvoidanceFraction = 0.7f;
    [SerializeField][Range(0, 1)] protected float maxRetreatFraction = 0.7f;
    [SerializeField]
    [Range(0, 50f)]
    [Tooltip("Obstacles moving relatively towards us faster than this will be avoided")]
    protected float dangerousObstacleSpeed = 0f;
    [SerializeField] protected ChaseMode chaseMode = ChaseMode.DirectlyToTarget;
    [SerializeField][Range(1f, 5f)] protected float avoidCollisionLookaheadTime = 2f;
    [SerializeField][Range(1, 30)]
    [Tooltip("How many points along the trajectory to check for collisions. Higher values are more accurate but more expensive.")]
    protected int collisionCheckPointCount = 5;

    [Header("Instances")]
    [SerializeField] protected List<EntityType> chaseEntityTypes;
    [SerializeField] protected List<EntityType> avoidEntityTypes;
    [SerializeField] protected List<EntityType> retreatFromEntityTypes;

    [Header("Debugging Settings")]
    [SerializeField] protected bool debugCollisions = false;
    [SerializeField] protected bool debugMovementVectors = false;
    [SerializeField] protected bool debugConsideredDistances = false;

    private void OnValidate()
    {
        if (retreatRange > stopChaseRange) retreatRange = stopChaseRange;
    }

    /**
    <summary>
    Calculate the control vector in world coordinates.
    </summary>
     **/
    public virtual Vector2 CalculateControlVector(MovementBehaviorData data)
    {
        Vector2 chaseVector = CalculateChaseVector(data);
        Vector2 retreatVector = CalculateRetreatVector(data);
        Vector2 avoidanceVector = CalculateAvoidanceVector(data);
        // All are normalized if longer than 1
        if (debugMovementVectors) Debug.DrawRay(data.transform.position, avoidanceVector, Color.blue);

        Vector2 scaledAvoidance = avoidanceVector * maxAvoidanceFraction; // 0 -> 0.7
        Vector2 scaledRetreat = retreatVector * (1 - scaledAvoidance.magnitude) * maxRetreatFraction; // 0 -> ((1 -> 0.3) * 0.7) => 0 -> (0.7 -> 0.21)
        Vector2 scaledChase = chaseVector * (1 - scaledRetreat.magnitude - scaledAvoidance.magnitude); // 1 -> 0.09
        Vector2 output = scaledAvoidance + scaledRetreat + scaledChase;
        if (output.magnitude > 1) output.Normalize();
        if (debugCollisions) Debug.DrawRay(data.transform.position, output, Color.yellow);
        return output;
    }
    protected virtual Vector2 CalculateChaseVector(MovementBehaviorData data)
    {
        GameObject chaseEntity = GetClosestChaseEntity(data);
        if (!chaseEntity) return Vector2.zero;
        Vector2 directionToTarget = CalculateDirectionToTarget(data, chaseEntity);

        if(debugMovementVectors) Debug.DrawRay(data.transform.position, directionToTarget, Color.white);

        if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
        return directionToTarget;

    }
    protected GameObject GetClosestChaseEntity(MovementBehaviorData data)
    {
        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(data.transform.position, data.myTeam.team, chaseEntityTypes, chaseRange);
        // If chased entity is farther than stopChaseRange, we want to chase it, otherwise we want to stop chasing and potentially start retreating
        return GeometryUtils.FindClosestEntityToPosition(entities, data.transform.position, stopChaseRange);
    }
    protected virtual Vector2 CalculateRetreatVector(MovementBehaviorData data)
    {
        List<GameObject> entities = GetRetreatEntities(data);

        GameObject entityToRetreatFrom = GeometryUtils.FindClosestEntityToPosition(entities, data.transform.position, 0, retreatRange);
        if (!entityToRetreatFrom) return Vector2.zero;
        Vector2 directionFromEntity = -CalculateDirectionToTarget(data, entityToRetreatFrom);
        if (directionFromEntity.magnitude > 1) directionFromEntity.Normalize();
        return directionFromEntity;
    }
    private List<GameObject> GetRetreatEntities(MovementBehaviorData data)
    {
        if (onlyRetreatFromEnemies)
        {
            return TeamManager.Instance.GetNearbyEnemies(data.transform.position, data.myTeam.team, retreatFromEntityTypes, retreatRange);
        }
        else
        {
            return EntityCounter.Instance.GetNearbyEntities(data.transform.position, retreatFromEntityTypes, retreatRange);
        }
    }
    protected virtual Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        if (chaseMode == ChaseMode.ExtrapolateTrajectory && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
        {
            Vector2 myVelocity = data.myRigidbody2D.velocity;
            float mySpeed = myVelocity.magnitude;
            const float MINIMUM_SPEED = 2f;
            float simulatedVelocity = Mathf.Max(myVelocity.magnitude, MINIMUM_SPEED);
            Vector2 relativeVelocity = targetTrajectory.GetVelocity() - myVelocity;
            float reachTime = (targetTrajectory.GetCurrentPosition() - (Vector2)data.transform.position).magnitude / simulatedVelocity;
            Vector2 predictedHitCoords = targetTrajectory.ExtrapolateFuturePosition(reachTime);
            if (debugMovementVectors)
            {
                Debug.DrawLine(chaseEntity.transform.position, predictedHitCoords, Color.green);
            }
            return predictedHitCoords - (Vector2)data.transform.position;
            
        }
        else
        {
            return GeometryUtils.CalculateVectorBetweenColliderEdges(chaseEntity, data.gameObject);
        }
    }

    protected virtual Vector2 CalculateAvoidanceVector(MovementBehaviorData data)
    {
        List<GameObject> avoidEntities = GetEntitiesToAvoid(data);
        Vector2 avoidanceVector = Vector2.zero;
        foreach (GameObject obstacle in avoidEntities)
        {
            if (debugConsideredDistances)
            {
                GeometryUtils.CollidingPoints collidingPoints = GeometryUtils.CalculateClosestDistanceBetweenColliders(obstacle, data.gameObject);
                Debug.DrawLine(collidingPoints.toPos, collidingPoints.fromPos, Color.cyan);
            }
         
            Trajectory obstacleTrajectory = obstacle.GetComponent<Trajectory>();
            Trajectory.CollisionInfo? crossingInfo = data.myTrajectory.WillObjectsCollide(obstacleTrajectory, avoidCollisionLookaheadTime, collisionCheckPointCount);
            if (!crossingInfo.HasValue) continue; // No collision predicted

            float timeToCollision = crossingInfo.Value.time;
            if (timeToCollision <= 0f) continue; // Already colliding, can't avoid

            // The closer the collision, the stronger the avoidance
            Vector2 threatVector = CalculateThreatVector(crossingInfo.Value);

            Vector2 relativeVelocity = obstacleTrajectory.ExtrapolateFutureVelocity(timeToCollision) - data.myTrajectory.ExtrapolateFutureVelocity(timeToCollision);
            float weight = 1 - (timeToCollision / avoidCollisionLookaheadTime); // Closer collisions have higher weight
            Vector2 scaledThreatVector = threatVector.normalized * weight;
            if (relativeVelocity.magnitude < dangerousObstacleSpeed)
            {
                if (debugCollisions) Debug.DrawRay(crossingInfo.Value.collisionPosition, scaledThreatVector, Color.gray);
                continue; // Not moving fast enough relative to us
            }
            if (debugCollisions) Debug.DrawRay(crossingInfo.Value.collisionPosition, scaledThreatVector, Color.red);
            avoidanceVector += scaledThreatVector;
        }
        if (avoidanceVector.magnitude > 1) avoidanceVector.Normalize();

        return avoidanceVector; // We invert the vector to move away from obstacles
    }

    private Vector2 CalculateThreatVector(Trajectory.CollisionInfo crossingInfo)
    {
        Line2D mVelocity = new Line2D(crossingInfo.myPoint, crossingInfo.myVelocity);
        Line2D otherVelocity = new Line2D(crossingInfo.otherPoint, crossingInfo.otherVelocity);

        // Maybe possible to simplify the function by using Vector2 in the calculation
        Line2D threatVector = mVelocity.CalculateThreatLine(otherVelocity);
        float distanceToThreat = threatVector.direction.magnitude;
        return threatVector.direction;
    }

    private List<GameObject> GetEntitiesToAvoid(MovementBehaviorData data)
    {
        List<EntityTeam.Team> teams = new List<EntityTeam.Team>() {
            EntityTeam.Team.Neutral,
            EntityTeam.Team.EnemyToAll,
            data.myTeam.team
        };
        // The avoid range is counted from the middle of the entity, so we need to check in a larger range than avoidRange
        const float MAX_ENTITY_SIZE = 5f;
        List<GameObject> entities = GeometryUtils.GetVisibleObjects(data.gameObject.transform.position, data.myTeam.team, avoidEntityTypes, chaseRange + MAX_ENTITY_SIZE, ignoreObject: data.gameObject);
        // Remove my own ship parts
        return EntityCounter.Instance.ExcludeMyChildren(data.gameObject, entities);
    }

}
