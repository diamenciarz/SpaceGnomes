using UnityEngine;
using static AIControlInput;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicularController: AbstractPhysicsController
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 2400f;
    [SerializeField][Range(1f, 10f)] private float thrustDampingMultiplier = 3f;
    [SerializeField][Range(0f, 20)] float maxVelocity = 10f;

    [SerializeField, Range(0f, 1f)] private float perpendicularDamping = 0.1f;
    [Header("Rotation")]
    [SerializeField] private float steerTorque = 10000f;
    [SerializeField][Range(1f, 10f)] private float maxAngularDampingMultiplier = 2f;
    [SerializeField][Range(0f, 360)] float maxAngularVelocity = 100f;
    [SerializeField][Range(0f, 1f)] private float angularDamping = 0.2f;
    

    private float thrustInput;
    private float steerInput;

    protected override void Awake()
    {
        base.Awake();
        thrustInput = 0f;
        steerInput = 0f;
    }

    private void FixedUpdate()
    {
        UpdateInputs();
        HandleMovement();
        ApplyPerpendicularDamping();
        ApplyAngularDamping();
    }
    private void UpdateInputs()
    {
        ControlInput controlInput = Input.GetKey(alternativeControlKey) ? alternativeShipControlInput : mainShipControlInput;
        if (controlInput != null)
        {
            thrustInput = controlInput.GetVerticalInput(ControlVectorCoordinates.Local, true);
            steerInput = controlInput.GetHorizontalInput(ControlVectorCoordinates.Local, true);
        }
    }

    public float GetMaxVelocity()
    {
        return maxVelocity;
    }
    public float GetMaxAngularVelocity()
    {
        return maxAngularVelocity;
    }

    private void HandleMovement()
    {
        // Apply thrust (forward/backward)
        Vector2 thrust = CalculateThrust(thrustInput);
        rb2d.AddForce(thrust * Time.fixedDeltaTime);
        if (rb2d.velocity.magnitude > maxVelocity)
        {
            rb2d.velocity = rb2d.velocity.normalized * maxVelocity;
        }

        // Handle rotation torque
        float torque = CalculateRotationTorque(steerInput);
        rb2d.AddTorque(torque * Time.fixedDeltaTime);
    }

    private Vector2 CalculateThrust(float thrustInput)
    {
        Vector2 forward = transform.up;
        Vector2 velocity = rb2d.velocity;
        float forwardVelocity = Vector2.Dot(velocity, forward); // Velocity along ship's forward axis
        Vector2 thrustDirection = forward * thrustInput * thrustForce;

        // Check if thrust input opposes the current forward velocity
        if (thrustInput != 0f && Mathf.Sign(thrustInput) * Mathf.Sign(forwardVelocity) < 0f)
        {
            // Apply the higher of thrustForce or thrustDampingForce to decelerate
            float thrustDampingForce = thrustForce * thrustDampingMultiplier;
            Vector2 decelerationForce = -Mathf.Sign(forwardVelocity) * forward * thrustDampingForce;
            return decelerationForce;
        }

        // Otherwise, apply standard thrust
        return thrustDirection;
    }

    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb2d.angularVelocity;

        // Check if steer input opposes the current rotation
        if (steerInput != 0f && Mathf.Sign(steerInput) * Mathf.Sign(angularVelocity) > 0f)
        {
            // Apply the higher of steerTorque or maxAngularDampingTorque to decelerate
            float maxAngularDampingTorque = steerTorque * maxAngularDampingMultiplier;
            float decelerationTorque = -Mathf.Sign(angularVelocity) * maxAngularDampingTorque;
            return decelerationTorque;
        }

        // Otherwise, apply the standard steer torque
        if (Mathf.Abs(angularVelocity) < maxAngularVelocity)
        {
            float desiredTorque = -steerInput * steerTorque;
            return desiredTorque;
        }
        else
        {
            return 0;
        }
    }

    private void ApplyAngularDamping()
    {
        if (Mathf.Abs(rb2d.angularVelocity) > maxAngularVelocity)
        {
            rb2d.angularVelocity = maxAngularVelocity * Mathf.Sign(rb2d.angularVelocity);
        }
        // Apply counter-torque only when steer input is approximately zero
        if (Mathf.Abs(steerInput) < 0.01f)
        {
            // Calculate counter-torque based on angular velocity and damping
            float counterTorque = -rb2d.angularVelocity * angularDamping;
            // Clamp the counter-torque to the maximum angular damping torque
            float maxAngularDampingTorque = steerTorque * maxAngularDampingMultiplier;
            counterTorque = Mathf.Clamp(counterTorque, -maxAngularDampingTorque, maxAngularDampingTorque);
            rb2d.AddTorque(counterTorque);
        }
    }

    private void ApplyPerpendicularDamping()
    {
        Vector2 forward = transform.up;
        Vector2 velocity = rb2d.velocity;
        // Project velocity onto forward direction
        Vector2 forwardVelocity = Vector2.Dot(velocity, forward) * forward;
        // Calculate perpendicular velocity
        Vector2 perpendicularVelocity = velocity - forwardVelocity;
        // Apply damping to perpendicular velocity
        Vector2 dampedPerpendicularVelocity = perpendicularVelocity * (1f - perpendicularDamping);
        // Reconstruct velocity with damped perpendicular component
        rb2d.velocity = forwardVelocity + dampedPerpendicularVelocity;
    }
}