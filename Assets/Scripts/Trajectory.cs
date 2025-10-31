using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void FixedUpdate()
    {
        Vector2 currentPosition = transform.position;

        // Record position
        positions.Add(currentPosition);
        if (maxPositions > 0 && positions.Count > maxPositions)
        {
            positions.RemoveAt(0);
        }

        Vector2 currentVelocity;
        currentVelocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;
        currentAcceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
        
        previousVelocity = currentVelocity;
        previousPosition = currentPosition;
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
}
