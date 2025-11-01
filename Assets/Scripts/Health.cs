using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public bool isInvulnerable = false;
    public float CurrentHealth => currentHealth;

    private float currentHealth;
    private EntityTeam parentEntityTeam;
    public EntityTeam.Team team => parentEntityTeam? parentEntityTeam.team : EntityTeam.Team.Neutral;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateParentEntityTeam();
    }

    public void UpdateParentEntityTeam()
    {
        parentEntityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
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