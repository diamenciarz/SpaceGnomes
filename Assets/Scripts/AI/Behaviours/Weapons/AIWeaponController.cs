using System;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AIWeaponController : MonoBehaviour
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

        // For each weapon, check if any enemies are in its Rotator movement arc (consider true if null)
        foreach (Weapon weapon in group.weapons)
        {
            Cone rotationCone = weapon.rotator.GetRotationCone();
            GameObject closestEnemy = rotationCone.GetClosestObjectInCone(visibleEnemies);
            if (!closestEnemy)
            {
                weapon.weaponController.SetAction(false);
                weapon.rotator.StopTargeting();
            }
            else
            {
                weapon.weaponController.SetAction(true);
                RotateToTarget(weapon, closestEnemy);
            }
        }
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
}
