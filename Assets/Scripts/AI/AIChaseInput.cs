using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static GeometryUtils;
using static HasEntityType;

[RequireComponent(typeof(EntityTeam))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Trajectory))]
//[RequireComponent(typeof(ShipController))]
public class AIChaseInput : ShipControlInput
{
    public enum ChaseMode
    {
        DirectlyToTarget,
        ExtrapolateTrajectory
    }
    [Header("Distance settings")]
    [SerializeField][Range(1f,100f)] private float chaseRange = 20f;
    [SerializeField][Range(0f, 100f)][Tooltip("Will stop chasing an entity once below that distance")] private float stopChaseRange = 6;
    [SerializeField][Range(0f, 100f)][Tooltip("The minimum distance to keep from avoided entities")] private float retreatRange = 3;
    [SerializeField] [Range(0,1)] private float maxAvoidanceFraction = 0.7f;
    [SerializeField] [Range(0, 50f)] [Tooltip("Obstacles moving relatively towards us faster than this will be avoided")]
    private float dangerousObstacleSpeed = 5f;
    [SerializeField] private ChaseMode chaseMode = ChaseMode.DirectlyToTarget;
    [SerializeField] [Range(1f,5f)] private float avoidCollisionLookaheadTime = 2f;
    [SerializeField][Range(1,30)][Tooltip("How many points along the trajectory to check for collisions. Higher values are more accurate but more expensive.")] 
    int collisionCheckPointCount = 5;

    [Header("Instances")]
    [SerializeField] private List<EntityType> chaseEntityTypes;
    [SerializeField] private List<EntityType> avoidEntityTypes;
    [SerializeField] private List<EntityType> retreatFromEntityTypes;

    [Header("Debugging Settings")]
    [SerializeField] private bool debug = false;

    public enum ControlVectorCoordinates
    {
        World,
        Local
    }

    private Vector2 controlVector = Vector2.zero;
    private bool calculatedControlThisFrame = false;
    private Rigidbody2D myRigidbody2D;
    private VehicularController shipController;
    private EntityTeam myTeam; // Get the team


    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        shipController = GetComponent<VehicularController>();
    }

    public override float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
    {
        if (!calculatedControlThisFrame) CalculateControlVector();
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).x : controlVector.x;
    }

    public override float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
    {
        if (!calculatedControlThisFrame) CalculateControlVector();
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).y : controlVector.y;
    }

    public override float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local)
    {
        return 0f;
    }

    void Update()
    {
        if (retreatRange > stopChaseRange) retreatRange = stopChaseRange;
        calculatedControlThisFrame = false;
    }
    /**
    <summary>
    Calculated the control vector in world coordinates.
    </summary>
     **/
    private void CalculateControlVector()
    {
        Vector2 chaseVector = CalculateChaseVector();
        //chaseVector = Vector2.zero; // Disable chasing for now to test avoidance
        Vector2 avoidanceVector = CalculateAvoidanceVector();
        if (debug) Debug.DrawRay(transform.position, avoidanceVector, Color.blue);

        if (avoidanceVector.magnitude > 1) avoidanceVector.Normalize();
        Vector2 scaledAvoidance = avoidanceVector * maxAvoidanceFraction; // 0 -> 0.7
        Vector2 scaledChase = chaseVector * (1 - scaledAvoidance.magnitude); // 1 -> 0.3
        Vector2 output = scaledAvoidance + scaledChase;
        if (output.magnitude > 1) output.Normalize();
        if (debug) Debug.DrawRay(transform.position, output, Color.yellow);
        controlVector = output;
        calculatedControlThisFrame = true;
    }
    private Vector2 CalculateChaseVector()
    {
        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(gameObject.transform.position, myTeam.team, chaseEntityTypes, chaseRange);
        GameObject chaseEntity = GeometryUtils.FindClosestEntityToPosition(entities, gameObject.transform.position, stopChaseRange);
        // If chased entity is 
        if (chaseEntity)
        {
            Vector2 directionToTarget = CalculateDirectionToTarget(chaseEntity);
            //if (debug) Debug.DrawRay(transform.position, directionToTarget, Color.red);
            if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
            return directionToTarget;
        }
        // Try to retreat
        GameObject retreatFromEntity = GeometryUtils.FindClosestEntityToPosition(entities, gameObject.transform.position, 0, retreatRange);
        if (!retreatFromEntity) return Vector2.zero;

        Vector2 directionToEntity = -CalculateDirectionToTarget(retreatFromEntity);
        if (directionToEntity.magnitude > 1) directionToEntity.Normalize();
        return directionToEntity.magnitude < retreatRange ? directionToEntity : Vector2.zero;

    }
    private Vector2 CalculateDirectionToTarget(GameObject chaseEntity)
    {
        if (chaseMode == ChaseMode.ExtrapolateTrajectory && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && myRigidbody2D)
        {
            float mySpeed = myRigidbody2D.velocity.magnitude;
            const float MINIMUM_SPEED = 2f;
            float simulatedVelocity = myRigidbody2D.velocity.magnitude > MINIMUM_SPEED ? myRigidbody2D.velocity.magnitude : MINIMUM_SPEED;
            //Vector2 predictedHitCoords = GeometryUtils.CalculateTrajectoryHitCoordinates(targetTrajectory, myRigidbody2D.position, shipController.GetMaxVelocity());
            Vector2 relativeVelocity = targetTrajectory.GetVelocity() - myRigidbody2D.velocity;
            float reachTime = (targetTrajectory.GetCurrentPosition() - (Vector2) transform.position).magnitude / simulatedVelocity;
            Vector2 predictedHitCoords = targetTrajectory.ExtrapolateFuturePosition(reachTime);
            //Debug.DrawLine(chaseEntity.transform.position, predictedHitCoords, Color.green);
            return predictedHitCoords - (Vector2) transform.position;
        }
        else
        {
            return GeometryUtils.CalculateVectorBetweenColliderEdges(chaseEntity, gameObject);
        }
    }

    private Vector2 CalculateAvoidanceVector()
    {
        List<GameObject> avoidEntities = GetEntitiesToAvoid();
        Vector2 avoidanceVector = Vector2.zero;
        foreach (GameObject obstacle in avoidEntities)
        {
            //GeometryUtils.CollidingPoints collidingPoints = GeometryUtils.CalculateClosestDistanceBetweenColliders(obstacle, gameObject);
            //Debug.DrawLine(collidingPoints.toPos, collidingPoints.fromPos, Color.cyan);
            Trajectory obstacleTrajectory = obstacle.GetComponent<Trajectory>();
            Trajectory myTrajectory = GetComponent<Trajectory>();
            Trajectory.CollisionInfo? crossingInfo = myTrajectory.WillObjectsCollide(obstacleTrajectory, avoidCollisionLookaheadTime, collisionCheckPointCount);
            if (!crossingInfo.HasValue) continue; // No collision predicted
            
            float timeToCollision = crossingInfo.Value.time;
            if (timeToCollision <= 0f) continue; // Already colliding, can't avoid

            // The closer the collision, the stronger the avoidance
            Vector2 threatVector = CalculateThreatVector(crossingInfo.Value);

            Vector2 relativeVelocity = obstacleTrajectory.ExtrapolateFutureVelocity(timeToCollision) - myTrajectory.ExtrapolateFutureVelocity(timeToCollision);
            if (relativeVelocity.magnitude < dangerousObstacleSpeed)
            {
                if (debug)  Debug.DrawRay(crossingInfo.Value.collisionPosition, threatVector, Color.gray);
                continue; // Not moving fast enough relative to us
            }
            avoidanceVector += threatVector;
            if (debug) Debug.DrawRay(crossingInfo.Value.collisionPosition, threatVector, Color.red);
        }
        if (avoidanceVector.magnitude > 1) avoidanceVector.Normalize();

        return avoidanceVector; // We invert the vector to move away from obstacles
    }

    private Vector2 CalculateThreatVector(Trajectory.CollisionInfo crossingInfo)
{
        Line2D myLine = new Line2D(crossingInfo.myPoint, crossingInfo.myVelocity);
        Line2D otherLine = new Line2D(crossingInfo.otherPoint, crossingInfo.otherVelocity);

        // Maybe possible to simplify the function by using Vector2 in the calculation
        Line2D threatVector = myLine.CalculateThreatVector(otherLine);
        if (float.IsNaN(threatVector.direction.x) || float.IsNaN(threatVector.direction.y))
        {
            Debug.Log("NaN threat vector. My velocity: " + crossingInfo.myVelocity + " Other velocity: " + crossingInfo.otherVelocity + " My point: " + crossingInfo.myPoint + " Other point: " + crossingInfo.otherPoint);
        }

        return threatVector.direction;
    }

    private List<GameObject> GetEntitiesToAvoid()
    {
        List<EntityTeam.Team> teams = new List<EntityTeam.Team>() {
            EntityTeam.Team.Neutral,
            EntityTeam.Team.EnemyToAll,
            myTeam.team
        };
        // The avoid range is counted from the middle of the entity, so we need to check in a larger range than avoidRange
        const float MAX_ENTITY_SIZE = 5f;
        List<GameObject> entities = GeometryUtils.GetVisibleObjects(gameObject.transform.position, myTeam.team, avoidEntityTypes, chaseRange + MAX_ENTITY_SIZE, ignoreObject:gameObject);
        // Remove my own ship parts
        return EntityCounter.Instance.ExcludeMyChildren(gameObject, entities);
    }
}
