using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ISettableTarget;

public class Rotator : MonoBehaviour, ISettableTarget
{
    [Header("Rotator settings")]
    [SerializeField] private float maxRotationSpeed = 180f; // degrees per second
    [SerializeField] private bool useCone = true;
    [SerializeField] private float coneAngle = 90f; // total angle of the cone in degrees
    [SerializeField][Tooltip("If false, the FieldOfView cone will never become visible")] private bool displayCone;
    [SerializeField][Range(1f,20f)] private float fovConeRadius = 10f;

    [Header("Instances")]
    [SerializeField] Transform rotator;
    [Header("Debug")]
    [SerializeField] bool debug = false;

    private TargetInstance? currentTarget;
    private Cache<Cone> rotationCone;
    private ProgressBar fovConeScript;

    #region Public Methods
    #region ISettableTarget Methods
    public void SetTarget(GameObject target)
    {
        currentTarget = new TargetInstance(target);
    }

    public void SetTarget(Vector2 position)
    {
        currentTarget = new TargetInstance(position);
    }
    public void StopTargetting()
    {
        currentTarget = null;
    }
    public TargetInstance? GetTarget()
    {
        return currentTarget;
    }
    #endregion
    public Vector2 GetCurrentDirection()
    {
        return GeometryUtils.AngleToDirectionVector(rotator.rotation.eulerAngles.z);
    }
    public void SetDefaultRotation()
    {
        currentTarget = new TargetInstance(GetDefaultRotationPosition());
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
    #endregion

    private Vector2 GetDefaultRotationPosition()
    {
        // Calculate a position that would make the rotator face its default rotation (0 degrees)
        return rotator.position + transform.right; // 10 units in the default direction
    }

    private void Start()
    {
        rotationCone = CacheManager.Instance.CreateCache<Cone>(CacheBehavior.EndOfUpdate);
        if (displayCone)
        {
            fovConeScript = UIManager.Instance.InstantiateRotationCone(gameObject, fovConeRadius, coneAngle, coneAngle / 2, true);
            fovConeScript.ShowBar();
        }
    }
    private void Update()
    {
        RotateTowardsTarget();
        //GetRotationCone().DebugDisplayCone(Color.gray);
    }

    private void RotateTowardsTarget()
    {
        if(!currentTarget.HasValue) return;
        Vector2 direction = currentTarget.Value.GetPosition() - (Vector2)rotator.position;

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
