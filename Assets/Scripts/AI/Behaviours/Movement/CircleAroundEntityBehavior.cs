using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CircleAroundEntityBehavior", fileName = "EnemyBehavior")]
public class CircleAroundEntityBehavior : MovementBehavior
{
    [Header("Fly Around Settings")]
    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("The delay in seconds between attempts to find a new position to fly around the target entity.")]
    float findNewPositionDelay = 2;
    [SerializeField][Range(0.1f, 10f)] float randomPositionMinRadius = 3;
    [SerializeField][Range(0.1f, 10f)] float randomPositionMaxRadius = 6;

    // Arc control for generating random positions around the target
    [SerializeField]
    [Range(0f, 360f)]
    [Tooltip("Starting angle (degrees). 0 is at the top.")]
    float startingAngle = 0f;
    [SerializeField]
    [Range(0f, 360f)]
    [Tooltip("Arc width in degrees for random angle selection.")]
    float arcWidth = 360f;

    private float lastNewPositionTime;
    private Vector2 deltaPosToTarget;

    protected override Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        if (Time.time - lastNewPositionTime > findNewPositionDelay) GenerateDeltaPositionToTarget();
        if (chaseMode == ChaseMode.ExtrapolateTrajectory && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
        {
            Vector2 myVelocity = data.myRigidbody2D.velocity;
            float mySpeed = myVelocity.magnitude;
            const float MINIMUM_SPEED = 2f;
            float simulatedVelocity = Mathf.Max(myVelocity.magnitude, MINIMUM_SPEED);
            Vector2 relativeVelocity = targetTrajectory.GetVelocity() - myVelocity;
            float reachTime = (targetTrajectory.GetCurrentPosition() - (Vector2)data.transform.position).magnitude / simulatedVelocity;
            Vector2 predictedHitCoords = targetTrajectory.ExtrapolateFuturePosition(reachTime);

            // Apply the offset around the target
            Vector2 offsetTarget = predictedHitCoords + deltaPosToTarget;

            if (debugMovementVectors)
            {
                Debug.DrawLine(chaseEntity.transform.position, offsetTarget, Color.green);
            }
            return offsetTarget - (Vector2)data.transform.position;

        }
        else
        {
            // Aim for the point offset from the chase entity's position
            Vector2 targetPos = (Vector2)chaseEntity.transform.position + deltaPosToTarget;
            if (debugMovementVectors)
            {
                Debug.DrawLine(data.transform.position, targetPos, Color.green);
            }
            return targetPos - (Vector2)data.transform.position;
        }
    }
    private Vector2 GenerateDeltaPositionToTarget()
    {
        // Pick an angle within the configured arc. 0 degrees is treated as "up" (world +Y).
        float angleOffset = Random.Range(0f, arcWidth);
        float angle = startingAngle + angleOffset;
        angle = Mathf.Repeat(angle, 360f);

        float distance = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);

        // Convert to radians and rotate so that 0 degrees maps to world up (0,1).
        float rad = (angle + 90f) * Mathf.Deg2Rad;
        deltaPosToTarget = new Vector2(distance * Mathf.Cos(rad), distance * Mathf.Sin(rad));
        lastNewPositionTime = Time.time;
        return deltaPosToTarget;
    }
}