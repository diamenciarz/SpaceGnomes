using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ShipWeaponManager : MonoBehaviour
{
    [Serializable]
    class Weapon
    {
        public AbstractWeaponController weaponController;
        public Rotator rotator;
    }
    [Serializable]
    class WeaponGroup
    {
        public List<AbstractSensor> sensors = new List<AbstractSensor>();
        public List<Weapon> weapons = new List<Weapon>();
    }

    [SerializeField][Tooltip("If true, the weapon manager will be controlled by the player instead of automatically targeting enemies")]
    bool isControlledByPlayer = false;
    [SerializeField][Tooltip("Targets a position the target will be when the bullet reaches it")] 
    bool predictTrajectories;
    [SerializeField] List<WeaponGroup> weaponGroups = new List<WeaponGroup>();


    void Update()
    {
        ControlWeaponGroups();
    }
    private void ControlWeaponGroups()
    {
        weaponGroups.ForEach(group => HandleWeaponGroup(group));
    }
    private void HandleWeaponGroup(WeaponGroup group)
    {
        List<GameObject> visibleEnemies = new List<GameObject>();
        group.sensors.ForEach(sensor => visibleEnemies.AddRange(sensor.GetVisibleEnemies()));

        foreach (Weapon weapon in group.weapons)
        {
            Cone rotationCone = weapon.rotator.GetRotationCone();
            bool mouseCursorVisible = IsMouseCursorVisible(rotationCone, visibleEnemies);
            GameObject target = rotationCone.GetClosestObjectInCone(visibleEnemies);

            if (isControlledByPlayer || mouseCursorVisible)
            {
                HandleMouseControl(weapon, mouseCursorVisible);
            }
            else
            {
                HandleAIControl(weapon, target);
            }
        }
    }
    private void HandleMouseControl(Weapon weapon, bool mouseCursorVisible)
    {
        if(mouseCursorVisible)
        {
            RotateToTarget(weapon, EntityCounter.Instance.MouseCursor);
            weapon.weaponController.isControlledByPlayer = true;
        }
        else
        {
            StopRotation(weapon);
            //DefaultRotation(weapon);
            weapon.weaponController.isControlledByPlayer = false;
        }
    }
    private void HandleAIControl(Weapon weapon, GameObject target)
    {
        weapon.weaponController.isControlledByPlayer = false;
        if(target)
        {
            RotateToTarget(weapon, target);
            weapon.weaponController.SetAction(true);
        }
        else
        {
            StopRotation(weapon);
            //DefaultRotation(weapon); // Not sure if this is necessary, it will make the weapon point in a default direction when there are no targets, which might look weird
            weapon.weaponController.SetAction(false);
        }
    }
    private bool IsMouseCursorVisible(Cone rotationCone, List<GameObject> visibleEnemies)
    {
        if (!visibleEnemies.Contains(EntityCounter.Instance.MouseCursor)) return false;
        return rotationCone.IsObjectInCone(EntityCounter.Instance.MouseCursor);
    }
    private void RotateToTarget(Weapon weapon, GameObject target)
    {
        if (!predictTrajectories)
        {
            weapon.rotator.SetTarget(target);
            return;
        }
        Trajectory targetTrajectory = target.GetComponent<Trajectory>();
        if (!targetTrajectory)
        {
            weapon.rotator.SetTarget(target);
            return;
        }
        Vector2 shootingPoint = weapon.weaponController.GetShootingPointTransform().position;
        Vector2 hitPosition = GeometryUtils.CalculateTrajectoryHitCoordinates(targetTrajectory, shootingPoint, weapon.weaponController.GetProjectileSpeed());
        weapon.rotator.SetTarget(hitPosition);
    }
    private void StopRotation(Weapon weapon)
    {
        weapon.rotator.StopTargeting();
        weapon.weaponController.SetAction(false);
    }
    private void DefaultRotation(Weapon weapon)
    {
        weapon.rotator.SetDefaultRotation();
        weapon.weaponController.SetAction(false);
    }
}
