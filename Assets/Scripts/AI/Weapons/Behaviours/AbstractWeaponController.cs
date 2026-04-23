using System;
using UnityEngine;
using static ISettableTarget;

public abstract class AbstractWeaponController: AbstractActionController, ISettableTarget
{
    protected TargetInstance? targettedObject;

    #region ISettableTarget Methods
    public void SetTarget(GameObject target)
    {
        targettedObject = new TargetInstance(target);
    }

    public void SetTarget(Vector2 position)
    {
        targettedObject = new TargetInstance(position);
    }
    public void StopTargetting()
    {
        targettedObject = null;
    }
    public TargetInstance? GetTarget()
    {
        return targettedObject;
    }
    #endregion
    public abstract Transform GetShootingPointTransform();
    public abstract Func<float, float> GetProjectileSpeed();
}