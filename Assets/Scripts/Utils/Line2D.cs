using UnityEngine;

public class Line2D
{
    public Vector2 start;
    public Vector2 direction;
    public Vector2 end;

    public Line2D(Vector2 point, Vector2 direction)
    {
        this.start = point;
        this.direction = direction;
        this.end = point + direction;
    }

    public override string ToString()
    {
        return $"Line2D(from=({start.x:F2};{start.y:F2}), dir=({direction.x:F2};{direction.y:F2}))";
    }

    public Line2D Copy()
    {
        return new Line2D(start, direction);
    }

    public Line2D SetStart(Vector2 newStart)
    {
        start = newStart;
        end = newStart + direction;
        return this;
    }

    public Line2D Translated(Vector2 translation)
    {
        Vector2 newStart = start + translation;
        return new Line2D(newStart, direction);
    }

    public Line2D Translate(Vector2 translation)
    {
        start += translation;
        end = start + direction;
        return this;
    }

    public Line2D Inverted()
    {
        return new Line2D(start, -direction);
    }

    public Line2D Invert()
    {
        direction = -direction;
        end = start + direction;
        return this;
    }

    public Line2D Scaled(float factor)
    {
        Vector2 newDirection = direction * factor;
        return new Line2D(start, newDirection);
    }

    public Line2D Scale(float factor)
    {
        direction *= factor;
        end = start + direction;
        return this;
    }

    public float Length()
    {
        return direction.magnitude;
    }

    public Line2D Normalized()
    {
        float length = Length();
        if (length == 0)
            return new Line2D(start, Vector2.zero);
        Vector2 newDirection = direction.normalized;
        return new Line2D(start, newDirection);
    }

    public Line2D Normalize()
    {
        float length = Length();
        if (length == 0)
            direction = Vector2.zero;
        else
            direction = direction.normalized;
        end = start + direction;
        return this;
    }

    public Vector2 ProjectOntoVector(Vector2 onto)
    {
        float dotProduct = Vector2.Dot(this.direction, onto);
        float ontoLengthSquared = onto.sqrMagnitude;
        if (ontoLengthSquared == 0)
            return Vector2.zero;
        float projectionScale = dotProduct / ontoLengthSquared;
        return onto * projectionScale;
    }

    public Line2D ProjectOntoLine(Line2D onto)
    {
        Vector2 projectedDirection = ProjectOntoVector(onto.direction);
        return new Line2D(start, projectedDirection);
    }

    public Line2D PerpendicularToLine(Line2D other)
    {
        Vector2 perpDirection = new Vector2(direction.y, -direction.x);
        return new Line2D(start, perpDirection);
    }

    public Line2D Perpendicular1()
    {
        return new Line2D(start, new Vector2(direction.y, -direction.x));
    }

    public Line2D Perpendicular2()
    {
        return new Line2D(start, new Vector2(-direction.y, direction.x));
    }

    public Vector2 ClosestPointOnLine(Vector2 point)
    {
        Vector2 lineToPoint = point - start;
        float projectionLength = Vector2.Dot(lineToPoint, direction) / direction.sqrMagnitude;
        return start + projectionLength * direction;
    }

    public float AngleBetween(Line2D other)
    {
        float dotProduct = Vector2.Dot(direction, other.direction);
        float lenSelf = Length();
        float lenOther = other.Length();
        if (lenSelf == 0 || lenOther == 0)
            return 0;
        float cosAngle = dotProduct / (lenSelf * lenOther);
        cosAngle = Mathf.Clamp(cosAngle, -1, 1);
        return Mathf.Acos(cosAngle);
    }

    public float AngleFromTo(Line2D other)
    {
        float angle = AngleBetween(other);
        float crossProduct = direction.x * other.direction.y - direction.y * other.direction.x;
        if (crossProduct < 0)
            angle = -angle;
        return angle;
    }

    public Line2D CalculateThreatLine(Line2D other)
    {
        Vector2 closestPoint = other.ClosestPointOnLine(start);
        Line2D perpendicularLine = ProjectOntoLine(other.Perpendicular1());

        // Maybe possible to simplify the function by using Vector2 in the calculation
        Line2D candidate1 = perpendicularLine.Copy().SetStart(closestPoint);
        float distanceToLine1 = Vector2.Distance(candidate1.end, start);
        Line2D candidate2 = candidate1.Inverted();
        float distanceToLine2 = Vector2.Distance(candidate2.end, start);

        bool angleOver90 = AngleBetween(other) / Mathf.PI > 0.5f;

        Line2D threatLine;
        if (angleOver90)
        {
            threatLine = AngleFromTo(candidate1) / Mathf.PI < 0 ? candidate1 : candidate2;
        }
        else
        {
            threatLine = distanceToLine1 < distanceToLine2 ? candidate1 : candidate2;
        }

        float distanceClosestPointToStart = Vector2.Distance(start, closestPoint);
        return threatLine.Normalized().Scale(distanceClosestPointToStart);
    }
}