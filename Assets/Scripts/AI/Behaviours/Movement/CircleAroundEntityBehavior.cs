using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CircleAroundEntityBehavior", fileName = "EnemyBehavior")]
public class CircleAroundEntityBehavior : MovementBehavior
{
    [Header("Orbit Settings")]
    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("Minimum orbit radius (used to pick a deterministic radius between min and max).")]
    float randomPositionMinRadius = 3f;

    [SerializeField]
    [Range(0.1f, 20f)]
    [Tooltip("Maximum orbit radius (used to pick a deterministic radius between min and max).")]
    float randomPositionMaxRadius = 6f;

    [SerializeField]
    [Range(0f, 360f)]
    [Tooltip("Starting angle (degrees). 0 is at the top.")]
    float startingAngle = 0f;

    [SerializeField]
    [Range(0f, 360f)]
    [Tooltip("Arc width in degrees for the orbit. 360 = full circle.")]
    float arcWidth = 360f;

    [SerializeField]
    [Range(-360f, 360f)]
    [Tooltip("Angular speed in degrees per second for the orbiting point.")]
    float angularSpeed = 30f;

    [SerializeField]
    [Tooltip("If true, each target will get a deterministic phase offset so multiple chasers stagger their positions.")]
    bool usePhaseOffset = true;

    protected override Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        if (chaseEntity == null) return Vector2.zero;

        // Determine center of orbit. If extrapolating, predict the target's future position and orbit around that.
        Vector2 center;
        if (chaseMode == ChaseMode.ExtrapolateTrajectory && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
        {
            Vector2 myVelocity = data.myRigidbody2D.velocity;
            const float MINIMUM_SPEED = 2f;
            float simulatedVelocity = Mathf.Max(myVelocity.magnitude, MINIMUM_SPEED);
            float reachTime = (targetTrajectory.GetCurrentPosition() - (Vector2)data.transform.position).magnitude / simulatedVelocity;
            center = targetTrajectory.ExtrapolateFuturePosition(reachTime);
        }
        else
        {
            center = chaseEntity.transform.position;
        }

        // Deterministic radius chosen between min and max so the orbit radius doesn't jitter every frame.
        float radius = DetermineDeterministicRadius(chaseEntity);

        // Compute angle using Time.time so the point moves along the arc at a consistent rate.
        float baseAngle = startingAngle;
        if (usePhaseOffset) baseAngle += GetDeterministicPhaseOffset(chaseEntity);

        float sweepDegrees = angularSpeed * Time.time;
        if (arcWidth < 360f)
        {
            // Keep the sweep constrained to the configured arc width
            sweepDegrees = Mathf.Repeat(sweepDegrees, arcWidth);
        }

        float angle = baseAngle + sweepDegrees;
        angle = Mathf.Repeat(angle, 360f);

        // Convert angle to world-space offset. 0 degrees corresponds to world-up (positive Y).
        float rad = (angle + 90f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));

        Vector2 orbitPoint = center + offset;

        if (debugMovementVectors)
        {
            Debug.DrawLine(data.transform.position, orbitPoint, Color.green);
            Debug.DrawLine(center, orbitPoint, Color.yellow);
        }

        return orbitPoint - (Vector2)data.transform.position;
    }

    // Pick a stable radius between min and max using the chaseEntity instance id so it doesn't change every frame.
    private float DetermineDeterministicRadius(GameObject chaseEntity)
    {
        if (Mathf.Approximately(randomPositionMinRadius, randomPositionMaxRadius)) return randomPositionMinRadius;
        int id = chaseEntity.GetInstanceID();
        int absId = id == int.MinValue ? int.MaxValue : Mathf.Abs(id);
        float frac = (absId % 1000) / 1000f; // stable fraction in [0,0.999]
        return Mathf.Lerp(randomPositionMinRadius, randomPositionMaxRadius, frac);
    }

    // Provide a deterministic phase offset (in degrees) so different chasers don't all sit on the exact same point.
    private float GetDeterministicPhaseOffset(GameObject chaseEntity)
    {
        int id = chaseEntity.GetInstanceID();
        int absId = id == int.MinValue ? int.MaxValue : Mathf.Abs(id);
        float frac = (absId % 360) / 360f; // 0..0.997
        return frac * arcWidth;
    }
}