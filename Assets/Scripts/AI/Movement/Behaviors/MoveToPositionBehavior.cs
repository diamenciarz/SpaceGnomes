using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Omnidirectional/MoveToPositionBehavior", fileName = "MoveToPositionBehavior")]
public class MoveToPositionBehavior : MovementBehavior
{
    [SerializeField][Range(1f, 20f)][Tooltip("Will start slowing down if going too fast within this distance of target (tail) position. [Must be greater than stopChaseRange + 0.1]")]
    float slowdownDistance = 2f;

    private Dictionary<int, Vector2> targetPositionsByEntity = new Dictionary<int, Vector2>();
    private Dictionary<int, UnitAI> unitAIByEntity = new Dictionary<int, UnitAI>();

    #region Public Methods
    public void SetTargetPosition(GameObject gameObject, Vector2 worldPosition, UnitAI notifyWhenReached)
    {
        int entityId = gameObject.GetInstanceID();
        targetPositionsByEntity[entityId] = worldPosition;
        unitAIByEntity[entityId] = notifyWhenReached;
    }
    #endregion

    protected override ChaseVector CalculateChaseVector(MovementBehaviorData data)
    {
        if(!HasTarget(data)) return new ChaseVector(Vector2.zero, null);

        Vector2 deltaPositionToTarget = CalculateDeltaPositionToTarget(data);
        if (CannotSeeTargetPosition(data)) return new ChaseVector(Vector2.ClampMagnitude(deltaPositionToTarget, 1), null);

        Vector2? targetVelocity = null;
        Vector2 myVelocityToTarget = CalculateMyVelocityToTarget(data, deltaPositionToTarget);
        Vector2 inputCounteractingSpin = -CalculatePerpendicularVelocityToTarget(data, myVelocityToTarget);
        if (deltaPositionToTarget.magnitude < slowdownDistance)
        {
            // Prioritize braking over turning when close to target position to avoid overshooting and oscillating around the target.
            Vector2 myVelocity = data.myTrajectory.GetVelocity();
            inputCounteractingSpin -= (1 - inputCounteractingSpin.magnitude) * myVelocityToTarget * myVelocity.sqrMagnitude;
            targetVelocity = Vector2.zero;
        }
        else
        {
            // Prioritize turning over braking when farther from target position to allow for sharper turns towards the target.
            inputCounteractingSpin += (1 - inputCounteractingSpin.sqrMagnitude) * deltaPositionToTarget;
        }
        if(debugMovementVectors) Debug.DrawRay(data.transform.position, inputCounteractingSpin, Color.cyan, 0.1f);

        //if (deltaPositionToTarget.magnitude < distanceToReachPosition)
        //{
        //    //unitAIByEntity[entityId].NotifyReachedTargetPosition();
        //    return new ChaseVector(Vector2.zero, true);
        //}

        if (inputCounteractingSpin.magnitude > 1) inputCounteractingSpin.Normalize();
        return new ChaseVector(inputCounteractingSpin, targetVelocity);
    }
    private bool CannotSeeTargetPosition(MovementBehaviorData data)
    {
        int entityId = data.gameObject.GetInstanceID();
        return !GeometryUtils.IsPositionVisible(targetPositionsByEntity[entityId], data.transform.position, data.myTeam.team, ignoreObject: data.gameObject);
    }
    private Vector2 CalculatePerpendicularVelocityToTarget(MovementBehaviorData data, Vector2 myVelocityToTarget)
    {
        Vector2 myVelocity = data.myTrajectory.GetVelocity();
        return Vector2.ClampMagnitude(myVelocity - myVelocityToTarget, 0.95f);

    }
    private Vector2 CalculateMyVelocityToTarget(MovementBehaviorData data, Vector2 deltaPositionToTarget)
    {
        Vector2 myVelocity = data.myTrajectory.GetVelocity();
        return GeometryUtils.Project(myVelocity, deltaPositionToTarget);
    }
    private Vector2 CalculateDeltaPositionToTarget(MovementBehaviorData data)
    {
        int entityId = data.gameObject.GetInstanceID();
        Vector2 deltaPositionToTarget = targetPositionsByEntity[entityId] - (Vector2)data.transform.position;
        if (debugMovementVectors) Debug.DrawRay(data.transform.position, deltaPositionToTarget, Color.white);
        return deltaPositionToTarget;
    }
    private bool HasTarget(MovementBehaviorData data)
    {
        int entityId = data.gameObject.GetInstanceID();
        return targetPositionsByEntity.ContainsKey(entityId);
    }
    protected override void OnValidate()
    {
        base.OnValidate();
        if (slowdownDistance < stopChaseRange + 0.1f) slowdownDistance = stopChaseRange + 0.1f;
    }
}
