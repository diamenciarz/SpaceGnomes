using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Omnidirectional/TailEntityBehavior", fileName = "TailEntityBehavior")]
public class TailEntityBehavior : MovementBehavior
{
    [Header("Tail Entity Settings")]
    [SerializeField][Range(1f, 20f)][Tooltip("Will start slowing down if going too fast within this distance of target (tail) position. [Must be greater than stopChaseRange + 0.1]")]
    float slowdownDistance = 2f;
    protected override void OnValidate()
    {
        base.OnValidate();
        if (slowdownDistance < stopChaseRange+0.1f) slowdownDistance = stopChaseRange+0.1f;
    }
    protected override ChaseVector CalculateChaseVector(MovementBehaviorData data)
    {
        GameObject chaseEntity = UpdateCurrentTarget(data);
        if (!chaseEntity) return new ChaseVector(Vector2.zero, null);
        Vector2 deltaPositionToTarget = CalculateDirectionToTarget(data, chaseEntity);


        ChaseVector inputVector = CalculateInputVector(data, deltaPositionToTarget, chaseEntity);
        if (inputVector.vector.magnitude > 1) inputVector.vector.Normalize();
        return inputVector;
    }
    private ChaseVector CalculateInputVector(MovementBehaviorData data, Vector2 deltaPositionToTarget, GameObject chaseEntity)
    {
        
        if (CannotSeeTarget(data, chaseEntity))
        {
            Debug.Log("Cannot see target position.");
            return new ChaseVector(Vector2.ClampMagnitude(deltaPositionToTarget, 1), null);
        }

        Vector2? targetVelocity = null;
        Vector2 chaseTargetVelocity = GetTargetVelocity(chaseEntity);
        Vector2 deltaVelocity = CalculateDeltaVelocity(data, chaseTargetVelocity);
        Vector2 deltaVelocityToTarget = CalculateDeltaVelocityToTarget(data, deltaVelocity, deltaPositionToTarget);
        Vector2 inputCounteractingSpin = -CalculatePerpendicularVelocityToTarget(data, deltaVelocity, deltaVelocityToTarget);
        if (deltaPositionToTarget.magnitude < slowdownDistance)
        {
            // Prioritize braking over turning when close to target position to avoid overshooting and oscillating around the target.
            inputCounteractingSpin = -deltaVelocity;// * Mathf.Clamp01(deltaVelocity.sqrMagnitude);
            if (debugMovementVectors) Debug.DrawRay(data.transform.position, -deltaVelocity, Color.white);
            targetVelocity = chaseTargetVelocity;
        }
        else
        {
            // Prioritize turning over braking when farther from target position to allow for sharper turns towards the target.
            inputCounteractingSpin += (1 - inputCounteractingSpin.sqrMagnitude) * deltaPositionToTarget * Mathf.Clamp01(deltaVelocity.sqrMagnitude);
            if (debugMovementVectors) Debug.DrawRay(data.transform.position, deltaPositionToTarget, Color.red);
        }
        if (debugMovementVectors) Debug.DrawRay(data.transform.position, inputCounteractingSpin, Color.cyan, 0.1f);

        if (inputCounteractingSpin.magnitude > 1) inputCounteractingSpin.Normalize();
        return new ChaseVector(inputCounteractingSpin, targetVelocity);
    }
    private bool CannotSeeTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        int entityId = data.gameObject.GetInstanceID();
        return !GeometryUtils.IsObjectVisible(chaseEntity, data.transform.position, data.myTeam.team, ignoreObject: data.gameObject);
    }
    private Vector2 GetTargetVelocity(GameObject chaseEntity)
    {
        return chaseEntity.TryGetComponent(out Trajectory chaseTrajectory) ? chaseTrajectory.GetVelocity() : Vector2.zero;
    }
    private Vector2 CalculateDeltaVelocity(MovementBehaviorData data, Vector2 chaseTargetVelocity)
    {
        return data.myTrajectory.GetVelocity() - chaseTargetVelocity;
    }
    private Vector2 CalculateDeltaVelocityToTarget(MovementBehaviorData data, Vector2 deltaVelocity, Vector2 deltaPositionToTarget)
    {
        return GeometryUtils.Project(deltaVelocity, deltaPositionToTarget);
    }
    private Vector2 CalculatePerpendicularVelocityToTarget(MovementBehaviorData data, Vector2 deltaVelocity, Vector2 deltaVelocityToTarget)
    {
        return Vector2.ClampMagnitude(deltaVelocity - deltaVelocityToTarget, 0.95f);
    }
}
