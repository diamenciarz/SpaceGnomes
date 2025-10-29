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
    private bool hasPreviousVelocity = false;
    private Vector2 previousPosition;
    private bool hasPreviousPosition = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("Trajectory requires a Rigidbody2D component.");
            enabled = false;
            return;
        }
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        // Your movement code here, e.g.:
        // transform.position += velocity * Time.deltaTime;

        Vector2 currentPos = transform.position;
        if (Vector2.Distance(currentPos, previousPosition) > 0.05f) // Threshold to avoid micro-moves
        {
            EntityCounter.Instance.UpdateEntityPosition(gameObject);
            previousPosition = currentPos;
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

        // Estimate velocity from position change
        Vector2 currentVelocity;
        if (hasPreviousPosition)
        {
            currentVelocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;
        }
        else
        {
            currentVelocity = rb != null && !rb.isKinematic ? rb.velocity : Vector2.zero;
            hasPreviousPosition = true;
        }

        // Calculate acceleration from velocity change
        if (hasPreviousVelocity)
        {
            currentAcceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
        }
        else
        {
            currentAcceleration = Vector2.zero;
            hasPreviousVelocity = true;
        }

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
