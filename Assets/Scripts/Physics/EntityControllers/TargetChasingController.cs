using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TargetChasingController : AutonomousMovementController, ISettableTarget
// Consider making this into a generic forward movement controller
{
    [Header("Acceleration Settings")]
    [SerializeField] private float initialVelocity = 10f;
    [SerializeField] private float targetVelocity = 20f;
    [SerializeField] private float accelerationTime = 2f; // Time to reach target velocity
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1); // Curve for acceleration
    [SerializeField] private float accelerationDelay = 0.5f; // Delay before starting acceleration

    [Header("Rotation Settings")]
    [SerializeField] private float steerTorque = 10000f;

    [Header("Physics Settings")]
    [SerializeField][Range(0f, 360)] float maxAngularVelocity = 100f;

    [Header("Instance Settings")]
    [SerializeField] private List<AbstractSensor> sensors = new List<AbstractSensor>();

    public new Func<float, float> VelocityFunction => GetVelocityAtTime;

    private Rigidbody2D rb;
    private GameObject target;
    private float lastFixedVelocity;
    private float lastFixedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Ensure kinematic for manual velocity control
    }
    private void FixedUpdate()
    {
        HandleMovement();
    }
    private void Update()
    {
        UpdateTarget();
        Debug.DrawLine(transform.position, target.transform.position, Color.red);
    }
    private void UpdateTarget()
    {
        if (target == null) return;
        Cone myDirectionCone = new Cone(transform.position, transform.up, 360f, Mathf.Infinity);
        List<GameObject> detectedObjects = GetDetectedObjects();
        target = myDirectionCone.GetClosestObjectInCone(detectedObjects, Cone.ConeDistance.SmallestAngle);
    }
    private List<GameObject> GetDetectedObjects()
    {
        List<GameObject> detectedObjects = new List<GameObject>();
        foreach (AbstractSensor sensor in sensors)
        {
            detectedObjects.AddRange(sensor.GetVisibleEnemies());
            // Add handling for other sensor types as needed
        }
        return detectedObjects.Distinct().ToList();
    }
    public void SetTarget(GameObject target)
    {
        this.target = target;
    }
    public override void Activate()
    {
        base.Activate();
        // Set initial velocity along forward direction (transform.up)
        rb.velocity = transform.up * initialVelocity;
    }
    private void HandleMovement()
    {
        // Apply thrust according to velocity function
        float fixedDeltaVelocity = CalculateFixedDeltaVelocity();
        float thrustForce = fixedDeltaVelocity * rb.mass / Time.fixedDeltaTime; // F = m * a, where a = Δv / Δt
        rb.AddForce(transform.up * thrustForce * Time.fixedDeltaTime);
        if (rb.velocity.magnitude > targetVelocity)
        {
            rb.velocity = rb.velocity.normalized * targetVelocity;
        }
        // Handle rotation torque
        float rotationInput = CalculateRotationInput();
        float torque = CalculateRotationTorque(rotationInput);
        rb.AddTorque(torque * Time.fixedDeltaTime);
    }
    private float CalculateFixedDeltaVelocity()
    {
        float velocityLastTime = lastFixedVelocity;
        float velocityNow = VelocityFunction(lastFixedTime + Time.fixedDeltaTime);
        float deltaVelocity = velocityNow - velocityLastTime;
        lastFixedTime += Time.fixedDeltaTime;
        lastFixedVelocity = velocityNow;
        return deltaVelocity;
    }
    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb.angularVelocity;
        float desiredTorque = -steerInput * steerTorque;

        // Otherwise, apply the standard steer torque
        if (Mathf.Abs(angularVelocity) < maxAngularVelocity)
        {
            return desiredTorque;
        }
        else
        {
            return 0;
        }
    }
    private float CalculateRotationInput()
    {
        Vector2 directionToTarget = (Vector2)target.transform.position - rb.position;
        float angleToTarget = Vector2.SignedAngle(transform.up, directionToTarget);
        return Mathf.Clamp(angleToTarget / 180f, -1f, 1f); // Normalize to [-1, 1]
    }
    private float GetVelocityAtTime(float time)
    {
        float mass = rb.mass;
        if (time < accelerationDelay)
        {
            return initialVelocity;
        }
        else if (time < accelerationDelay + accelerationTime)
        {
            float t = (time - accelerationDelay) / accelerationTime; // Normalize time to [0, 1]
            float curveValue = accelerationCurve.Evaluate(t); // Get curve value
            return Mathf.Lerp(initialVelocity, targetVelocity, curveValue); // Interpolate velocity
        }
        else
        {
            return targetVelocity; // After acceleration phase, maintain target velocity
        }
    }

}