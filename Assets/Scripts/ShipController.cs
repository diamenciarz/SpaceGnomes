using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float steerTorque = 5f;
    [SerializeField, Range(0f, 1f)] private float perpendicularDamping = 0.9f;
    [SerializeField, Range(0f, 1f)] private float angularDamping = 0.8f;
    [SerializeField] private float maxAngularDampingTorque = 2f;
    [SerializeField] ShipControlInput shipControlInput = null;

    private Rigidbody2D rb;
    private float thrustInput; // Set by KeyListener
    private float steerInput; // Set by KeyListener

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
        if(shipControlInput != null)
        {
            thrustInput = shipControlInput.GetThrustInput();
            steerInput = shipControlInput.GetSteerInput();
        }
    }

    private void HandleMovement()
    {
        // Apply forward/backward thrust
        Vector2 thrustDirection = transform.up * thrustInput * thrustForce;
        rb.AddForce(thrustDirection);

        // Handle rotation torque
        float torque = CalculateRotationTorque(steerInput);
        Debug.Log("Calculated torque" + torque);

        rb.AddTorque(torque);
    }

    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb.angularVelocity;
        float desiredTorque = -steerInput * steerTorque;

        // Check if steer input is opposing the current rotation
        if (steerInput != 0f && Mathf.Sign(steerInput) * Mathf.Sign(angularVelocity) > 0f)
        {
            // Apply the higher of steerTorque or maxAngularDampingTorque to decelerate
            float maxTorque = Mathf.Max(steerTorque, maxAngularDampingTorque);
            float decelerationTorque = -Mathf.Sign(angularVelocity) * maxTorque;

            // Check if applying this torque would overshoot zero angular velocity
            float nextAngularVelocity = angularVelocity + decelerationTorque * Time.fixedDeltaTime;
            if (Mathf.Sign(nextAngularVelocity) != Mathf.Sign(angularVelocity) && angularVelocity != 0f)
            {
                // If overshooting, clamp to reach zero angular velocity exactly
                return -angularVelocity / Time.fixedDeltaTime;
            }

            return decelerationTorque;
        }

        // Otherwise, apply the standard steer torque
        return desiredTorque;
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