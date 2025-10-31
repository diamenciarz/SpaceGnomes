using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HasEntityType;

[RequireComponent(typeof(EntityTeam))]
public class AIControlInput : ShipControlInput
{
    [Header("Distance settings")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float stopRange = 5;
    [SerializeField] private float avoidRange = 10f;

    [Header("Instances")]
    private EntityTeam myTeam; // Get the team

    [SerializeField] private List<EntityType> chaseEntityTypes;
    [SerializeField] private List<EntityType> avoidEntityTypes;

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
        GameObject chaseEntity = GetEntityToChase();
        List<GameObject> avoidEntities = GetEntitiesToAvoid();

        if (!chaseEntity)
        {
            controlVector = Vector2.zero;
            return;
        }

        controlVector = CalculateControlVector(chaseEntity, avoidEntities);
        //Vector2 a = CalculateControlVector(chaseEntity, avoidEntities);
    }
    private Vector2 CalculateControlVector(GameObject chaseEntity, List<GameObject> avoidEntities)
    {
        Vector2 directionToTarget = (chaseEntity.transform.position - gameObject.transform.position);
        if(directionToTarget.magnitude > 1)
        {
            directionToTarget.Normalize();
        }
        Debug.DrawRay(transform.position, directionToTarget, Color.yellow);
        return WorldCoordsToLocal(directionToTarget);
    }
    private Vector2 WorldCoordsToLocal(Vector2 worldCoords)
    {
        Vector3 worldDir3 = new Vector3(worldCoords.x, worldCoords.y, 0f);
        Vector3 localDir3 = transform.InverseTransformDirection(worldDir3);
        return new Vector2(localDir3.x, localDir3.y);
    }
    private List<GameObject> GetEntitiesToAvoid()
    {
        List<EntityTeam.Team> teams = new List<EntityTeam.Team>() {
            EntityTeam.Team.Neutral,
            myTeam.team
        };
        List<GameObject> avoidEntities = TeamManager.Instance.GetNearbyEntitiesInTeams(avoidEntityTypes, gameObject.transform.position, avoidRange, teams);
        return avoidEntities;
    }
    private GameObject GetEntityToChase()
    {
        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(chaseEntityTypes, gameObject.transform.position, chaseRange, myTeam.team);
        GameObject chaseEntity = GeometryUtils.FindClosestEntity(entities, gameObject.transform.position, stopRange);
        return chaseEntity;
    }
}
