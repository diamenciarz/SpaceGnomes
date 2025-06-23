using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController: MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float thrustDampingForce = 20f;
    [SerializeField][Range(0f, 20)] float maxVelocity;

    [SerializeField, Range(0f, 1f)] private float perpendicularDamping = 0.9f;
    [Header("Rotation")]
    [SerializeField] private float steerTorque = 5f;
    [SerializeField] private float maxAngularDampingTorque = 10f;
    [SerializeField] [Range(0f, 360)] float maxAngularVelocity;
    [SerializeField, Range(0f, 1f)] private float angularDamping = 0.8f;
    [Header("Instances")]
    [SerializeField] private ShipControlInput shipControlInput = null;

    private Rigidbody2D rb;
    private float thrustInput;
    private float steerInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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
        if (shipControlInput != null)
        {
            thrustInput = shipControlInput.GetThrustInput();
            steerInput = shipControlInput.GetSteerInput();
        }
    }

    private void HandleMovement()
    {
        // Apply thrust (forward/backward)
        Vector2 thrust = CalculateThrust(thrustInput);
        rb.AddForce(thrust * Time.fixedDeltaTime);
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        // Handle rotation torque
        float torque = CalculateRotationTorque(steerInput);
        rb.AddTorque(torque * Time.fixedDeltaTime);
    }

    private Vector2 CalculateThrust(float thrustInput)
    {
        Vector2 forward = transform.up;
        Vector2 velocity = rb.velocity;
        float forwardVelocity = Vector2.Dot(velocity, forward); // Velocity along ship's forward axis
        Vector2 thrustDirection = forward * thrustInput * thrustForce;

        // Check if thrust input opposes the current forward velocity
        if (thrustInput != 0f && Mathf.Sign(thrustInput) * Mathf.Sign(forwardVelocity) < 0f)
        {
            // Apply the higher of thrustForce or thrustDampingForce to decelerate
            float maxForce = Mathf.Max(thrustForce, thrustDampingForce);
            Vector2 decelerationForce = -Mathf.Sign(forwardVelocity) * forward * maxForce;
            return decelerationForce;
        }

        // Otherwise, apply standard thrust
        return thrustDirection;
    }

    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb.angularVelocity;
        float desiredTorque = -steerInput * steerTorque;

        // Check if steer input opposes the current rotation
        if (steerInput != 0f && Mathf.Sign(steerInput) * Mathf.Sign(angularVelocity) > 0f)
        {
            // Apply the higher of steerTorque or maxAngularDampingTorque to decelerate
            float maxTorque = Mathf.Max(steerTorque, maxAngularDampingTorque);
            float decelerationTorque = -Mathf.Sign(angularVelocity) * maxTorque;
            return decelerationTorque;
        }

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

    private void ApplyAngularDamping()
    {
        // Apply counter-torque only when steer input is approximately zero
        if (Mathf.Abs(steerInput) < 0.01f)
        {
            // Calculate counter-torque based on angular velocity and damping
            float counterTorque = -rb.angularVelocity * angularDamping;
            // Clamp the counter-torque to the maximum angular damping torque
            counterTorque = Mathf.Clamp(counterTorque, -maxAngularDampingTorque, maxAngularDampingTorque);
            rb.AddTorque(counterTorque);
        }
    }

    private void ApplyPerpendicularDamping()
    {
        // Get the ship's forward direction
        Vector2 forward = transform.up;
        // Get current velocity
        Vector2 velocity = rb.velocity;
        // Project velocity onto forward direction
        Vector2 forwardVelocity = Vector2.Dot(velocity, forward) * forward;
        // Calculate perpendicular velocity
        Vector2 perpendicularVelocity = velocity - forwardVelocity;
        // Apply damping to perpendicular velocity
        Vector2 dampedPerpendicularVelocity = perpendicularVelocity * (1f - perpendicularDamping);
        // Reconstruct velocity with damped perpendicular component
        rb.velocity = forwardVelocity + dampedPerpendicularVelocity;
    }
}