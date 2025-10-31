using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityTeam;
using static HasEntityType;
using static UnityEngine.EventSystems.EventTrigger;

public class TeamManager : MonoBehaviour
{
    // This class will keep track of which entity belongs to which team and will control team switching
    // Singleton instance
    public static TeamManager Instance { get; private set; }
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public Team GetEntityTeam(GameObject entity)
    {
        Health hm = entity.GetComponent<Health>();
        if (hm != null)
        {
            return hm.team;
        }
        EntityTeam et = GetParentEntityTeam(entity);
        if (et != null)
        {
            return et.team;
        }
        Debug.LogError("Entity does not have Health or EntityTeam component to determine team!");
        return Team.Neutral;
    }

    public EntityTeam GetParentEntityTeam(GameObject obj)
    {
        return GetParentEntityTeamRecursive(obj, obj.GetComponent<EntityTeam>());
    }

    public EntityTeam GetParentEntityTeamRecursive(GameObject obj, EntityTeam currentHighest)
    {
        GameObject parent = obj.transform.parent?.gameObject;
        if (parent == null)
        {
            return currentHighest;
        }
        EntityTeam parentScript = parent.GetComponent<EntityTeam>();
        if (parentScript != null)
        {
            currentHighest = parentScript;
        }
        return GetParentEntityTeamRecursive(parent, currentHighest);
    }

    private bool IsAlly(Team myTeam, Team otherTeam)
    {
        if (myTeam == Team.Neutral || myTeam == Team.EnemyToAll) return false;
        if (otherTeam == Team.Neutral || otherTeam == Team.EnemyToAll) return false;
        return myTeam == otherTeam;
    }

    private bool IsEnemy(Team myTeam, Team otherTeam)
    {
        if (otherTeam == Team.Neutral) return false;
        if (otherTeam == Team.EnemyToAll) return true;
        if (myTeam == Team.EnemyToAll) return true;
        return myTeam != otherTeam;
    }

    public List<GameObject> GetNearbyEntitiesInTeam(List<EntityType> types, Vector2 center, float radius, Team team)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (EntityType type in types)
        {
            entitiesInTeam.AddRange(GetNearbyEntitiesInTeam(type, center, radius, team));
        }
        return entitiesInTeam;
    }
    
    public List<GameObject> GetNearbyEntitiesInTeam(EntityType type, Vector2 center, float radius, Team team)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetNearbyEntities(type, center, radius))
        {
            if (GetEntityTeam(entity)==team)
            {
                entitiesInTeam.Add(entity);
            }
        }
        return entitiesInTeam;
    }
    public List<GameObject> GetNearbyEntitiesInTeams(List<EntityType> types, Vector2 center, float radius, List<Team> teams)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (EntityType type in types)
        {
            entitiesInTeam.AddRange(GetNearbyEntitiesInTeams(type, center, radius, teams));
        }
        return entitiesInTeam;
    }

    public List<GameObject> GetNearbyEntitiesInTeams(EntityType type, Vector2 center, float radius, List<Team> teams)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetNearbyEntities(type, center, radius))
        {
            if (teams.Contains(GetEntityTeam(entity)))
            {
                entitiesInTeam.Add(entity);
            }
        }
        return entitiesInTeam;
    }
    public List<GameObject> GetEntitiesInTeam(List<EntityType> types, Team team)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (EntityType type in types)
        {
            entitiesInTeam.AddRange(GetEntitiesInTeam(type, team));
        }
        return entitiesInTeam;
    }

    public List<GameObject> GetEntitiesInTeam(EntityType type, Team team)
    {
        List<GameObject> entitiesInTeam = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetEntities(type))
        {
            if (GetEntityTeam(entity)==team)
            {
                entitiesInTeam.Add(entity);
            }
        }
        return entitiesInTeam;
    }

    public List<GameObject> GetNearbyAllies(List<EntityType> types, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> allies = new List<GameObject>();
        foreach (EntityType type in types)
        {
            allies.AddRange(GetNearbyAllies(type, center, radius, myTeam));
        }
        return allies;
    }

    public List<GameObject> GetNearbyAllies(EntityType type, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> allies = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetNearbyEntities(type, center, radius))
        {
            if (IsAlly(myTeam, GetEntityTeam(entity)))
            {
                allies.Add(entity);
            }
        }
        return allies;
    }

    public List<GameObject> GetNearbyEnemies(List<EntityType> types, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (EntityType type in types)
        {
            enemies.AddRange(GetNearbyEnemies(type, center, radius, myTeam));
        }
        return enemies;
    }

    public List<GameObject> GetNearbyEnemies(EntityType type, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetNearbyEntities(type, center, radius))
        {
            if (IsEnemy(myTeam, GetEntityTeam(entity)))
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }

    public List<GameObject> GetAllies(List<EntityType> types, Team myTeam)
    {
        List<GameObject> allies = new List<GameObject>();
        foreach (EntityType type in types)
        {
            allies.AddRange(GetAllies(type, myTeam));
        }
        return allies;
    }

    public List<GameObject> GetAllies(EntityType type, Team myTeam)
    {
        List<GameObject> allies = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetEntities(type))
        {
            if (IsAlly(myTeam, GetEntityTeam(entity)))
            {
                allies.Add(entity);
            }
        }
        return allies;
    }

    public List<GameObject> GetEnemies(List<EntityType> types, Team myTeam)
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (EntityType type in types)
        {
            enemies.AddRange(GetEnemies(type, myTeam));
        }
        return enemies;
    }

    public List<GameObject> GetEnemies(EntityType type, Team myTeam)
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in EntityCounter.Instance.GetEntities(type))
        {
            if (IsEnemy(myTeam, GetEntityTeam(entity)))
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }
}
