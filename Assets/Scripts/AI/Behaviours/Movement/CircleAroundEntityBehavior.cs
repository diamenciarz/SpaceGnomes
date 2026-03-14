using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CircleAroundEntityBehavior", fileName = "EnemyBehavior")]
public class CircleAroundEntityBehavior : ArcMovementBehavior
{

    [Header("Circle Around Settings")]
    [SerializeField][Range(0.1f, 10f)] private float randomPositionMinRadius = 3;
    [SerializeField][Range(0.1f, 10f)] private float randomPositionMaxRadius = 6;

    [Header("Arc Path Settings")]
    [SerializeField][Range(1f, 90f)][Tooltip("Angular step (degrees) between generated arc points.")]
    private float arcAngularStep = 10f;
    [SerializeField][Range(1f, 180f)][Tooltip("Angle threshold (degrees) to advance to the next arc point when close to it.")]
    private float advanceAngleThreshold = 10f;
    [SerializeField][Range(0.1f, 10f)][Tooltip("If the ship gets stuck and fails to reach the target point within this time after reaching it, regenerate the arc to try to get unstuck.")]
    private float regenerateArcIfStuckFor = 2;

    private Dictionary<int, List<Vector2>> arcOffsetsByEntity = new Dictionary<int, List<Vector2>>();
    private Dictionary<int, float> lastNewOffsetTimes = new Dictionary<int, float>();

    private bool addingArcPointsClockwise;
    private float lastArcPointAngle;

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

        Vector2 worldTarget = DetermineWorldTarget(offsets, actualCenter, data, chaseEntity);

        DrawDebugLines(data, worldTarget, offsets, actualCenter);

        return worldTarget - shipPos;
    }
    private List<Vector2> GetOrCreateArcOffsets(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualCenter)
    {
        if (!lastNewOffsetTimes.TryGetValue(entityId, out var lastNewOffsetTime))
        {
            lastNewOffsetTimes[entityId] = Time.time;
        }
        bool isStuck = Time.time - lastNewOffsetTime > regenerateArcIfStuckFor;
        if (isStuck)
        {
            // Reset the arc in an opposite direction to try to get unstuck
            addingArcPointsClockwise = !addingArcPointsClockwise;
            InitializeNewArc(entityId, chaseEntity, data, actualCenter);
            return arcOffsetsByEntity[entityId];
        }

        bool arcExists = arcOffsetsByEntity.TryGetValue(entityId, out var existing);// && existing != null && existing.Count > 0;
        if (!arcExists)
        {
            InitializeNewArc(entityId, chaseEntity, data, actualCenter);
        }

        return arcOffsetsByEntity[entityId];
    }
    private void InitializeNewArc(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualCenter)
    {
        // Instantiate a new arc by adding current ship position
        float startAngle = AngleDegFromVector((Vector2) data.transform.position - actualCenter);
        float initialRadius = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);
        Vector2 p = VectorFromAngleDeg(startAngle, initialRadius);
        arcOffsetsByEntity[entityId] = new List<Vector2> { p };
        lastArcPointAngle = startAngle;

        // Then one more point in the direction of the arc
        AddArcPoint(arcOffsetsByEntity[entityId], actualCenter, entityId);
    }
    private void PruneArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, int entityId)
    {
        int closestIndex = FindClosestPointIndex(offsets, actualCenter, shipPos);
        if (closestIndex > 0)
        {
            offsets.RemoveRange(0, closestIndex);
            // Add one new point at the end of the arc
            AddArcPoint(offsets, actualCenter, entityId);
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
                AddArcPoint(offsets, actualCenter, entityId);
            }
        }
    }
    private void AddArcPoint(List<Vector2> offsets, Vector2 actualCenter, int entityId)
    {
        float deltaAngle = addingArcPointsClockwise ? -arcAngularStep : arcAngularStep;
        float angle = lastArcPointAngle + deltaAngle;
        angle = Mathf.Repeat(angle, 360f);
        float radius = offsets[0].magnitude;
        Vector2 p = VectorFromAngleDeg(angle, radius);
        offsets.Add(p);
        lastArcPointAngle = angle;
        lastNewOffsetTimes[entityId] = Time.time;
    }
}