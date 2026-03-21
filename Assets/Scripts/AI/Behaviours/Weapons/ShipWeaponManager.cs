using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
            if (isControlledByPlayer || mouseCursorVisible)
            {
                HandleMouseControl(weapon, mouseCursorVisible);
                return;
            }
            List<Vector2> enemyPositions = visibleEnemies.Select(enemy => GetPredictedTargetPosition(weapon, enemy)).ToList();
            Vector2? targetPosition = rotationCone.GetClosestPositionInCone(enemyPositions);
            HandleAIControl(weapon, targetPosition);
        }
    }
    private bool IsMouseCursorVisible(Cone rotationCone, List<GameObject> visibleEnemies)
    {
        if (!visibleEnemies.Contains(EntityCounter.Instance.MouseCursor)) return false;
        return rotationCone.IsObjectInCone(EntityCounter.Instance.MouseCursor);
    }
    private void HandleMouseControl(Weapon weapon, bool mouseCursorVisible)
    {
        if(mouseCursorVisible)
        {
            RotateToTarget(weapon, EntityCounter.Instance.MouseCursor.transform.position);
            weapon.weaponController.isControlledByPlayer = true;
            //SetAction is handled by the player input, so we don't set it here
        }
        else
        {
            //StopRotation(weapon);
            DefaultRotation(weapon);
            weapon.weaponController.isControlledByPlayer = false;
        }
    }
    private void HandleAIControl(Weapon weapon, Vector2? targetPosition)
    {
        weapon.weaponController.isControlledByPlayer = false;
        if(targetPosition.HasValue)
        {
            RotateToTarget(weapon, targetPosition.Value);
            weapon.weaponController.SetAction(true);
        }
        else
        {
            StopRotation(weapon);
            //DefaultRotation(weapon); // Not sure if this is necessary, it will make the weapon point in a default direction when there are no targets, which might look weird
            weapon.weaponController.SetAction(false);
        }
    }
    private void RotateToTarget(Weapon weapon, Vector2 targetPosition)
    {
        weapon.rotator.SetTarget(targetPosition);
    }
    private Vector2 GetPredictedTargetPosition(Weapon weapon, GameObject target)
    {
        if (!predictTrajectories)
        {
            return target.transform.position;
        }
        Trajectory targetTrajectory = target.GetComponent<Trajectory>();
        if (!targetTrajectory)
        {
            return target.transform.position;
        }
        Vector2 shootingPoint = weapon.weaponController.GetShootingPointTransform().position;
        Vector2 hitPosition = GeometryUtils.CalculateTrajectoryHitCoordinates(targetTrajectory, shootingPoint, weapon.weaponController.GetProjectileSpeed());
        return hitPosition;
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
