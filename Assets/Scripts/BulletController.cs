using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
    // Consider making this into a generic forward movement controller
{
    [SerializeField] private float initialVelocity = 20f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Ensure kinematic for manual velocity control
    }

    private void OnEnable()
    {
        // Set initial velocity along forward direction (transform.up)
        rb.velocity = transform.up * initialVelocity;
    }


}