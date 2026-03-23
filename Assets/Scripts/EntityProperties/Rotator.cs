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
    private Cache<Cone> rotationCone;
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

    public void SetDefaultRotation()
    {
        targetPosition = GetDefaultRotationPosition();
        usePosition = true;
    }
    private Vector2 GetDefaultRotationPosition()
    {
        // Calculate a position that would make the rotator face its default rotation (0 degrees)
        return rotator.position + transform.right; // 10 units in the default direction
    }

    public void StopTargeting()
    {
        targetObject = null;
        usePosition = false;
    }
    private void Start()
    {
        rotationCone = CacheManager.Instance.CreateCache<Cone>(CacheBehavior.EndOfUpdate);
    }
    private void Update()
    {
        RotateTowardsTarget();
        //GetRotationCone().DebugDisplayCone(Color.gray);
    }
    public Cone GetRotationCone()
    {
        if(!rotationCone.isCached) rotationCone.Set(
            new Cone(
                rotator.position,
                GeometryUtils.AngleToDirectionVector(transform.rotation.eulerAngles.z),
                coneAngle,
                Mathf.Infinity));
        return rotationCone.Get();
    }

    private void RotateTowardsTarget()
    {
        Vector2 direction = Vector2.zero;
        if (usePosition)
        {
            direction = targetPosition - (Vector2)rotator.position;
        }
        else if (targetObject != null)
        {
            direction = (Vector2)targetObject.transform.position - (Vector2)rotator.position;
        }
        else
        {
            return; // No target set
        }

        Debug.DrawRay(rotator.position, direction, Color.red);
        if (direction != Vector2.zero)
        {
            Cone myCone = GetRotationCone();
            float relativeTargetAngle = myCone.CalculateRelativePositionAngle(direction);
            if (useCone)
            {
                relativeTargetAngle = myCone.ClampAngleToCone(relativeTargetAngle);
            }
            if(debug) Debug.DrawRay(rotator.position, GeometryUtils.AngleToDirectionVector(relativeTargetAngle + transform.rotation.eulerAngles.z), Color.blue);
            float currentRelativeAngle = GeometryUtils.ClampAngle180(rotator.localRotation.eulerAngles.z);
            float newLocalAngle = Mathf.MoveTowards(currentRelativeAngle, relativeTargetAngle, maxRotationSpeed * Time.deltaTime);
            rotator.localRotation = Quaternion.Euler(0f, 0f, newLocalAngle);
        }
    }
}
