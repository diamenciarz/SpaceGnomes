using UnityEngine;

public abstract class AbstractWeaponController: MonoBehaviour
{
    public abstract void SetShooting(bool isShooting);
    public abstract ShipAction GetActionType();
    public abstract void Detach();
    public abstract Transform GetShootingPointTransform();
    public abstract float GetProjectileSpeed();
}