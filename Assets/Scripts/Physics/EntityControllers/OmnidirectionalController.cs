using UnityEngine;
using static ControlInput;

/// <summary>
/// OmnidirectionalController is a physics-based controller that allows for movement in any direction and independent rotation.
/// It applies thrust in the direction of the movement input and applies torque based on the rotation input.
/// The controller also includes damping to prevent excessive velocity and angular velocity, allowing for precise control of the ship's movement and rotation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class OmnidirectionalController: AbstractPhysicsController
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 2000f;
    [SerializeField][Range(1f, 10f)] private float thrustDampingMultiplier = 3f;
    [SerializeField][Range(0f, 20)] float maxVelocity = 10f;
    [SerializeField][Range(0f, 20)] float brakeSpeed = 0.2f;

    [Header("Rotation")]
    [SerializeField] private float steerTorque = 10000f;
    [SerializeField] private float maxAngularDampingMultiplier = 2f;
    [SerializeField] [Range(0f, 360)] float maxAngularVelocity = 100f;
    [SerializeField, Range(0f, 1f)] private float angularDamping = 0.2f;


    private Vector2 movementInput;
    private float rotationInput;
    private Vector2? targetVelocity;

    protected override void Awake()
    {
        base.Awake();
        movementInput = Vector2.zero;
        rotationInput = 0f;
    }

    private void FixedUpdate()
    {
        UpdateInputs();
        HandleMovement();
        ApplyAngularDamping();
    }

    public float GetMaxVelocity()
    {
        return maxVelocity;
    }
    public float GetMaxAngularVelocity()
    {
        return maxAngularVelocity;
    }

    private void UpdateInputs()
    {
        ControlInput controlInput = Input.GetKey(alternativeControlKey) ? alternativeShipControlInput : mainShipControlInput;
        if (controlInput != null)
        {
            ControlInputData controlInputData = controlInput.GetControlInput(ControlVectorCoordinates.World);
            movementInput = controlInputData.controlVector;
            rotationInput = controlInputData.rotation;
            targetVelocity = controlInputData.targetVelocity;
        }
    }

    private void HandleMovement()
    {
        if(ShouldSetVelocityToTarget())
        {
            rb2d.velocity = targetVelocity.Value; // Apply brake by reducing velocity
            return; // Skip applying thrust when braking
        }

        // Apply thrust in the direction of movementInput
        Vector2 thrust = CalculateThrust(movementInput);
        rb2d.AddForce(thrust * Time.fixedDeltaTime);
        if (rb2d.velocity.magnitude > maxVelocity)
        {
            rb2d.velocity = rb2d.velocity.normalized * maxVelocity;
        }

        // Handle rotation torque
        float torque = CalculateRotationTorque(rotationInput);
        rb2d.AddTorque(torque * Time.fixedDeltaTime);
    }
    private bool ShouldSetVelocityToTarget()
    {
        if (!targetVelocity.HasValue) return false;
        Vector2 velocityDelta = targetVelocity.Value - rb2d.velocity;
        return velocityDelta.magnitude < brakeSpeed;
    }
    private Vector2 CalculateThrust(Vector2 thrustInput)
    {
        Vector2 velocity = rb2d.velocity;
        float dot = Vector2.Dot(thrustInput, velocity);
        Vector2 thrustDirection = thrustInput * thrustForce;

        // If input opposes the current velocity, apply higher force to decelerate
        if (thrustInput != Vector2.zero && dot < 0f)
        {
            float thrustDampingForce = Mathf.Max(thrustForce, thrustForce * thrustDampingMultiplier);
            thrustDirection = thrustInput * thrustDampingForce;
        }

        return thrustDirection;
    }

    private float CalculateRotationTorque(float steerInput)
    {
        float angularVelocity = rb2d.angularVelocity;
        float desiredTorque = -steerInput * steerTorque;

        // Check if steer input opposes the current rotation
        if (steerInput != 0f && Mathf.Sign(steerInput) * Mathf.Sign(angularVelocity) > 0f)
        {
            // Apply the higher of steerTorque or maxAngularDampingTorque to decelerate
            float maxAngularDampingTorque = steerTorque * maxAngularDampingMultiplier;
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
        if (Mathf.Abs(rb2d.angularVelocity) > maxAngularVelocity)
        {
            rb2d.angularVelocity = maxAngularVelocity * Mathf.Sign(rb2d.angularVelocity);
        }
        // Apply counter-torque only when rotation input is approximately zero
        if (Mathf.Abs(rotationInput) < 0.01f)
        {
            // Calculate counter-torque based on angular velocity and damping
            float counterTorque = -rb2d.angularVelocity * angularDamping;
            // Clamp the counter-torque to the maximum angular damping torque
            float maxAngularDampingTorque = steerTorque * maxAngularDampingMultiplier;
            counterTorque = Mathf.Clamp(counterTorque, -maxAngularDampingTorque, maxAngularDampingTorque);
            rb2d.AddTorque(counterTorque);
        }
    }
}