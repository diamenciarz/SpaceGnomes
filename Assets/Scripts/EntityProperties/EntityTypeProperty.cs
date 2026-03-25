using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Trajectory))]
public class EntityTypeProperty : MonoBehaviour
{
    public enum EntityType
    {
        Wall,
        Ship,
        Plasma,
        Explosion,
        SpaceDebris,
        Missile
    }
    [SerializeField] private EntityType entityType;
    public EntityType Type => entityType;
    private bool wasRegistered = false;

    public EntityType[] GetCollidableEntityTypes()
    {
        return new EntityType[] { EntityType.Wall, EntityType.Ship, EntityType.SpaceDebris };
    }
    public void SetRegistered()
    {
        wasRegistered = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!wasRegistered)
        {
            EntityCounter.Instance.RegisterEntity(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
