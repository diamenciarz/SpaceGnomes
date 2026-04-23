using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ArcMovementBehavior : MovementBehavior
{
    /// <summary>
    /// A large radius value used for raycasting to determine the thickness of the chase entity's collider when calculating the offset radius.
    /// </summary>

    /// <summary>
    /// Determines the world target position to move towards based on the provided arc offsets and chase mode.
    /// If chase mode is set to ExtrapolateAndCollideWithTarget and the chase entity has a Trajectory component, it will predict the future position of the target and use that as the center for the arc offsets.
    /// Otherwise, it will use the current position of the chase entity as the center for the arc offsets.
    /// </summary>
    /// <param name="offsets"></param>
    /// <param name="actualCenter"></param>
    /// <param name="data"></param>
    /// <param name="chaseEntity"></param>
    /// <returns></returns>
    protected Vector2 DetermineWorldTargetPosition(List<Vector2> offsets, Vector2 actualCenter, MovementBehaviorData data, GameObject chaseEntity)
    {
        if(!data.myRigidbody2D) Debug.LogError($"ArcMovementBehavior requires a Rigidbody2D component on {data.gameObject} using it.");
        if(!chaseEntity.TryGetComponent(out Trajectory targetTrajectory)) Debug.LogError($"ArcMovementBehavior requires a Trajectory component on {data.gameObject} using it.");
        
        Vector2 targetOffset = offsets[0];
        if (chaseMode == ChaseMode.ExtrapolateAndCollideWithTarget && targetTrajectory && data.myRigidbody2D)
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
                Vector2 targetPositions = actualCenter + offsets[i];
                Debug.DrawLine(actualCenter, targetPositions, Color.yellow);
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
        return GeometryUtils.DirectionVectorToAngle(v);
    }

    // Convert angle (0 = up) and radius to vector
    protected Vector2 VectorFromAngleDeg(float angleDeg, float radius)
    {
        return GeometryUtils.AngleToDirectionVector(angleDeg, radius);
    }
    protected float GetOffsetRadius(GameObject chaseEntity, MovementBehaviorData data, float initialRadius, float offsetAngle)
    {
        // Calculate the radius needed to avoid colliding with the chase entity,
        // by raycasting from the center of the chase entity in the direction of the offset and checking how thick is its collider.
        // We add this extra radius to the initial radius to ensure we don't collide with the chase entity.
        GameObject chaseParent = EntityCounter.Instance.GetEntityParent(chaseEntity);
        Vector2 hitPoint = GetParentShipColliderHitPoint(chaseParent, offsetAngle);
        float deltaColliderRadius = ((Vector2)chaseParent.transform.position - hitPoint).magnitude;

        return initialRadius + deltaColliderRadius;
    }
    private Vector2 GetParentShipColliderHitPoint(GameObject chaseParent, float offsetAngle)
    {
        Vector2 offsetPosition = (Vector2)chaseParent.transform.position + VectorFromAngleDeg(offsetAngle, GeometryUtils.RADIUS_OF_THE_LARGEST_SHIP);
        RaycastHit2D[] hits = GeometryUtils.RaycastInLine(chaseParent.transform.position, offsetPosition, GeometryUtils.RADIUS_OF_THE_LARGEST_SHIP);
        // Select the hit that corresponds to the chaseParent's collider
        RaycastHit2D chaseParentHit = hits.First(hit => hit.collider && hit.collider.gameObject == chaseParent);
        return chaseParentHit.point;
    }
}
