using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasEntityType : MonoBehaviour
{
    public enum EntityType
    {
        Wall,
        Ship,
        Bullet,
        Explosion,
        SpaceDebris
    }
    [SerializeField] private EntityType entityType;
    public EntityType Type => entityType;
    private bool wasRegistered = false;

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
