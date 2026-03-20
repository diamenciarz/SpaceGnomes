using UnityEngine;

public abstract class AbstractWeaponController: AbstractActionController
{
    public abstract Transform GetShootingPointTransform();
    public abstract float GetProjectileSpeed();
}