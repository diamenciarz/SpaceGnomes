using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HealthManager healthManager = other.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.TakeDamage(damage);
        }
    }
}