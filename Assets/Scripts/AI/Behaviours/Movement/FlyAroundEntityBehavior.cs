using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/FlyAroundEntityBehavior", fileName ="EnemyBehavior")]
public class FlyAroundEntityBehavior : MovementBehavior
{
    [Header("Fly Around Settings")]
    [SerializeField][Range(0.1f, 10f)][Tooltip("The delay in seconds between attempts to find a new position to fly around the target entity.")]
    float findNewPositionDelay= 2;
    [SerializeField][Range(0.1f, 10f)] float randomPositionMinRadius = 3;
    [SerializeField][Range(0.1f, 10f)] float randomPositionMaxRadius = 6;

    // Arc control for generating random positions around the target
    [SerializeField][Range(0f, 360f)][Tooltip("Starting angle (degrees). 0 is at the top.")]
    float startingAngle = 0f;
    [SerializeField][Range(0f, 360f)][Tooltip("Arc width in degrees for random angle selection.")]
    float arcWidth = 360f;

    [Header("Arc Path Settings")]
    [SerializeField][Range(1f, 90f)][Tooltip("Angular step (degrees) between generated arc points.")]
    float arcAngularStep = 10f;
    [SerializeField][Range(1f, 45f)][Tooltip("Angle threshold (degrees) to advance to the next arc point when close to it.")]
    float advanceAngleThreshold = 10f;
    [SerializeField] bool regenerateArcOnReachingTarget = true;
    [SerializeField][Range(0.1f, 10f)][Tooltip("If the ship gets stuck and fails to reach the target point within this time after reaching it, regenerate the arc to try to get unstuck.")]
    float regenerateArcIfStuckFor = 2;

    // Per-entity state (keyed by chaseEntity parent GetInstanceID())
    private Dictionary<int, float> lastNewPositionTimes = new Dictionary<int, float>();
    // Offsets relative to the target center (so we can shift them for extrapolation)
    private Dictionary<int, List<Vector2>> arcOffsetsByEntity = new Dictionary<int, List<Vector2>>();
    private Dictionary<int, float> lastNewOffsetTimes = new Dictionary<int, float>();

    protected override Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        chaseEntity = EntityCounter.Instance.GetEntityParent(chaseEntity);
        int entityId = chaseEntity.GetInstanceID();

        Vector2 actualCenter = (Vector2)chaseEntity.transform.position;
        List<Vector2> offsets = GetOrCreateArcOffsets(entityId, chaseEntity, data, actualCenter);

        if (offsets == null || offsets.Count == 0)
        {
            return HandleFallback(data, actualCenter, entityId, chaseEntity);
        }

        Vector2 shipPos = (Vector2)data.transform.position;
        PruneArc(offsets, actualCenter, shipPos, entityId);
        AdvanceArc(offsets, actualCenter, shipPos, entityId);

        arcOffsetsByEntity[entityId] = offsets;

        Vector2 worldTarget = DetermineWorldTarget(offsets, actualCenter, data, chaseEntity);

        DrawDebugLines(data, worldTarget, offsets, actualCenter);

        return worldTarget - shipPos;
    }

    private Vector2 HandleFallback(MovementBehaviorData data, Vector2 actualCenter, int entityId, GameObject chaseEntity)
    {
        Debug.LogError($"Failed to generate arc offsets for entity {chaseEntity.name} (ID: {entityId}).");
        Vector2 fallbackDelta = GenerateDeltaOffset();
        Vector2 fallbackTarget = actualCenter + fallbackDelta;
        if (debugMovementVectors) Debug.DrawLine(data.transform.position, fallbackTarget, Color.green);
        return fallbackTarget - (Vector2)data.transform.position;
    }

    private void PruneArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, int entityId)
    {
        int closestIndex = FindClosestPointIndex(offsets, actualCenter, shipPos);
        if (closestIndex > 0)
        {
            offsets.RemoveRange(0, closestIndex);
            lastNewOffsetTimes[entityId] = Time.time;
        }
    }

    private void AdvanceArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, int entityId)
    {
        if (offsets.Count >= 2)
        {
            float shipAngle = AngleDegFromVector(shipPos - actualCenter);
            float secondAngle = AngleDegFromVector(offsets[1]);
            if (Mathf.Abs(Mathf.DeltaAngle(shipAngle, secondAngle)) <= advanceAngleThreshold)
            {
                offsets.RemoveAt(0);
                lastNewOffsetTimes[entityId] = Time.time;
            }
        }
    }

    private Vector2 DetermineWorldTarget(List<Vector2> offsets, Vector2 actualCenter, MovementBehaviorData data, GameObject chaseEntity)
    {
        Vector2 targetOffset = offsets[0];
        if (chaseMode == ChaseMode.ExtrapolateTrajectory && chaseEntity.TryGetComponent(out Trajectory targetTrajectory) && data.myRigidbody2D)
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

    private void DrawDebugLines(MovementBehaviorData data, Vector2 worldTarget, List<Vector2> offsets, Vector2 actualCenter)
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

    private List<Vector2> GetOrCreateArcOffsets(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualCenter)
    {
        if (!lastNewOffsetTimes.TryGetValue(entityId, out var lastNewOffsetTime))
        {
            lastNewOffsetTimes[entityId] = Time.time;
        }

        if (lastNewPositionTimes.TryGetValue(entityId, out float lastTime))
        {
            if (Time.time - lastTime <= findNewPositionDelay)
            {
                bool arcExists = arcOffsetsByEntity.TryGetValue(entityId, out var existing) && existing != null && existing.Count > 0;
                if (arcExists)
                {
                    bool reachedEndOfArc = regenerateArcOnReachingTarget && existing.Count == 1;
                    if(!reachedEndOfArc)
                    {
                        bool isStuck = Time.time - lastNewOffsetTime > regenerateArcIfStuckFor;
                        if (!isStuck) return existing;
                    }
                }
            }
        }

        // Generate new arc
        Vector2 newDelta = GenerateDeltaOffset();
        List<Vector2> newOffsets = BuildArcOffsets(actualCenter, (Vector2)data.transform.position, newDelta);
        arcOffsetsByEntity[entityId] = newOffsets;
        lastNewPositionTimes[entityId] = Time.time;
        lastNewOffsetTimes[entityId] = Time.time;
        return newOffsets;
    }

    private Vector2 GenerateDeltaOffset()
    {
        // Pick an angle within the configured arc. 0 degrees is treated as "up" (world +Y).
        float angleOffset = Random.Range(0f, arcWidth);
        float angle = startingAngle + angleOffset;
        angle = Mathf.Repeat(angle, 360f);

        float distance = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);

        // Convert to radians and rotate so that 0 degrees maps to world up (0,1).
        float rad = (angle + 90f) * Mathf.Deg2Rad;
        Vector2 delta = new Vector2(distance * Mathf.Cos(rad), distance * Mathf.Sin(rad));
        return delta;
    }

    // Build arc offsets (relative to center) that move from the ship's current angular position to the desired delta offset.
    private List<Vector2> BuildArcOffsets(Vector2 center, Vector2 myPos, Vector2 deltaOffset)
    {
        float radius = deltaOffset.magnitude;
        if (radius <= Mathf.Epsilon)
        {
            return new List<Vector2> { deltaOffset };
        }

        float startAngle = AngleDegFromVector(myPos - center);
        float endAngle = AngleDegFromVector(deltaOffset);
        float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);

        int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(deltaAngle) / arcAngularStep));
        float angleStep = deltaAngle / steps;

        List<Vector2> offsets = new List<Vector2>(steps);
        for (int i = 1; i <= steps; i++)
        {
            float angle = startAngle + angleStep * i;
            angle = Mathf.Repeat(angle, 360f);
            Vector2 p = VectorFromAngleDeg(angle, radius);
            offsets.Add(p);
        }

        // Ensure final point exactly equals the desired deltaOffset (avoid floating error)
        offsets[offsets.Count - 1] = deltaOffset;
        return offsets;
    }

    private int FindClosestPointIndex(List<Vector2> offsets, Vector2 center, Vector2 shipPos)
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
    private float AngleDegFromVector(Vector2 v)
    {
        if (v.sqrMagnitude == 0) return 0f;
        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        ang = ang - 90f; // convert so 0 = up
        return Mathf.Repeat(ang, 360f);
    }

    // Convert angle (0 = up) and radius to vector
    private Vector2 VectorFromAngleDeg(float angleDeg, float radius)
    {
        float rad = (angleDeg + 90f) * Mathf.Deg2Rad;
        return new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
    }
}