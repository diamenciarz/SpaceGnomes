using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This behavior generates a dynamic arc of points around the target entity and moves towards the next point in the arc, creating a circling movement pattern.
/// The arc is continuously updated based on the ship's position and the target's position, allowing for smooth circling even if the target moves.
/// If the ship gets stuck and fails to reach the next point within a certain time, a new arc is generated in an attempt to get unstuck.
/// </summary>
[CreateAssetMenu(menuName= "ScriptableObjects/Behaviors/Movement/Omnidirectional/CircleAroundEntityBehavior", fileName = "CircleAroundEntityBehavior")]
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

    protected override void OnValidate()
    {
        base.OnValidate();
        if (randomPositionMaxRadius < randomPositionMinRadius) randomPositionMaxRadius = randomPositionMinRadius;
    }

    protected override Vector2 CalculateDirectionToTarget(MovementBehaviorData data, GameObject chaseEntity)
    {
        chaseEntity = EntityCounter.Instance.GetEntityParent(chaseEntity);
        int entityId = chaseEntity.GetInstanceID();
        GameObject chaseParent = EntityCounter.Instance.GetEntityParent(chaseEntity);
        Vector2 actualChaseEntityCenter = chaseParent.transform.position;
        List<Vector2> offsets = GetOrCreateArcOffsets(entityId, chaseEntity, data, actualChaseEntityCenter);

        Vector2 shipPos = (Vector2)data.transform.position;
        PruneArc(offsets, actualChaseEntityCenter, shipPos, data, chaseEntity);
        AdvanceArc(offsets, actualChaseEntityCenter, shipPos, data, chaseEntity);

        arcOffsetsByEntity[entityId] = offsets;

        Vector2 worldTarget = DetermineWorldTargetPosition(offsets, actualChaseEntityCenter, data, chaseEntity);

        DrawDebugLines(data, worldTarget, offsets, actualChaseEntityCenter);

        return worldTarget - shipPos;
    }
    private List<Vector2> GetOrCreateArcOffsets(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualChaseEntityCenter)
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
            CreateNewArc(entityId, chaseEntity, data, actualChaseEntityCenter);
            return arcOffsetsByEntity[entityId];
        }

        bool arcExists = arcOffsetsByEntity.TryGetValue(entityId, out var existing);// && existing != null && existing.Count > 0;
        if (!arcExists)
        {
            CreateNewArc(entityId, chaseEntity, data, actualChaseEntityCenter);
        }

        return arcOffsetsByEntity[entityId];
    }
    private void CreateNewArc(int entityId, GameObject chaseEntity, MovementBehaviorData data, Vector2 actualChaseEntityCenter)
    {
        // Instantiate a new arc by adding current ship position
        float startAngle = AngleDegFromVector((Vector2) data.transform.position - actualChaseEntityCenter);
        float initialRadius = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);
        Vector2 p = VectorFromAngleDeg(startAngle, GetOffsetRadius(chaseEntity, data, initialRadius, startAngle));
        arcOffsetsByEntity[entityId] = new List<Vector2> { p };
        lastArcPointAngle = startAngle;

        // Then one more point in the direction of the arc
        AddArcPoint(arcOffsetsByEntity[entityId], actualChaseEntityCenter, data, chaseEntity);
    }
    
    private void PruneArc(List<Vector2> offsets, Vector2 actualCenter, Vector2 shipPos, MovementBehaviorData data, GameObject chaseEntity)
    {
        int closestIndex = FindClosestPointIndex(offsets, actualCenter, shipPos);
        if (closestIndex > 0)
        {
            offsets.RemoveRange(0, closestIndex);
            // Add one new point at the end of the arc
            AddArcPoint(offsets, actualCenter, data, chaseEntity);
        }
    }

    private void AdvanceArc(List<Vector2> offsets, Vector2 actualChaseEntityCenter, Vector2 shipPos, MovementBehaviorData data, GameObject chaseEntity)
    {
        if (offsets.Count >= 2)
        {
            float shipAngle = AngleDegFromVector(shipPos - actualChaseEntityCenter);
            float secondAngle = AngleDegFromVector(offsets[1]);
            if (Mathf.Abs(Mathf.DeltaAngle(shipAngle, secondAngle)) <= advanceAngleThreshold)
            {
                offsets.RemoveAt(0);
                AddArcPoint(offsets, actualChaseEntityCenter, data, chaseEntity);
            }
        }
    }
    private void AddArcPoint(List<Vector2> offsets, Vector2 actualCenter, MovementBehaviorData data, GameObject chaseEntity)
    {
        float deltaAngle = addingArcPointsClockwise ? -arcAngularStep : arcAngularStep;
        float angle = lastArcPointAngle + deltaAngle;
        angle = Mathf.Repeat(angle, 360f);
        float initialRadius = Random.Range(randomPositionMinRadius, randomPositionMaxRadius);
        Vector2 p = VectorFromAngleDeg(angle, GetOffsetRadius(chaseEntity, data, initialRadius, angle));
        offsets.Add(p);
        lastArcPointAngle = angle;
        lastNewOffsetTimes[chaseEntity.GetInstanceID()] = Time.time;
    }
}