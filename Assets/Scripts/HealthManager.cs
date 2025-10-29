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
    [SerializeField] private Team myTeam = Team.Neutral;
    public bool isInvulnerable = false;
    private HealthManager parentScript;

    public float CurrentHealth => currentHealth;
    public HealthManager ParentScript { get { return parentScript; } }
    public Team team => myTeam;
    public void SetTeam(Team newTeam) => myTeam = newTeam;

    private void Awake()
    {
        currentHealth = maxHealth;
        parentScript = GetParentHealthManager(gameObject, GetComponent<HealthManager>());
    }

    private HealthManager GetParentHealthManager(GameObject obj, HealthManager currentHighest)
    {
        GameObject parent = obj.transform.parent?.gameObject;
        if (parent == null)
        {
            return currentHighest;
        }
        HealthManager parentHM = parent.GetComponent<HealthManager>();
        if (parentHM != null)
        {
            currentHighest = parentHM;
        }
        return GetParentHealthManager(parent, currentHighest);
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