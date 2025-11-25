using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Trajectory : MonoBehaviour
{
    [SerializeField] [Tooltip("Save a path of n positions. -1 for unlimited")] 
    private int maxPositions = 100;


    private List<Vector2> positions = new List<Vector2>();
    private Vector2 previousVelocity = Vector2.zero;
    private Vector2 currentAcceleration = Vector2.zero;
    private Vector2 previousPosition;

    // This position is used to track movement for EntityCounter updates, so it measures distance, not velocity
    private Vector2 lastMovedPosition = Vector2.zero;

    public struct CrossingInfo
    {
        public float time;
        public Vector2 position;
    }

    public class TrajectoryInstance
    {
        public TrajectoryInstance(Trajectory trajectory)
        {
            //positions = new List<Vector2>(trajectory.positions);
            positions = new List<Vector2>();
            previousVelocity = trajectory.previousVelocity;
            currentAcceleration = trajectory.currentAcceleration;
            previousPosition = trajectory.previousPosition;
            currentPosition = trajectory.GetCurrentPosition();
        }

        public List<Vector2> positions;
        public Vector2 previousVelocity;
        public Vector2 currentAcceleration;
        public Vector2 previousPosition;
        public Vector2 currentPosition;
        public Vector2 lastMovedPosition;

        public Vector2 ExtrapolateFutureVelocity(float deltaTime)
        {
            return previousVelocity + currentAcceleration * deltaTime;
        }

        public Vector2 ExtrapolateFuturePosition(float deltaTime)
        {
            return previousPosition + previousVelocity * deltaTime + (currentAcceleration * deltaTime * deltaTime / 2);
        }

        public CrossingInfo? TrajectoryCrossing(Trajectory other, float maxTime)
        {
            return TrajectoryCrossing(other.Copy(), maxTime);
        }
        /// <summary>
        /// Predicts if and when the trajectory of this object will cross with another Trajectory object's trajectory within a given maximum time.
        /// Uses an approximation based on the time of closest approach for constant relative velocity, then checks if positions coincide.
        /// </summary>
        /// <param name="other">The other Trajectory object to check for crossing.</param>
        /// <param name="maxTime">The maximum time in seconds to look ahead for a crossing.</param>
        /// <returns>A CrossingInfo struct with the time and position of crossing if they cross within maxTime, otherwise null.</returns>
        public CrossingInfo? TrajectoryCrossing(TrajectoryInstance other, float maxTime)
        {
            // Get current state of both trajectories
            Vector2 p1 = currentPosition;
            Vector2 v1 = previousVelocity;
            Vector2 a1 = currentAcceleration;
            Vector2 p2 = other.currentPosition;
            Vector2 v2 = other.previousVelocity;
            Vector2 a2 = other.currentAcceleration;

            // Calculate relative position and velocity
            Vector2 deltaPos = p1 - p2;
            Vector2 deltaV = v1 - v2;
            float dvMagSq = deltaV.sqrMagnitude;

            // If relative velocity is zero, check if they are already at the same position
            if (dvMagSq == 0)
            {
                if (deltaPos == Vector2.zero)
                {
                    return new CrossingInfo { time = 0, position = p1 };
                }
                else
                {
                    return null;
                }
            }

            // Calculate time of closest approach using dot product formula
            float t = -Vector2.Dot(deltaPos, deltaV) / dvMagSq;

            // If time is negative or exceeds maxTime, no crossing within bounds
            if (t < 0 || t > maxTime)
            {
                return null;
            }

            // Extrapolate positions at time t using kinematic equations
            Vector2 posAtT = p1 + v1 * t + (a1 * t * t) / 2;
            Vector2 pos2AtT = p2 + v2 * t + (a2 * t * t) / 2;
            //Debug.DrawLine(posAtT, pos2AtT, Color.magenta);

            // Check if positions are close enough to consider a crossing
            if (Vector2.Distance(posAtT, pos2AtT) < 0.1f)
            {
                return new CrossingInfo { time = t, position = posAtT };
            }
            else
            {
                return null;
            }
        }
    }

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector2 currentPos = transform.position;
        if (Vector2.Distance(currentPos, lastMovedPosition) > 0.01f) // Threshold to avoid micro-moves
        {
            EntityCounter.Instance.UpdateEntityPosition(gameObject);
            lastMovedPosition = currentPos;
        }
    }

    private void Update()
    {
        Vector2 currentPosition = transform.position;

        // Record position
        positions.Add(currentPosition);
        if (maxPositions > 0 && positions.Count > maxPositions)
        {
            positions.RemoveAt(0);
        }

        Vector2 currentVelocity;
        currentVelocity = (currentPosition - previousPosition) / Time.deltaTime;
        currentAcceleration = (currentVelocity - previousVelocity) / Time.deltaTime;
        
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

    public Vector2 GetAcceleration()
    {
        return currentAcceleration;
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
        return previousVelocity + currentAcceleration * deltaTime;
    }

    public Vector2 ExtrapolateFuturePosition(float deltaTime)
    {
        return previousPosition + previousVelocity * deltaTime + (currentAcceleration * deltaTime * deltaTime / 2);
    }

    public CrossingInfo? TrajectoryCrossing(Trajectory other, float maxTime)
    {
        return TrajectoryCrossing(other.Copy(), maxTime);
    }
    /// <summary>
    /// Predicts if and when the trajectory of this object will cross with another Trajectory object's trajectory within a given maximum time.
    /// Uses an approximation based on the time of closest approach for constant relative velocity, then checks if positions coincide.
    /// </summary>
    /// <param name="other">The other Trajectory object to check for crossing.</param>
    /// <param name="maxTime">The maximum time in seconds to look ahead for a crossing.</param>
    /// <returns>A CrossingInfo struct with the time and position of crossing if they cross within maxTime, otherwise null.</returns>
    public CrossingInfo? TrajectoryCrossing(TrajectoryInstance other, float maxTime)
    {
        // Get current state of both trajectories
        Vector2 p1 = GetCurrentPosition();
        Vector2 v1 = GetVelocity();
        Vector2 a1 = GetAcceleration();
        Vector2 p2 = other.currentPosition;
        Vector2 v2 = other.previousVelocity;
        Vector2 a2 = other.currentAcceleration;

        // Calculate relative position and velocity
        Vector2 deltaPos = p1 - p2;
        Vector2 deltaV = v1 - v2;
        float dvMagSq = deltaV.sqrMagnitude;

        // If relative velocity is zero, check if they are already at the same position
        if (dvMagSq == 0)
        {
            if (deltaPos == Vector2.zero)
            {
                return new CrossingInfo { time = 0, position = p1 };
            }
            else
            {
                return null;
            }
        }

        // Calculate time of closest approach using dot product formula
        float t = -Vector2.Dot(deltaPos, deltaV) / dvMagSq;

        // If time is negative or exceeds maxTime, no crossing within bounds
        if (t < 0 || t > maxTime)
        {
            return null;
        }

        // Extrapolate positions at time t using kinematic equations
        Vector2 posAtT = p1 + v1 * t + (a1 * t * t) / 2;
        Vector2 pos2AtT = p2 + v2 * t + (a2 * t * t) / 2;

        // Check if positions are close enough to consider a crossing
        if (Vector2.Distance(posAtT, pos2AtT) < 0.01f)
        {
            return new CrossingInfo { time = t, position = posAtT };
        }
        else
        {
            return null;
        }
    }
}
