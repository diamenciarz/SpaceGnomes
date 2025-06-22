using UnityEngine;

public class HealthManager : MonoBehaviour
{

    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

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
            ObjectPoolManager.Instance.Despawn(gameObject);
            gameObject.SetActive(false); // Deactivate for pooling or destruction
        }
    }
}