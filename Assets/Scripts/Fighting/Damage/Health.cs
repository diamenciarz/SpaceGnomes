using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public bool isInvulnerable = false;
    public float value => currentHealth;

    private float currentHealth;
    private EntityTeam parentEntityTeam;
    private bool isDestroyed = false;
    private PooledObjectProperty pooledObjectProperty;
    public EntityTeam.Team team => parentEntityTeam? parentEntityTeam.team : EntityTeam.Team.Neutral;

    private void Start()
    {
        pooledObjectProperty = GetComponent<PooledObjectProperty>();
        currentHealth = maxHealth;
        UpdateParentEntityTeam();
    }

    public void UpdateParentEntityTeam()
    {
        parentEntityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if(isDestroyed) return;
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
            currentHealth = 0f;
            isDestroyed = true;
            Despawn();
        }
    }
    private void Despawn()
    {
        if (pooledObjectProperty == null)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
            return;
        }
        ObjectPoolManager.Instance.Despawn(gameObject, pooledObjectProperty.poolId);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collided with: " + collision.gameObject);
    }
}