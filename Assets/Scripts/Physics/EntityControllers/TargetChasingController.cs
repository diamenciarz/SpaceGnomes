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

    [Header("Movement Settings")]
    [SerializeField, Range(0f, 1f), Tooltip("This is applied during the delay period")] private float initialPerpendicularDamping = 0.1f;
    [SerializeField, Range(0f, 1f)] private float perpendicularDamping = 0.1f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float steerTorque = 5000f;
    [SerializeField][Range(0f, 360)] float maxAngularVelocity = 100f;
    [SerializeField][Range(1f, 10f)] private float maxAngularDampingMultiplier = 2f;

    [Header("Instance Settings")]
    [SerializeField] private List<AbstractSensor> sensors = new List<AbstractSensor>();

    [Header("Predator Mode Settings")]
    [SerializeField][Tooltip("If true, will turn off perpendicularDamping allowing the rocket to turn around quickly")] bool usePredatorMode = false;
    [SerializeField][Tooltip("Predator mode will be turned on once angle to target is above this value")] float turnOnAboveAngle = 90f;
    [SerializeField][Tooltip("Predator mode will be turned off once angle to target is below this value")] float turnOffBelowAngle = 5f;

    // Physics Settings
    [Tooltip("This value makes force calculation match the desired deltaVelocity")] 
    private float THRUST_MULTIPLIER = 45f;


    public new Func<float, float> VelocityFunction => GetVelocityAtTime;

    private Rigidbody2D rb2d;
    private GameObject target;
    private float lastFixedVelocity;
    private float lastFixedTime;
    private float startTime;

    private bool isInPredatorMode = false;

    public override void Activate()
    {
        base.Activate();
        // Set initial velocity along forward direction (transform.up)
        rb2d.velocity = transform.right * initialVelocity;
        lastFixedTime = Time.time;
        lastFixedVelocity = initialVelocity;
        startTime = Time.time;
        target = null;
    }
    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        HandleMovement();
        ApplyPerpendicularDamping();
    }
    private void Update()
    {
        UpdateTarget();
        if(target) Debug.DrawLine(transform.position, target.transform.position, Color.red);
    }
    private void ApplyPerpendicularDamping()
    {
        Vector2 forward = transform.right;
        Vector2 velocity = rb2d.velocity;
        // Project velocity onto forward direction
        Vector2 forwardVelocity = Vector2.Dot(velocity, forward) * forward;
        // Calculate perpendicular velocity
        Vector2 perpendicularVelocity = velocity - forwardVelocity;
        // Apply damping to perpendicular velocity
        Vector2 dampedPerpendicularVelocity = perpendicularVelocity * (1f - GetPerpendicularDamping());
        // Reconstruct velocity with damped perpendicular component
        rb2d.velocity = forwardVelocity + dampedPerpendicularVelocity;
    }
    private float GetPerpendicularDamping()
    {
        if (Time.time < startTime + accelerationDelay) return initialPerpendicularDamping;
        if(!isInPredatorMode) return perpendicularDamping;

        float angleToTarget = CalculateAngleToTarget();
        if(Mathf.Abs(angleToTarget) < turnOffBelowAngle)
        {
            isInPredatorMode = false;
            return perpendicularDamping;
        }
        else
        {
            return 0f; // No damping in predator mode for quick turning
        }
    }
    private void UpdateTarget()
    {
        if (target != null) return;
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
    private void HandleMovement()
    {
        // Apply thrust according to velocity function
        HandleThrust();
        // Handle rotation torque
        HandleTorque();
    }
    private void HandleThrust()
    {
        float fixedDeltaVelocity = CalculateFixedDeltaVelocity();
        float thrustForce = fixedDeltaVelocity * rb2d.mass / Time.fixedDeltaTime; // F = m * a, where a = Δv / Δt
        rb2d.AddForce(transform.right * thrustForce * Time.fixedDeltaTime * THRUST_MULTIPLIER);
        ClampVelocity();
    }
    private void HandleTorque()
    {
        if (!target) return;
        float angleToTarget = CalculateAngleToTarget();
        float steerInput = Mathf.Clamp(angleToTarget / 180, -1, 1); // Normalize to [-1, 1]

        CheckPredatorMode(angleToTarget);

        float torque = CalculateRotationTorque(steerInput);
        rb2d.AddTorque(torque * Time.fixedDeltaTime);

        ClampAngularVelocity();
    }
    private void CheckPredatorMode(float angleToTarget)
    {
        if(usePredatorMode && !isInPredatorMode && Mathf.Abs(angleToTarget) > turnOnAboveAngle) isInPredatorMode = true;
    }
    private void ClampVelocity()
    {
        if (rb2d.velocity.magnitude > targetVelocity)
        {
            rb2d.velocity = rb2d.velocity.normalized * targetVelocity;
        }
    }
    private void ClampAngularVelocity()
    {
        if(Mathf.Abs(rb2d.angularVelocity) > maxAngularVelocity) rb2d.angularVelocity = maxAngularVelocity * Mathf.Sign(rb2d.angularVelocity);
    }
    private float CalculateFixedDeltaVelocity()
    {
        if (lastFixedTime + Time.fixedDeltaTime > startTime + accelerationDelay + accelerationTime)
        {
            // If we've passed the acceleration phase, return the last delta velocity calculated at the end of the acceleration phase
            // This is fine because we clamp RigidBody2D velocity to targetVelocity after the acceleration phase, so any extra velocity from the curve won't affect the physics
            float velocityLastTime = lastFixedVelocity;
            float velocityBefore= VelocityFunction(lastFixedTime - Time.fixedDeltaTime - startTime);
            float deltaVelocity = velocityLastTime - velocityBefore;
            return deltaVelocity;
        }
        else
        {
            float velocityLastTime = lastFixedVelocity;
            float velocityNow = VelocityFunction(lastFixedTime + Time.fixedDeltaTime - startTime);
            float deltaVelocity = velocityNow - velocityLastTime;
            lastFixedTime += Time.fixedDeltaTime;
            lastFixedVelocity = velocityNow;
            return deltaVelocity;
        }
    }
    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb2d.angularVelocity;

        // Check if steer input opposes the current rotation
        if (steerInput != 0f && Mathf.Sign(steerInput) * Mathf.Sign(angularVelocity) < 0f)
        {
            // Apply the higher of steerTorque or maxAngularDampingTorque to decelerate
            float maxAngularDampingTorque = steerTorque * maxAngularDampingMultiplier;
             // Deceleration torque
            return Mathf.Sign(steerInput) * maxAngularDampingTorque;
        }

        // Otherwise, apply the standard steer torque
        if (Mathf.Abs(angularVelocity) <= maxAngularVelocity)
        {
            // Acceleration torque
            return steerInput * steerTorque;
        }
        else
        {
            return 0;
        }
    }
    private float CalculateAngleToTarget()
    {
        Vector2 directionToTarget = (Vector2)target.transform.position - rb2d.position;
        return Vector2.SignedAngle(transform.right, directionToTarget);
    }
    private float GetVelocityAtTime(float time)
    {
        float mass = rb2d.mass;
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