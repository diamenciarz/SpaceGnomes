using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public enum Team
    {
        Neutral, // e.g., invincible walls
        EnemyToAll, // e.g., asteroids
        Team1,
        Team2,
        Team3,
        Team4,
        Team5,
        Team6,
        Team7,
        Team8
    }

    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private float currentHealth;
    public bool isInvulnerable = false;

    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable)
        {
            //Debug.Log("Entity is invulnerable, no damage taken.");
            return;
        }
        if (damage < 0f)
        {
            Debug.LogWarning("Damage cannot be negative!");
            return;
        }
        //Debug.Log("Received " + damage + " damage!");

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collided with: " + collision.gameObject);
    }
}