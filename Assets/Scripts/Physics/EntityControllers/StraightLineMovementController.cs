using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StraightLineMovementController : AutonomousMovementController
{
    [SerializeField] private float initialVelocity = 20f;
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Ensure kinematic for manual velocity control
    }

    public override void Activate()
    {
        base.Activate();
        // Set initial velocity along forward direction (transform.up)
        rb.velocity = transform.right * initialVelocity;
    }

    public override float VelocityFunction(float time)
    {
        // For a simple bullet, velocity is constant, so we ignore time
        return initialVelocity;
    }
}