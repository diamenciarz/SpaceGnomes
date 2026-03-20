using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSensor : MonoBehaviour
{
    public abstract List<GameObject> GetVisibleEnemies();
    public abstract List<GameObject> GetVisibleAllies();
    public abstract List<GameObject> GetVisibleObjects();
    public abstract GameObject GetClosestVisibleEnemy();
    public abstract GameObject GetClosestVisibleAlly();
    public abstract GameObject GetClosestVisibleObject();
}
