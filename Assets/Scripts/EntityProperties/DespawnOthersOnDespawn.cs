using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnOthersOnDespawn : ActivateOnDespawn
{
    [SerializeField] public List<GameObject> objectsToDespawn = new List<GameObject>();

    public override void Activate()
    {
        base.Activate();
        DespawnOthers();
    }
    private void DespawnOthers()
    {
        foreach (GameObject obj in objectsToDespawn)
        {
            if (obj == null) continue;

            PooledObjectProperty pooledObjectProperty = obj.GetComponent<PooledObjectProperty>();
            if (pooledObjectProperty == null)
            {
                ObjectPoolManager.Instance.Despawn(obj);
            }
            else
            {
                ObjectPoolManager.Instance.Despawn(obj, pooledObjectProperty.poolId);
            }
        }
    }
}
