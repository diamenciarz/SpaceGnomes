using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPositionTracker : MonoBehaviour
{
    private Vector2 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }
    private void LateUpdate()
    {
        // Your movement code here, e.g.:
        // transform.position += velocity * Time.deltaTime;

        Vector2 currentPos = transform.position;
        if (Vector2.Distance(currentPos, lastPosition) > 0.05f) // Threshold to avoid micro-moves
        {
            EntityCounter.Instance.UpdateEntityPosition(gameObject);
            lastPosition = currentPos;
        }
    }
}
