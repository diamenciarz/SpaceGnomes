using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static GeometryUtils;
using static HasEntityType;
using static Trajectory;

[RequireComponent(typeof(EntityTeam))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Trajectory))]
[RequireComponent(typeof(ShipController))]
public class AIChaseInput : ShipControlInput
{
    public enum ChaseMode
    {
        DirectlyToTarget,
        ExtrapolateTrajectory
    }
    [Header("Distance settings")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float stopRange = 5;
    [SerializeField] [Range(0,1)] private float maxAvoidanceFraction = 0.7f;
    [SerializeField] [Range(0, 50f)] [Tooltip("Obstacles moving relatively towards us faster than this will be avoided")]
    private float dangerousObstacleSpeed = 5f;
    [SerializeField] private ChaseMode chaseMode = ChaseMode.DirectlyToTarget;
    [SerializeField] [Range(1f,5f)] private float avoidCollisionLookaheadTime = 2f;

    [Header("Instances")]
    [SerializeField] private List<EntityType> chaseEntityTypes;
    [SerializeField] private List<EntityType> avoidEntityTypes;

    [Header("Debugging Settings")]
    [SerializeField] private bool debug = false;

    private Vector2 controlVector = Vector2.zero;
    private Rigidbody2D myRigidbody2D;
    private ShipController shipController;
    private EntityTeam myTeam; // Get the team
    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        shipController = GetComponent<ShipController>();
    }

    public override float GetSteerInput()
    {
        return controlVector.x;
    }

    public override float GetThrustInput()
    {
        return controlVector.y;
    }

    void Update()
    {
        Vector2 chaseVector = CalculateChaseVector();
        Vector2 avoidanceVector = CalculateAvoidanceVector();
        if (debug) Debug.DrawRay(transform.position, avoidanceVector, Color.blue);

        Vector2 scaledAvoidance = avoidanceVector.normalized * avoidanceVector.magnitude * maxAvoidanceFraction; // 0 -> 0.7
        Vector2 scaledChase = chaseVector * (1 - scaledAvoidance.magnitude); // 1 -> 0.3
        Vector2 output = scaledAvoidance + scaledChase;
        if (output.magnitude > 1) output.Normalize();
        if (debug) Debug.DrawRay(transform.position, output, Color.yellow);
        controlVector = WorldCoordsToLocal(output);
    }
    private Vector2 CalculateChaseVector()
    {
        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(gameObject.transform.position, myTeam.team, chaseEntityTypes, chaseRange);
        GameObject chaseEntity = GeometryUtils.FindClosestEntityToPosition(entities, gameObject.transform.position, stopRange);
        if (!chaseEntity) return Vector2.zero; // Noone to chase

        Vector2 directionToTarget = CalculateDirectionToTarget(chaseEntity);
        //if (debug) Debug.DrawRay(transform.position, directionToTarget, Color.red);
        if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
        return directionToTarget;
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
            return - GeometryUtils.CalculateVectorBetweenColliderEdges(chaseEntity, gameObject);
        }
    }

    private Vector2 CalculateAvoidanceVector()
    {
        List<GameObject> avoidEntities = GetEntitiesToAvoid();
        Vector2 avoidanceVector = Vector2.zero;
        foreach (GameObject obstacle in avoidEntities)
        {
            CollidingPoints collidingPoints = GeometryUtils.CalculateColliderEdgePoints(obstacle, gameObject);
            Trajectory.TrajectoryInstance obstacleTrajectory = obstacle.GetComponent<Trajectory>().GetShifted(collidingPoints.toPos - (Vector2)obstacle.transform.position);
            Trajectory.TrajectoryInstance myTrajectory = GetComponent<Trajectory>().GetShifted(collidingPoints.fromPos - (Vector2) transform.position);
            Trajectory.CrossingInfo? crossingInfo = myTrajectory.TrajectoryCrossing(obstacleTrajectory, avoidCollisionLookaheadTime);
            if (crossingInfo == null) continue; // No collision predicted
            
            float timeToCollision = crossingInfo.Value.time;
            Vector2 relativeVelocity = obstacleTrajectory.ExtrapolateFutureVelocity(timeToCollision) - myTrajectory.ExtrapolateFutureVelocity(timeToCollision);

            if (relativeVelocity.magnitude < dangerousObstacleSpeed) 
            {
                Debug.DrawRay(collidingPoints.fromPos, relativeVelocity, Color.gray);
                continue; // Not moving fast enough relative to us
            }
            Debug.DrawRay(collidingPoints.fromPos, relativeVelocity, Color.red);
            avoidanceVector += (collidingPoints.fromPos - crossingInfo.Value.position).normalized / timeToCollision; // The closer the collision, the stronger the avoidance
        }
        if (avoidanceVector.magnitude > 1) avoidanceVector.Normalize();

        return avoidanceVector; // We invert the vector to move away from obstacles
    }

    private List<GameObject> GetEntitiesToAvoid()
    {
        List<EntityTeam.Team> teams = new List<EntityTeam.Team>() {
            EntityTeam.Team.Neutral,
            EntityTeam.Team.EnemyToAll,
            myTeam.team
        };
        // The avoid range is counted from the middle of the entity, so we need to check in a larger range than avoidRange
        List<GameObject> avoidEntities = TeamManager.Instance.GetNearbyEntitiesInTeams(avoidEntityTypes, gameObject.transform.position, chaseRange, teams);
        avoidEntities = GeometryUtils.KeepVisibleObjects(gameObject.transform.position, avoidEntities, myTeam.team, chaseRange);
        return avoidEntities;
    }

    private Vector2 WorldCoordsToLocal(Vector2 worldCoords)
    {
        Vector3 worldDir3 = new Vector3(worldCoords.x, worldCoords.y, 0f);
        Vector3 localDir3 = transform.InverseTransformDirection(worldDir3);
        return new Vector2(localDir3.x, localDir3.y);
    }
}
