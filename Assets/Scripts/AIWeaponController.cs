using System;
using System.Collections.Generic;
using UnityEngine;

public class AIWeaponController : MonoBehaviour
{
    [Serializable]
    class WeaponConfig
    {
        public AbstractWeaponController weaponController;
        public CameraSensor cameraSensor;
        public Rotator rotator;
    }
    [SerializeField][Tooltip("Targets a position the target will be when the bullet reaches it")] 
    bool predictTrajectories;
    [SerializeField] List<WeaponConfig> weaponConfigs = new List<WeaponConfig>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ControlWeapons();
    }
    private void ControlWeapons()
    {
        foreach (WeaponConfig config in weaponConfigs)
        {
            GameObject target = config.cameraSensor.GetClosestVisibleEnemy();
            
            if (!target)
            {
                config.weaponController.SetShooting(false);
                config.rotator.StopTargeting();
                continue;
            }
            else
            {
                config.weaponController.SetShooting(true);
                RotateToTarget(config, target);
            }
        }
    }
    private void RotateToTarget(WeaponConfig config, GameObject target)
    {
        if (!predictTrajectories)
        {
            config.rotator.SetTarget(target);
            return;
        }
        Trajectory targetTrajectory = target.GetComponent<Trajectory>();
        if (!targetTrajectory)
        {
            config.rotator.SetTarget(target);
            return;
        }
        Vector2 shootingPoint = config.weaponController.GetShootingPointTransform().position;
        Vector2 hitPosition = GeometryUtils.CalculateTrajectoryHitCoordinates(targetTrajectory, shootingPoint, config.weaponController.GetProjectileSpeed());
        config.rotator.SetTarget(hitPosition);

    }
}
