using UnityEngine;

public class EntityTeam : MonoBehaviour
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
    public static Team playerTeam = Team.Team1; // Default player team, can be changed in inspector or at runtime
    public Team team => myTeam;
    [SerializeField] private Team myTeam = Team.Neutral;
    public void SetTeam(Team newTeam) => myTeam = newTeam;

    private void Awake()
    {
        TeamManager.Instance.SetEntityTexturesToTeamColor(gameObject, team);
    }
}
