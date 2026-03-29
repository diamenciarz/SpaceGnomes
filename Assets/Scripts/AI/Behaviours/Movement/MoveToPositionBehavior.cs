using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Omnidirectional/MoveToPositionBehavior", fileName = "MoveToPositionBehavior")]
public class MoveToPositionBehavior : MovementBehavior
{
    protected override Vector2 CalculateChaseVector(MovementBehaviorData data)
    {
        GameObject chaseEntity = UpdateCurrentTarget(data);
        if (!chaseEntity) return Vector2.zero;
        Vector2 directionToTarget = CalculateDirectionToTarget(data, chaseEntity);

        if (debugMovementVectors) Debug.DrawRay(data.transform.position, directionToTarget, Color.white);

        if (directionToTarget.magnitude > 1) directionToTarget.Normalize();
        return directionToTarget;

    }
}
