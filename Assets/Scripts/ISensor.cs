using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISensor
{
    public List<GameObject> GetVisibleEnemies();
    public List<GameObject> GetVisibleAllies();
    public List<GameObject> GetVisibleObjects();
}
