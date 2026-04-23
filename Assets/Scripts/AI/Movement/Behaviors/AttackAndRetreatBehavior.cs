using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ControlInput;

[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Vehicular/AttackAndRetreatBehavior", fileName = "AttackAndRetreatBehavior")]
public class AttackAndRetreatBehavior : ChaseEntityOmnidirectionalBehavior
{
    [Header("Retreat Settings")]
    [SerializeField][Range(0.5f,20f)] private float retreatDelay = 1f;
    [SerializeField][Range(0.5f,20f)] private float retreatDuration= 4f;
    [SerializeField][Range(0.5f,20f)] private float minDistanceToStartRetreating= 2f;
    [SerializeField][Range(0.5f,20f)] private float retreatFromTargetDistance= 6f;

    private Dictionary<int, float> lastTimestamp = new Dictionary<int, float>(); // Stores the last timestamp for each entity to track retreat timing
    private Dictionary<int, float> targetRetreatDistance = new Dictionary<int, float>(); // Stores the current retreat distance for that entity which may be temporarily increased when the target is close to trigger retreat sooner.
    private Dictionary<int, GameObject> currentTarget = new Dictionary<int, GameObject>(); // Stores the current retreat distance for that entity which may be temporarily increased when the target is close to trigger retreat sooner.
    private Dictionary<int, bool> reachedTarget = new Dictionary<int, bool>(); // Stores the last timestamp for each entity to track retreat timing

    private void UpdateRetreatSetting(MovementBehaviorData data, Vector2 directionToTarget, Vector2 directionBetweenColliders)
    {
        if (directionToTarget.magnitude < 0.01f) return;

        if(retreatRange == 0)
        {
            Debug.LogWarning("Retreat range is 0, retreat behavior will not function. Please set a positive retreat range.");
            return; // No retreat if retreatRange is 0
        }

        int entityId = data.gameObject.GetInstanceID();
        if (!lastTimestamp.ContainsKey(entityId)) lastTimestamp[entityId] = Time.time;
        if (!targetRetreatDistance.ContainsKey(entityId)) targetRetreatDistance[entityId] = 0;
        if (!reachedTarget.ContainsKey(entityId)) reachedTarget[entityId] = false;

        //Debug.Log($"Entity {data.gameObject.name} direction to target magnitude: {directionToTarget.magnitude}, current retreat distance: {targetRetreatDistance[entityId]}, time since last timestamp: {Time.time - lastTimestamp[entityId]} seconds.");
        // Actual retreat logic: if currently retreating, check if retreat duration has passed to stop retreating.
        if (targetRetreatDistance[entityId] > 0)
        {
            if (Time.time - lastTimestamp[entityId] > retreatDuration)
            {
                //Debug.Log($"Entity {data.gameObject.name} stopped retreating after {retreatDuration} seconds.");
                lastTimestamp[entityId] = Time.time;
                targetRetreatDistance[entityId] = 0;
                reachedTarget[entityId] = false;
            }
        }
        else
        {
            if((directionBetweenColliders.magnitude > minDistanceToStartRetreating) && !reachedTarget[entityId]) lastTimestamp[entityId] = Time.time; // If target is not close enough, do not start retreating even if delay has passed.
            if((directionBetweenColliders.magnitude < minDistanceToStartRetreating) ) reachedTarget[entityId] = true; // If target is close enough, mark that we have reached the target so that retreating can be triggered after delay even if target moves away again.
            // If not currently retreating, check if retreat delay has passed to start retreating.
            if (Time.time - lastTimestamp[entityId] > retreatDelay)
            {
                //Debug.Log($"Entity {data.gameObject.name} started retreating for {retreatDuration} seconds after {retreatDelay} seconds.");
                targetRetreatDistance[entityId] = retreatRange;
                lastTimestamp[entityId] = Time.time;
            }
        }
    }
    // Just add the current target to the list of entities it retreats from ignoring the retreatList.
    public override ControlInputData CalculateControlVector(MovementBehaviorData data)
    {
        ChaseVector chaseVector = CalculateChaseVector(data);
        Vector2 retreatVector = CalculateRetreatVector(data);
        Vector2 avoidanceVector = CalculateCollisionAvoidanceVector(data);
        // All are normalized if longer than 1
        if (debugMovementVectors) Debug.DrawRay(data.transform.position, avoidanceVector, Color.blue);

        Vector2 scaledAvoidance = avoidanceVector * maxCollisionAvoidanceWeight; // 0 -> 0.7
        Vector2 scaledRetreat = retreatVector * (1 - scaledAvoidance.magnitude) * maxEntityRetreatWeight; // 0 -> ((1 -> 0.3) * 0.7) => 0 -> (0.7 -> 0.21)
        Vector2 scaledChase = chaseVector.vector * (1 - scaledRetreat.magnitude - scaledAvoidance.magnitude); // 1 -> 0.09
        Vector2 output = scaledAvoidance + scaledRetreat + scaledChase;
        if (output.magnitude > 1) output.Normalize();
        if (debugCollisions) Debug.DrawRay(data.transform.position, output, Color.yellow);

        return new ControlInputData(output, 0f, null);
    }
    protected override ChaseVector CalculateChaseVector(MovementBehaviorData data)
    {
        int entityId = data.gameObject.GetInstanceID();
        UpdateCurrentTarget(data, entityId);
        if (!currentTarget[entityId]) return new ChaseVector(Vector2.zero, null);

        Vector2 directionBetweenColliders = GeometryUtils.CalculateVectorBetweenColliderEdges(currentTarget[entityId], data.gameObject);
        Vector2 directionToTarget = CalculateDirectionToTarget(data, currentTarget[entityId], directionBetweenColliders);
        UpdateRetreatSetting(data, directionToTarget, directionBetweenColliders);

        if (targetRetreatDistance[entityId] != 0) return new ChaseVector(Vector2.zero, null);
        if (debugMovementVectors) Debug.DrawRay(data.transform.position, directionToTarget, directionBetweenColliders.magnitude <= minDistanceToStartRetreating ? Color.magenta : Color.white);

        if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
        return new ChaseVector(directionToTarget, null);
    }
    private Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity, Vector2 directionBetweenColliders)
    {
        if (chaseMode == ChaseMode.ExtrapolateAndCollideWithTarget && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
        {
            Vector2 predictedHitCoords = PredictHitCoords(targetTrajectory, data.myTrajectory, data.transform.position);
            return predictedHitCoords - (Vector2) data.transform.position;
        }
        else
        {
            return directionBetweenColliders;
        }
    }
    private void UpdateCurrentTarget(MovementBehaviorData data, int entityId)
    {
        if (!currentTarget.ContainsKey(entityId)) currentTarget[entityId] = null;

        List<GameObject> entities = TeamManager.Instance.GetNearbyEnemies(data.transform.position, data.myTeam.team, chaseEntityTypes, chaseRange);
        // If chased entity is farther than stopChaseRange, we want to chase it, otherwise we want to stop chasing and potentially start retreating
        GameObject newTarget = GeometryUtils.FindClosestEntityToPosition(entities, data.transform.position, stopChaseRange);
        if (newTarget) currentTarget[entityId] = newTarget;
    }
    protected override Vector2 CalculateRetreatVector(MovementBehaviorData data)
    {
        int entityId = data.gameObject.GetInstanceID();

        if (maxEntityRetreatWeight == 0) return Vector2.zero; // No avoidance if maxAvoidanceFraction is 0 or negative
        GameObject entityToRetreatFrom = GetEntityToRetreatFrom(data);

        Vector2 directionBetweenColliders;
        if (!entityToRetreatFrom)
        {
            if (targetRetreatDistance[entityId] ==0 || !currentTarget.ContainsKey(entityId) || !currentTarget[entityId])
            {
                return Vector2.zero;
            }
            directionBetweenColliders = GeometryUtils.CalculateVectorBetweenColliderEdges(currentTarget[entityId], data.gameObject);
            bool targetTooFarToRetreatFrom = directionBetweenColliders.magnitude > retreatFromTargetDistance;
            if (targetTooFarToRetreatFrom)
            {
                // If found no entities to retreat from and target is farther than threshold distance, no need to retreat
                return Vector2.zero;
            }
            entityToRetreatFrom = currentTarget[entityId];
        }
        else
        {
            directionBetweenColliders = GeometryUtils.CalculateVectorBetweenColliderEdges(entityToRetreatFrom, data.gameObject);

            // Find the closest entity to retreat from. If target is closer, retreat from target instead.
            if ((targetRetreatDistance[entityId] !=0) && currentTarget.ContainsKey(entityId) && currentTarget[entityId])
            {
                Vector2 directionToTarget = GeometryUtils.CalculateVectorBetweenColliderEdges(currentTarget[entityId], data.gameObject);

                if((directionToTarget.magnitude < retreatFromTargetDistance) && (directionToTarget.sqrMagnitude < directionBetweenColliders.sqrMagnitude))
                {
                    directionBetweenColliders = directionToTarget;
                    entityToRetreatFrom = currentTarget[entityId];
                }
            }
        }

        Vector2 directionFromEntity = -CalculateDirectionToTarget(data, entityToRetreatFrom, directionBetweenColliders);
        if (directionFromEntity.sqrMagnitude > 1) directionFromEntity.Normalize();
        return directionFromEntity;
    }
}
