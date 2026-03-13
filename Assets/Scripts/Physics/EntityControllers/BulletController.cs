using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : ActivateOnSpawn
// Consider making this into a generic forward movement controller
{
    [SerializeField] private float initialVelocity = 20f;

    private Rigidbody2D rb;

    public float InitialVelocity => initialVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Ensure kinematic for manual velocity control
    }

    public override void Activate()
    {
        // Set initial velocity along forward direction (transform.up)
        rb.velocity = transform.up * initialVelocity;
    }


}