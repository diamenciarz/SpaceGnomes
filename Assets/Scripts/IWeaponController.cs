using UnityEngine;

public interface IWeaponController
{
    void SetShooting(bool isShooting);
    ShipAction GetActionType();
}