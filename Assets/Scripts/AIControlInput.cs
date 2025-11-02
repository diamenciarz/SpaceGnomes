using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static HasEntityType;

[RequireComponent(typeof(EntityTeam))]
public class AIControlInput : ShipControlInput
{
    [Header("Distance settings")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float stopRange = 5;
    [SerializeField] private float avoidRange = 10f;
    [SerializeField] [Range(0,1)] private float maxAvoidanceFraction = 0.7f;

    [Header("Instances")]
    private EntityTeam myTeam; // Get the team

    [SerializeField] private List<EntityType> chaseEntityTypes;
    [SerializeField] private List<EntityType> avoidEntityTypes;

    [Header("Debugging Settings")]
    [SerializeField] private bool debug = false;

    private Vector2 controlVector = Vector2.zero;
    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
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

        Vector2 scaledAvoidance = avoidanceVector.normalized * avoidanceVector.magnitude * maxAvoidanceFraction; // 0 -> 0.7
        Vector2 scaledChase = chaseVector * (1 - scaledAvoidance.magnitude); // 1 -> 0.3
        Vector2 output = scaledAvoidance + scaledChase;
        if (output.magnitude > 1) output.Normalize();
        if (debug) Debug.DrawRay(transform.position, output, Color.yellow);
        controlVector = WorldCoordsToLocal(output);
    }
    private Vector2 CalculateChaseVector()
    {
        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(chaseEntityTypes, gameObject.transform.position, chaseRange, myTeam.team);
        GameObject chaseEntity = GeometryUtils.FindClosestEntity(entities, gameObject.transform.position, stopRange);
        if (!chaseEntity) return Vector2.zero;

        Vector2 directionToTarget = GeometryUtils.CalculateVectorBetweenColliderEdges(chaseEntity, gameObject);
        if (debug) Debug.DrawRay(transform.position, directionToTarget, Color.red);
        if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
        return directionToTarget;
    }
    private Vector2 CalculateAvoidanceVector()
    {
        List<GameObject> avoidEntities = GetEntitiesToAvoid();
        Vector2 avoidanceVector = Vector2.zero;
        foreach (GameObject obstacle in avoidEntities)
        {
            Vector2 dirToObstacle = GeometryUtils.CalculateVectorBetweenColliderEdges(obstacle, gameObject);
            if (dirToObstacle.magnitude < avoidRange)
            {
                // The closer the obstacle, the stronger the avoidance
                float scale = ((avoidRange - dirToObstacle.magnitude) / avoidRange);
                avoidanceVector += scale * dirToObstacle.normalized;
            }
        }
        if (avoidanceVector.magnitude > 1) avoidanceVector.Normalize();

        return avoidanceVector; // We invert the vector to move away from obstacles
    }
    private List<GameObject> GetEntitiesToAvoid()
    {
        List<EntityTeam.Team> teams = new List<EntityTeam.Team>() {
            EntityTeam.Team.Neutral,
            myTeam.team
        };
        // The avoid range is counted from the middle of the entity, so we need to check in a larger range than avoidRange
        List<GameObject> avoidEntities = TeamManager.Instance.GetNearbyEntitiesInTeams(avoidEntityTypes, gameObject.transform.position, chaseRange, teams);
        return avoidEntities;
    }
    private Vector2 WorldCoordsToLocal(Vector2 worldCoords)
    {
        Vector3 worldDir3 = new Vector3(worldCoords.x, worldCoords.y, 0f);
        Vector3 localDir3 = transform.InverseTransformDirection(worldDir3);
        return new Vector2(localDir3.x, localDir3.y);
    }
}
