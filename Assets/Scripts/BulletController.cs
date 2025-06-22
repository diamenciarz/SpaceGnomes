using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    [SerializeField] private float initialVelocity = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string poolId = ""; // Pool ID for this bullet

    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Ensure kinematic for manual velocity control
    }

    private void OnEnable()
    {
        // Set initial velocity along forward direction (transform.up)
        rb.velocity = transform.up * initialVelocity;
        timer = 0f;
    }

    private void Update()
    {
        // Update lifetime timer
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            ObjectPoolManager.Instance.Despawn(gameObject, poolId);
        }
    }
}