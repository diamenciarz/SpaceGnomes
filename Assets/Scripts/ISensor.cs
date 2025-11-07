using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISensor
{
    public GameObject[] GetVisibleEnemies();
    public GameObject[] GetVisibleAllies();
    public GameObject[] GetVisibleObjects();
}
