using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcMovementBehavior : MovementBehavior
{
    protected Vector2 DetermineWorldTarget(List<Vector2> offsets, Vector2 actualCenter, MovementBehaviorData data, GameObject chaseEntity)
    {
        Vector2 targetOffset = offsets[0];
        if (chaseMode == ChaseMode.ExtrapolateAndCollideWithTarget && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
        {
            Vector2 myVelocity = data.myRigidbody2D.velocity;
            const float MINIMUM_SPEED = 2f;
            float simulatedVelocity = Mathf.Max(myVelocity.magnitude, MINIMUM_SPEED);
            float reachTime = (targetTrajectory.GetCurrentPosition() - (Vector2)data.transform.position).magnitude / simulatedVelocity;
            Vector2 predictedCenter = targetTrajectory.ExtrapolateFuturePosition(reachTime);
            return predictedCenter + targetOffset;
        }
        else
        {
            return actualCenter + targetOffset;
        }
    }

    protected void DrawDebugLines(MovementBehaviorData data, Vector2 worldTarget, List<Vector2> offsets, Vector2 actualCenter)
    {
        if (debugMovementVectors)
        {
            Debug.DrawLine(data.transform.position, worldTarget, Color.green);
            for (int i = 0; i < offsets.Count; i++)
            {
                Vector2 p = actualCenter + offsets[i];
                Debug.DrawLine(actualCenter, p, Color.yellow);
                Debug.DrawLine(p, p + Vector2.up * 0.01f, Color.cyan);
            }
        }
    }
    protected int FindClosestPointIndex(List<Vector2> offsets, Vector2 center, Vector2 shipPos)
    {
        int closestIndex = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < offsets.Count; i++)
        {
            Vector2 worldPoint = center + offsets[i];
            float d = Vector2.SqrMagnitude(worldPoint - shipPos);
            if (d < bestDist)
            {
                bestDist = d;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    // Map vector to angle degrees where 0 is world-up (+Y)
    protected float AngleDegFromVector(Vector2 v)
    {
        if (v.sqrMagnitude == 0) return 0f;
        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        ang = ang - 90f; // convert so 0 = up
        return Mathf.Repeat(ang, 360f);
    }

    // Convert angle (0 = up) and radius to vector
    protected Vector2 VectorFromAngleDeg(float angleDeg, float radius)
    {
        float rad = (angleDeg + 90f) * Mathf.Deg2Rad;
        return new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
    }
}
