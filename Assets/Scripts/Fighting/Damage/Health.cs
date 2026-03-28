using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public bool isInvulnerable = false;
    public float value => currentHealth;

    private float currentHealth;
    private EntityTeam parentEntityTeam;
    private bool isDead = false;
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
        if(isDead) return;
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
            isDead = true;
            ObjectPoolManager.Instance.Despawn(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collided with: " + collision.gameObject);
    }
}