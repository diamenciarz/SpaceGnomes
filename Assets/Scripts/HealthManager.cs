using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public enum EntityType
    {
        Wall,
        Ship
    }

    [SerializeField] private EntityType entityType = EntityType.Ship;
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public EntityType Type => entityType;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage < 0f)
        {
            Debug.LogWarning("Damage cannot be negative!");
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
        {
            gameObject.SetActive(false); // Deactivate for pooling or destruction
        }
    }
}