using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EntitySelectionArea : MonoBehaviour
{
    Collider2D myCollider;
    public bool debug = false;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GeometryUtils.GetNonCompositeCollider(gameObject);
        UnitSelectionManager.Instance.RegisterSelectableEntity(this);
    }

    void Update()
    {
        if (debug)
        {
            DrawSelectionArea();
        }
    }

    public bool RectangleTouchesSelectionArea(Rect rect)
    {
        GeometryUtils.ColliderPoints? crossection = GeometryUtils.CalculateColliderCrossection(myCollider, transform.up);
        if (!crossection.HasValue) return false;

        Vector2[] points = crossection.Value.points;

        Vector2 dir = transform.up.normalized;
        Vector2 perp = Vector2.Perpendicular(dir);

        float dotFront = Vector2.Dot(points[0], dir);
        float dotBack = Vector2.Dot(points[1], dir);
        float dotLeft = Vector2.Dot(points[2], perp);
        float dotRight = Vector2.Dot(points[3], perp);

        float minDir = Mathf.Min(dotFront, dotBack);
        float maxDir = Mathf.Max(dotFront, dotBack);
        float minPerp = Mathf.Min(dotLeft, dotRight);
        float maxPerp = Mathf.Max(dotLeft, dotRight);

        Vector2[] selectionCorners = new Vector2[4];
        selectionCorners[0] = maxDir * dir + minPerp * perp; // frontLeft
        selectionCorners[1] = maxDir * dir + maxPerp * perp; // frontRight
        selectionCorners[2] = minDir * dir + maxPerp * perp; // backRight
        selectionCorners[3] = minDir * dir + minPerp * perp; // backLeft

        Vector2[] rectCorners = new Vector2[4];
        rectCorners[0] = new Vector2(rect.xMin, rect.yMin);
        rectCorners[1] = new Vector2(rect.xMax, rect.yMin);
        rectCorners[2] = new Vector2(rect.xMax, rect.yMax);
        rectCorners[3] = new Vector2(rect.xMin, rect.yMax);

        Vector2[] axes = new Vector2[4] { Vector2.right, Vector2.up, dir, perp };

        // Check separating axis theorem
        foreach (Vector2 axis in axes)
        {
            float min1 = float.MaxValue, max1 = float.MinValue;
            foreach (Vector2 p in rectCorners)
            {
                float proj = Vector2.Dot(p, axis);
                min1 = Mathf.Min(min1, proj);
                max1 = Mathf.Max(max1, proj);
            }
            float min2 = float.MaxValue, max2 = float.MinValue;
            foreach (Vector2 p in selectionCorners)
            {
                float proj = Vector2.Dot(p, axis);
                min2 = Mathf.Min(min2, proj);
                max2 = Mathf.Max(max2, proj);
            }
            if (max1 < min2 || max2 < min1)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsPointInSelectionArea(Vector2 point)
    {
        GeometryUtils.ColliderPoints? crossection = GeometryUtils.CalculateColliderCrossection(myCollider, transform.up);

        if (!crossection.HasValue)
        {
            return false;
        }

        Vector2[] points = crossection.Value.points;
        if (points.Length < 4)
        {
            return false;
        }

        Vector2 dir = transform.up.normalized;
        Vector2 perp = Vector2.Perpendicular(dir);

        // Assuming points[0] is front, points[1] is back, points[2] is left, points[3] is right
        float dotFront = Vector2.Dot(points[0], dir);
        float dotBack = Vector2.Dot(points[1], dir);
        float dotLeft = Vector2.Dot(points[2], perp);
        float dotRight = Vector2.Dot(points[3], perp);

        float minDir = Mathf.Min(dotFront, dotBack);
        float maxDir = Mathf.Max(dotFront, dotBack);
        float minPerp = Mathf.Min(dotLeft, dotRight);
        float maxPerp = Mathf.Max(dotLeft, dotRight);

        float pointDir = Vector2.Dot(point, dir);
        float pointPerp = Vector2.Dot(point, perp);

        return pointDir >= minDir && pointDir <= maxDir && pointPerp >= minPerp && pointPerp <= maxPerp;
    }

    private void DrawSelectionArea()
    {
        GeometryUtils.ColliderPoints? crossection = GeometryUtils.CalculateColliderCrossection(myCollider, transform.up);

        if (!crossection.HasValue)
        {
            return;
        }

        Vector2[] points = crossection.Value.points;
        if (points.Length < 4)
        {
            return;
        }

        Vector2 dir = transform.up.normalized;
        Vector2 perp = Vector2.Perpendicular(dir);

        float dotFront = Vector2.Dot(points[0], dir);
        float dotBack = Vector2.Dot(points[1], dir);
        float dotLeft = Vector2.Dot(points[2], perp);
        float dotRight = Vector2.Dot(points[3], perp);

        float minDir = Mathf.Min(dotFront, dotBack);
        float maxDir = Mathf.Max(dotFront, dotBack);
        float minPerp = Mathf.Min(dotLeft, dotRight);
        float maxPerp = Mathf.Max(dotLeft, dotRight);

        // Calculate corners
        Vector2 frontLeft = maxDir * dir + minPerp * perp;
        Vector2 frontRight = maxDir * dir + maxPerp * perp;
        Vector2 backLeft = minDir * dir + minPerp * perp;
        Vector2 backRight = minDir * dir + maxPerp * perp;

        // Draw lines
        Debug.DrawLine(frontLeft, frontRight, Color.red);
        Debug.DrawLine(frontRight, backRight, Color.red);
        Debug.DrawLine(backRight, backLeft, Color.red);
        Debug.DrawLine(backLeft, frontLeft, Color.red);
    }
}
