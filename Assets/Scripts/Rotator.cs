using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 180f; // degrees per second
    [SerializeField] private bool useCone = true;
    [SerializeField] private float coneAngle = 90f; // total angle of the cone in degrees
    [Header("Instances")]
    [SerializeField] Transform rotator;
    [Header("Debug")]
    [SerializeField] bool debug = false;

    private GameObject targetObject;
    private Vector2 targetPosition;
    private bool usePosition = false;
    public Vector2 GetCurrentDirection()
    {
        return GeometryUtils.AngleToDirectionVector(rotator.rotation.eulerAngles.z);
    }
    public void SetTarget(GameObject obj)
    {
        targetObject = obj;
        usePosition = false;
    }

    public void SetTarget(Vector2 position)
    {
        targetPosition = position;
        usePosition = true;
    }

    public void StopTargeting()
    {
        targetObject = null;
        usePosition = false;
    }

    private void Update()
    {
        RotateTowardsTarget();
    }

    private void RotateTowardsTarget()
    {
        Vector2 direction;
        if (usePosition)
        {
            direction = targetPosition - (Vector2)rotator.position;
        }
        else if (targetObject != null)
        {
            direction = (Vector2)targetObject.transform.position - (Vector2)rotator.position;
            if (debug) Debug.DrawLine(rotator.position, targetObject.transform.position, Color.red);
        }
        else
        {
            return; // No target set
        }

        if (direction != Vector2.zero)
        {
            float relativeTargetAngle = CalculateRelativeTargetAngle(direction);
            if (useCone)
            {
                relativeTargetAngle = ClampAngleToCone(relativeTargetAngle, coneAngle / 2f);
            }
            if(debug) Debug.DrawRay(rotator.position, GeometryUtils.AngleToDirectionVector(relativeTargetAngle + transform.rotation.eulerAngles.z), Color.blue);
            float currentRelativeAngle = GeometryUtils.ClampAngle180(rotator.localRotation.eulerAngles.z);
            float newLocalAngle = Mathf.MoveTowards(currentRelativeAngle, relativeTargetAngle, maxRotationSpeed * Time.deltaTime);
            rotator.localRotation = Quaternion.Euler(0f, 0f, newLocalAngle);
        }
    }
    private float CalculateRelativeTargetAngle(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float myAngle = GeometryUtils.ClampAngle180(transform.rotation.eulerAngles.z);
        return GeometryUtils.ClampAngle180(targetAngle - myAngle);
    }
    private float ClampAngleToCone(float relativeTargetAngle, float halfAngle)
    {
        if (Mathf.Abs(relativeTargetAngle) > halfAngle)
        {
            return Mathf.Sign(relativeTargetAngle) * halfAngle;
        }
        else
        {
            return relativeTargetAngle;
        }
    }
}
