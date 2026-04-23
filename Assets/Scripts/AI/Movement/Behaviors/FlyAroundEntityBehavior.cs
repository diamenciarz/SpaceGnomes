using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Omnidirectional/FlyAroundEntityBehavior", fileName = "FlyAroundEntityBehavior")]
public class FlyAroundEntityBehavior : ArcMovementBehavior
{
    [Header("Fly Around Settings")]
    [SerializeField][Range(0.1f, 10f)][Tooltip("The delay in seconds between attempts to find a new position to fly around the target entity.")]
    private float findNewPositionDelay= 2;
    [SerializeField][Range(0.1f, 10f)] protected float randomPositionMinRadius = 3;
    [SerializeField][Range(0.1f, 10f)] protected float randomPositionMaxRadius = 6;

    // Arc control for generating random positions around the target
    [SerializeField][Range(0f, 360f)][Tooltip("Starting angle (degrees). 0 is at the top.")]
    protected float startingAngle = 0f;
    [SerializeField][Range(0f, 360f)][Tooltip("Counterclockwise arc width in degrees for random angle selection.")]
    protected float arcWidth = 360f;

    [Header("Arc Path Settings")]
    [SerializeField][Range(1f, 90f)][Tooltip("Angular step (degrees) between generated arc points.")]
    protected float arcAngularStep = 10f;
    [SerializeField][Range(1f, 45f)][Tooltip("Angle threshold (degrees) to advance to the next arc point when close to it.")]
    protected float advanceAngleThreshold = 10f;
    [SerializeField] private bool regenerateArcOnReachingTarget = true;
    [SerializeField][Range(0.1f, 10f)][Tooltip("If the ship gets stuck and fails to reach the target point within this time after reaching it, regenerate the arc to try to get unstuck.")]
    protected float regenerateArcIfStuckFor = 2;

    // Per-entity state (keyed by chaseEntity parent GetInstanceID())
    private Dictionary<int, float> lastNewPositionTimes = new Dictionary<int, float>();
    // Offsets relative to the target center (so we can shift them for extrapolation)
    private Dictionary<int, List<Vector2>> arcOffsetsByEntity = new Dictionary<int, List<Vector2>>();
    private Dictionary<int, float> lastNewOffsetTimes = new Dictionary<int, float>();

    protected override void OnValidate()
    {
        // Currently frames drop when a large ship is flying around another ship. Avoiding collisions is not laggy.
        base.OnValidate();
        if (randomPositionMaxRadius < randomPositionMinRadius) randomPositionMaxRadius = randomPositionMinRadius;
    }
    protected override Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        chaseEntity = EntityCounter.Instance.GetEntityParent(chaseEntity);
        int entityId = chaseEntity.GetInstanceID();

        Vector2 actualCenter = (Vector2)chaseEntity.transform.position;
        List<Vector2> offsets = GetOrCreateArcOffsets(entityId, chaseEntity, data, actualCenter);

        Vector2 shipPos = (Vector2)data.transform.position;
        PruneArc(offsets, actualCenter, shipPos, entityId);
        AdvanceArc(offsets, actualCenter, shipPos, entityId);

        arcOffsetsByEntity[entityId] = offsets;

        Vector2 worldTarget = DetermineWorldTargetPosition(offsets, actualCenter, data, chaseEntity);

        DrawDebugLines(data, worldTarget, offsets, actualCenter);

        return worldTarget - shipPos;
    }

    protected virtual void PruneArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, int entityId)
    {
        int closestIndex = FindClosestPointIndex(offsets, actualCenter, shipPos);
        if (closestIndex > 0)
        {
            offsets.RemoveRange(0, closestIndex);
            lastNewOffsetTimes[entityId] = Time.time;
        }
    }

    protected virtual void AdvanceArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, int entityId)
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

    protected virtual List<Vector2> GetOrCreateArcOffsets(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualCenter)
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
        List<Vector2> newOffsets = BuildArcOffsets(actualCenter, newDelta, chaseEntity, data);
        arcOffsetsByEntity[entityId] = newOffsets;
        lastNewPositionTimes[entityId] = Time.time;
        lastNewOffsetTimes[entityId] = Time.time;
        return newOffsets;
    }

    protected Vector2 GenerateDeltaOffset()
    {
        float angleOffset = Random.Range(0f, arcWidth)-arcWidth/2;
        float angle = Mathf.Repeat(startingAngle + angleOffset, 360f);

        float distance = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);
        return VectorFromAngleDeg(angle, distance);
    }

    /// <summary>
    /// Builds a list of offsets along an arc from the current position to the desired deltaOffset around the center.
    /// The radius of the arc is determined by the magnitude of deltaOffset plus the thickness of the target's collider.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="deltaOffset"></param>
    /// <param name="chaseEntity"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected virtual List<Vector2> BuildArcOffsets(Vector2 center, Vector2 deltaOffset, GameObject chaseEntity, MovementBehaviorData data)
    {
        float radius = deltaOffset.magnitude;
        if (radius <= Mathf.Epsilon) return new List<Vector2> { deltaOffset };

        Vector2 myPos = (Vector2)data.transform.position;
        float startAngle = AngleDegFromVector(myPos - center);
        float endAngle = AngleDegFromVector(deltaOffset);
        float deltaAngle = Mathf.DeltaAngle(startAngle, endAngle);

        int steps = Mathf.CeilToInt(Mathf.Abs(deltaAngle) / arcAngularStep);
        float angleStep = deltaAngle / steps;

        List<Vector2> offsets = new List<Vector2>(steps);
        for (int i = 1; i <= steps; i++)
        {
            float angle = Mathf.Repeat(startAngle + angleStep * i, 360f);
            Vector2 offset = VectorFromAngleDeg(angle, GetOffsetRadius(chaseEntity, data, radius, angle));
            offsets.Add(offset);
        }
        return offsets;
    }
}