using System;
using System.Collections;
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
    [SerializeField] List<WeaponConfig> weaponConfigs = new List<WeaponConfig>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (WeaponConfig config in weaponConfigs)
        {
            GameObject[] enemies = config.cameraSensor.GetVisibleEnemies();
            if (enemies.Length == 0)
            {
                config.weaponController.SetShooting(false);
                config.rotator.StopTargeting();
                continue;
            }

            GameObject target = GeometryUtils.FindClosestEntityToPosition(enemies, config.cameraSensor.transform.position);
            config.weaponController.SetShooting(true);
            config.rotator.SetTarget(target);
        }
    }
}
