using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WeaponManager : MonoBehaviour
{
    [Serializable]
    class Weapon
    {
        public AbstractWeaponController weaponController;
        public Rotator rotator;
    }
    [Serializable]
    class WeaponGroupWithSensors
    {
        public List<AbstractSensor> sensors = new List<AbstractSensor>();
        public List<Weapon> weapons = new List<Weapon>();
    }

    [SerializeField][Tooltip("If true, the weapon manager will be controlled by the player instead of automatically targeting enemies")]
    bool isControlledByPlayer = false;
    [SerializeField][Tooltip("Targets a position the target will be when the bullet reaches it")] 
    bool predictTrajectories;
    [SerializeField] List<WeaponGroupWithSensors> weaponGroupsWithSharedSensors = new List<WeaponGroupWithSensors>();

    #region Public Methods
    public void SetControlledByPlayer(bool isOn) => isControlledByPlayer = isOn;
    #endregion

    void Update()
    {
        ControlWeaponGroups();
    }
    private void ControlWeaponGroups()
    {
        weaponGroupsWithSharedSensors.ForEach(group => HandleWeaponGroup(group));
    }
    private void HandleWeaponGroup(WeaponGroupWithSensors group)
    {
        List<GameObject> visibleEnemies = GetAllEnemiesSeenBySensors(group);
        foreach (Weapon weapon in group.weapons)
        {
            HandleWeapon(weapon, visibleEnemies);
        }
    }
    private List<GameObject> GetAllEnemiesSeenBySensors(WeaponGroupWithSensors group)
    {
        List<GameObject> visibleEnemies = new List<GameObject>();
        group.sensors.ForEach(sensor => visibleEnemies.AddRange(sensor.GetVisibleEnemies()));
        return visibleEnemies.Distinct().ToList();
    }
    private void HandleWeapon(Weapon weapon, List<GameObject> visibleEnemies)
    {
        Cone rotationCone = weapon.rotator.GetRotationCone();
        bool mouseCursorVisible = visibleEnemies.Contains(EntityCounter.Instance.MouseCursor);
        if (isControlledByPlayer || mouseCursorVisible)
        {
            HandleMouseControl(weapon, mouseCursorVisible);
            return;
        }
        List<Vector2> enemyPositions = visibleEnemies.Select(enemy => GetPredictedTargetPosition(weapon, enemy)).ToList();
        Vector2? targetPosition = rotationCone.GetClosestPosition(enemyPositions);
        int targetIndex = enemyPositions.FindIndex(pos => pos == targetPosition);
        GameObject target = targetIndex != -1 ? visibleEnemies[targetIndex] : null;
        HandleAIControl(weapon, targetPosition, target);
    }
    private void HandleMouseControl(Weapon weapon, bool mouseCursorVisible)
    {
        if(mouseCursorVisible)
        {
            RotateToTarget(weapon, EntityCounter.Instance.MouseCursor.transform.position);
            weapon.weaponController.SetControlledByPlayer(true);
            //SetAction is handled by the player input, so we don't set it here
        }
        else
        {
            StopRotation(weapon);
            //DefaultRotation(weapon);
            weapon.weaponController.SetControlledByPlayer(false);
        }
    }
    private void HandleAIControl(Weapon weapon, Vector2? targetPosition, GameObject target)
    {
        weapon.weaponController.SetControlledByPlayer(false);
        if(target)
        {
            RotateToTarget(weapon, targetPosition.Value);
            weapon.weaponController.SetTarget(target);
            weapon.weaponController.Activate();
        }
        else
        {
            StopRotation(weapon);
            //DefaultRotation(weapon); // Not sure if this is necessary, it will make the weapon point in a default direction when there are no targets, which might look weird
            weapon.weaponController.SetTarget(null);
            weapon.weaponController.Deactivate();
        }
    }
    private void RotateToTarget(Weapon weapon, Vector2 targetPosition)
    {
        weapon.rotator.SetTarget(targetPosition);
    }
    private Vector2 GetPredictedTargetPosition(Weapon weapon, GameObject target)
    {
        if (!predictTrajectories) return target.transform.position;

        Trajectory targetTrajectory = target.GetComponent<Trajectory>();
        if (!targetTrajectory) return target.transform.position;
        
        Vector2 shootingPoint = weapon.weaponController.GetShootingPointTransform().position;
        Vector2 hitPosition = targetTrajectory.CalculateCoordinatesToHitTrajectory(shootingPoint, weapon.weaponController.GetProjectileSpeed());
        return hitPosition;
    }
    private void StopRotation(Weapon weapon)
    {
        weapon.rotator.StopTargetting();
        weapon.weaponController.SetTarget(null);
        weapon.weaponController.Deactivate();
    }
    // If we change our mind and want the weapon to point in a default direction when there are no targets, we can use this method instead of StopRotation
    private void DefaultRotation(Weapon weapon)
    {
        weapon.rotator.SetDefaultRotation();
        weapon.weaponController.SetTarget(null);
        weapon.weaponController.Deactivate();
    }
}
