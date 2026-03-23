using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/TailEntityBehavior", fileName = "TailEntityBehavior")]
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
    protected override Vector2 CalculateChaseVector(MovementBehaviorData data)
    {
        GameObject chaseEntity = UpdateCurrentTarget(data);
        if (!chaseEntity) return Vector2.zero;
        Vector2 directionToTarget = CalculateDirectionToTarget(data, chaseEntity);

        if (debugMovementVectors) Debug.DrawRay(data.transform.position, directionToTarget, Color.white);

        Vector2 inputVector = CalculateInputVector(data, directionToTarget, chaseEntity);
        if (inputVector.magnitude > 1) inputVector.Normalize();
        return inputVector;
    }
    private Vector2 CalculateInputVector(MovementBehaviorData data, Vector2 directionToTarget, GameObject chaseEntity)
    {
        if(directionToTarget.magnitude > slowdownDistance) return directionToTarget;

        Trajectory targetTrajectory = chaseEntity.GetComponent<Trajectory>();
        Vector2 myVelocity = data.myTrajectory.GetVelocity();
        Vector2 targetVelocity = targetTrajectory.GetVelocity();
        bool bothMoving = myVelocity.magnitude != 0 && targetVelocity.magnitude != 0;

        if(!bothMoving) return directionToTarget;

        Vector2 targetVelocityInMyDir = bothMoving ? GeometryUtils.Project(targetVelocity, myVelocity) : Vector2.zero;
        Vector2 deltaVelocity = targetVelocityInMyDir - myVelocity;
        float scalar = 1 - ((directionToTarget.magnitude - stopChaseRange) / (slowdownDistance - stopChaseRange));

        if (debugMovementVectors) Debug.DrawRay(data.transform.position, deltaVelocity * scalar * 2, Color.red);
        return deltaVelocity * scalar;

    }
}
