using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [SerializeField][Tooltip("Save a path of n positions and velocities. -1 for unlimited")]
    private int saveStates = 10;


    private List<Vector2> positions = new List<Vector2>();
    private List<Vector2> velocities = new List<Vector2>();
    private List<float> deltaTimes = new List<float>();
    private Vector2 previousVelocity = Vector2.zero;
    private Vector2 cachedAcceleration;
    private Vector2 CurrentAcceleration => CalculateAcceleration();
    private Vector2 previousPosition;

    // This position is used to track movement for EntityCounter updates, so it measures distance, not velocity
    private Vector2 lastMovedPosition = Vector2.zero;
    private Collider2D myCollider;
    private bool calculatedAcc = false;
    public Collider2D MyCollider => myCollider;

    public struct CollisionInfo
    {
        public float time; // The time at which these trajectories' colliders first contact
        public Vector2 collisionPosition; // Collision position
        public float collisionSpeed;
        public Vector2 myPoint;
        public Vector2 otherPoint;
        public Vector2 myVelocity;
        public Vector2 otherVelocity;
        public bool otherIsWall; // If true, otherVelocity is going along the wall in the direction of myVelocity, not the velocity of the wall itself (which is 0)
    }

    public class TrajectoryInstance
    {
        public TrajectoryInstance(Trajectory trajectory)
        {
            // If any code needs them -> uncomment
            //positions = new List<Vector2>(trajectory.positions);
            //deltaTimes = new List<Vector2>(trajectory.deltaTimes);
            previousVelocity = trajectory.previousVelocity;
            currentAcceleration = trajectory.CurrentAcceleration;
            previousPosition = trajectory.previousPosition;
            currentPosition = trajectory.GetCurrentPosition();
            lastMovedPosition = trajectory.lastMovedPosition;
            MyCollider = trajectory.MyCollider;
        }

        public List<Vector2> positions => throw new System.Exception("Positions are unused right now.");
        //public List<Vector2> positions = new List<Vector2>();
        public List<float> deltaTimes => throw new System.Exception("DeltaTimes are unused right now.");
        //public List<float> deltaTimes = new List<float>();
        public Vector2 previousVelocity;
        public Vector2 currentAcceleration;
        public Vector2 previousPosition;
        public Vector2 currentPosition;
        public Vector2 lastMovedPosition;
        public Collider2D MyCollider;

        public Vector2 ExtrapolateFutureVelocity(float deltaTime)
        {
            return previousVelocity + currentAcceleration * deltaTime;
        }

        public Vector2 ExtrapolateFuturePosition(float deltaTime)
        {
            return previousPosition + previousVelocity * deltaTime;// + (currentAcceleration * deltaTime * deltaTime / 2);
        }

        public CollisionInfo? WillObjectsCollide(Trajectory other, float maxTime)
        {
            return Trajectory.WillObjectsCollide(this, other.Copy(), maxTime);
        }
        public CollisionInfo? WillObjectsCollide(TrajectoryInstance other, float maxTime)
        {
            return Trajectory.WillObjectsCollide(this, other, maxTime);
        }
    }

    private void Start()
    {
        previousPosition = transform.position;
        myCollider = GeometryUtils.GetNonCompositeCollider(gameObject);
    }
    private void LateUpdate()
    {
        Vector2 currentPos = transform.position;
        if (Vector2.Distance(currentPos, lastMovedPosition) > 0.01f) // Threshold to avoid micro-moves
        {
            EntityCounter.Instance.UpdateEntityPosition(gameObject);
            lastMovedPosition = currentPos;
        }
        calculatedAcc = false;
    }
    private void Update()
    {
        deltaTimes.Add(Time.deltaTime);
        if (saveStates > 0 && deltaTimes.Count > saveStates)
        {
            deltaTimes.RemoveAt(0);
        }

        Vector2 currentPosition = transform.position;

        // Record position
        positions.Add(currentPosition);
        if (saveStates > 0 && positions.Count > saveStates)
        {
            positions.RemoveAt(0);
        }

        Vector2 currentVelocity = (currentPosition - previousPosition) / Time.deltaTime;
        velocities.Add(currentVelocity);
        if (saveStates > 0 && velocities.Count > saveStates)
        {
            velocities.RemoveAt(0);
        }

        previousVelocity = currentVelocity;
        previousPosition = currentPosition;
    }
    public TrajectoryInstance Copy()
    {
        return new TrajectoryInstance(this);
    }

    public TrajectoryInstance GetShifted(Vector2 deltaPos)
    {
        TrajectoryInstance shifted = new TrajectoryInstance(this);
        shifted.currentPosition += deltaPos;
        shifted.previousPosition += deltaPos;
        return shifted;
    }
    public Vector2 CalculateAcceleration()
    {
        if (calculatedAcc)
        {
            return cachedAcceleration;
        }
        else
        {
            // Calculate acceleration using the last 10 velocities
            calculatedAcc = true;
            cachedAcceleration = MathUtils.CalculateAverageAcceleration(velocities, deltaTimes, Mathf.Max(velocities.Count - 10, 0), velocities.Count);
            return cachedAcceleration;
        }
    }
    public List<Vector2> GetPositions()
    {
        return new List<Vector2>(positions);
    }
    public Vector2 GetCurrentPosition()
    {
        return positions.Count > 0 ? positions[positions.Count - 1] : Vector2.zero;
    }
    public Vector2 GetVelocity()
    {
        return previousVelocity;
    }
    public Vector2 ExtrapolateFutureVelocity(float deltaTime)
    {
        return previousVelocity + CurrentAcceleration * deltaTime;
    }
    public Vector2 ExtrapolateFuturePosition(float deltaTime)
    {
        return previousPosition + previousVelocity * deltaTime;// + (currentAcceleration * deltaTime * deltaTime / 2);
    }
    public CollisionInfo? WillObjectsCollide(Trajectory other, float maxTime)
    {
        return WillObjectsCollide(Copy(), other.Copy(), maxTime);
    }
    public CollisionInfo? WillObjectsCollide(TrajectoryInstance other, float maxTime)
    {
        return WillObjectsCollide(Copy(), other, maxTime);
    }
    /// <summary>
    /// Predicts if and when the trajectory of this object will cross with another Trajectory object's trajectory within a given maximum time.
    /// Uses an approximation based on the time of closest approach for constant relative velocity, then checks if positions coincide.
    /// </summary>
    /// <returns>A CrossingInfo struct with the time and position of crossing if they cross within maxTime, otherwise null.</returns>
    public static CollisionInfo? WillObjectsCollide(TrajectoryInstance trajectory, TrajectoryInstance other, float maxTime)
    {
        // Get current state of both trajectories
        Vector2 pA = trajectory.currentPosition;
        Vector2 vA = trajectory.previousVelocity;
        //Vector2 aA = CurrentAcceleration;
        Vector2 pB = other.currentPosition;
        Vector2 vB = other.previousVelocity;
        //Vector2 aB = other.currentAcceleration;

        if (vA.magnitude == 0 && vB.magnitude == 0) return null;

        if (Mathf.Approximately(vB.magnitude, 0)) return WillCollideWithWall(trajectory, vA, other, maxTime, true);
        if (Mathf.Approximately(vA.magnitude, 0)) return WillCollideWithWall(other, vB, trajectory, maxTime, false);

        // Calculate moving collider collision
        return WillMovingObjectsCollide(pA, vA, trajectory.MyCollider, pB, vB, other.MyCollider, maxTime);
    }
    private static CollisionInfo? WillCollideWithWall(TrajectoryInstance movingTrajectory, Vector2 velocity, TrajectoryInstance staticTrajectory, float maxTime, bool isMyColliderMoving)
    {
        Collider2D movingCollider2D = movingTrajectory.MyCollider;
        Collider2D staticCollider2D = staticTrajectory.MyCollider;
        GeometryUtils.ColliderPoints? colliderPoints = GeometryUtils.CalculateColliderCrossection(movingCollider2D, velocity, true);
        if (!colliderPoints.HasValue) return null;
        Vector2 frontMovingPoint = GeometryUtils.GetForwardmostPoint(colliderPoints.Value.points, velocity);
        RaycastHit2D[] hits = GeometryUtils.RaycastInDir(frontMovingPoint, velocity);

        RaycastHit2D? wallHit = null;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == staticCollider2D) wallHit = hit;
        }
        if (!wallHit.HasValue) return null;

        Line2D wallLine = GeometryUtils.GetHitWallDirection(wallHit.Value, staticCollider2D, velocity);

        float distanceToHit = Vector2.Distance(frontMovingPoint, wallHit.Value.point);
        float timeToHit = distanceToHit / velocity.magnitude;
        if (timeToHit < 0 || timeToHit > maxTime) return null;

        Vector2 vA = isMyColliderMoving ? movingTrajectory.previousVelocity : staticTrajectory.previousVelocity;
        Vector2 vB = isMyColliderMoving ? staticTrajectory.previousVelocity : movingTrajectory.previousVelocity;
        return new CollisionInfo { time = timeToHit, collisionPosition = wallHit.Value.point, collisionSpeed = velocity.magnitude, myPoint = frontMovingPoint, otherPoint = wallHit.Value.point, myVelocity=vA, otherVelocity= wallLine.direction, otherIsWall=isMyColliderMoving};
    }
    private static CollisionInfo? WillMovingObjectsCollide(Vector2 pA, Vector2 vA, Collider2D myCollider2D, Vector2 pB, Vector2 vB, Collider2D otherCollider2D, float maxTime)
    {
        // Determinant for 2D cross product
        float D = vA.x * vB.y - vB.x * vA.y;
        if (Mathf.Approximately(D, 0f))
        {
            // Lines are parallel, no intersection
            return null;
        }

        GeometryUtils.ColliderPoints? colliderCrossectionA = GeometryUtils.CalculateColliderCrossection(myCollider2D, vA, true);
        if (!colliderCrossectionA.HasValue) return null;
        GeometryUtils.ColliderPoints? colliderCrossectionB = GeometryUtils.CalculateColliderCrossection(otherCollider2D, vB, true);
        if (!colliderCrossectionB.HasValue) return null;

        List<GeometryUtils.LineCrossingInfo> crossings = new List<GeometryUtils.LineCrossingInfo>();

        // For each point in A, check if it passed the crossings before (or after) all points in B
        foreach (Vector2 point in colliderCrossectionA.Value.points)
        {
            GeometryUtils.LineCrossingInfo? lineCrossing = CollidesWithCrossection(point, vA, colliderCrossectionB.Value, vB, D, maxTime);
            if (lineCrossing.HasValue) crossings.Add(lineCrossing.Value);
        }
        GeometryUtils.LineCrossingInfo? info = FindFirstCrossing(crossings);

        if (info == null) return null;
        //Debug.DrawLine(info.Value.crossPoint, info.Value.pointA, Color.red);

        float crossTime = Mathf.Max(info.Value.crossTimeA, info.Value.crossTimeB);
        Vector2 relativeVelocity = vA - vB;
        return new CollisionInfo { collisionPosition = info.Value.crossPoint, time = crossTime, collisionSpeed = relativeVelocity.magnitude, myPoint = info.Value.pointA, otherPoint = info.Value.pointB, myVelocity = vA, otherVelocity = vB, otherIsWall = false };
    }
    private static GeometryUtils.LineCrossingInfo? CollidesWithCrossection(Vector2 pointA, Vector2 vA, GeometryUtils.ColliderPoints crossection, Vector2 vB, float D, float maxTime)
    {
        List<bool> comparisons = new List<bool>();
        List<GeometryUtils.LineCrossingInfo> infos = new List<GeometryUtils.LineCrossingInfo>();

        foreach (Vector2 pointB in crossection.points)
        {
            GeometryUtils.LineCrossingInfo? lineCrossing = CalculateEfficientLineCrossing(pointA, vA, pointB, vB, D, maxTime);
            if (lineCrossing.HasValue)
            {
                comparisons.Add(lineCrossing.Value.crossTimeA < lineCrossing.Value.crossTimeB);
                infos.Add(lineCrossing.Value);
            }
        }

        // Return true if all values in the list are the same
        if (comparisons.Count == 0) return null;
        bool firstValue = comparisons[0];
        bool crosses = false;
        for (int i = 1; i < comparisons.Count; i++)
        {
            if (comparisons[i] != firstValue) crosses = true;
        }

        if (crosses)
        {
            // Find crossing with the smallest time
            return FindFirstCrossing(infos);
        }
        else
        {
            return null;
        }
    }
    private static GeometryUtils.LineCrossingInfo? FindFirstCrossing(List<GeometryUtils.LineCrossingInfo> infos)
    {
        float soonestCrossTime = float.MaxValue;
        GeometryUtils.LineCrossingInfo? soonestCrossing = null;
        for (int i = 0; i < infos.Count; i++)
        {
            float crossTime = Mathf.Max(infos[i].crossTimeA, infos[i].crossTimeB);
            if (crossTime < soonestCrossTime)
            {
                soonestCrossTime = infos[i].crossTimeA;
                soonestCrossing = infos[i];
            }
        }
        return soonestCrossing;
    }
    private static GeometryUtils.LineCrossingInfo? CalculateEfficientLineCrossing(Vector2 startA, Vector2 velocityA, Vector2 startB, Vector2 velocityB, float D, float maxTime = float.MaxValue)
    {
        // Find intersection of the lines: startA + velocityA * t and startB + velocityB * s
        Vector2 delta = startB - startA;

        // Solve for t and s
        float tA = (delta.x * velocityB.y - velocityB.x * delta.y) / D;
        float tB = (delta.x * velocityA.y - velocityA.x * delta.y) / D;

        // Check if intersection is in the future (t > 0 and s > 0)
        if (tA <= 0 || tB <= 0)
        {
            return null;
        }

        // Check if at least one time is within maxTime
        if (tA > maxTime && tB > maxTime)
        {
            return null;
        }

        // Calculate the crossing point
        Vector2 crossPoint = startA + velocityA * tA;

        return new GeometryUtils.LineCrossingInfo { crossPoint = crossPoint, crossTimeA = tA, crossTimeB = tB, pointA=startA, pointB=startB };
    }
}
