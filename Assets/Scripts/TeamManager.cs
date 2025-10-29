using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HasEntityType;
using static HealthManager;

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
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team == team)
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
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team == team)
            {
                entitiesInTeam.Add(entity);
            }
        }
        return entitiesInTeam;
    }

    public List<GameObject> GetNearbyAllies(List<EntityType> types, Vector2 center, float radius, Team myTeam)
    {
        return GetNearbyEntitiesInTeam(types, center, radius, myTeam);
    }

    public List<GameObject> GetNearbyAllies(EntityType type, Vector2 center, float radius, Team myTeam)
    {
        return GetNearbyEntitiesInTeam(type, center, radius, myTeam);
    }

    public List<GameObject> GetNearbyEnemies(List<EntityType> types, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> allNearby = new List<GameObject>();
        foreach (EntityType type in types)
        {
            allNearby.AddRange(EntityCounter.Instance.GetNearbyEntities(type, center, radius));
        }
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in allNearby)
        {
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team != myTeam)
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }

    public List<GameObject> GetNearbyEnemies(EntityType type, Vector2 center, float radius, Team myTeam)
    {
        List<GameObject> allNearby = EntityCounter.Instance.GetNearbyEntities(type, center, radius);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in allNearby)
        {
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team != myTeam)
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }

    public List<GameObject> GetAllies(List<EntityType> types, Team myTeam)
    {
        return GetEntitiesInTeam(types, myTeam);
    }

    public List<GameObject> GetAllies(EntityType type, Team myTeam)
    {
        return GetEntitiesInTeam(type, myTeam);
    }

    public List<GameObject> GetEnemies(List<EntityType> types, Team myTeam)
    {
        List<GameObject> allEntities = new List<GameObject>();
        foreach (EntityType type in types)
        {
            allEntities.AddRange(EntityCounter.Instance.GetEntities(type));
        }
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in allEntities)
        {
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team != myTeam)
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }

    public List<GameObject> GetEnemies(EntityType type, Team myTeam)
    {
        List<GameObject> allEntities = EntityCounter.Instance.GetEntities(type);
        List<GameObject> enemies = new List<GameObject>();
        foreach (GameObject entity in allEntities)
        {
            HealthManager hm = entity.GetComponent<HealthManager>();
            if (hm != null && hm.team != myTeam)
            {
                enemies.Add(entity);
            }
        }
        return enemies;
    }
}
