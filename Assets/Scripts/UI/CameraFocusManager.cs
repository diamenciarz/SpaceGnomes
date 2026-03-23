using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocusManager : AbstractSingleton<CameraFocusManager>
{
    public List<GameObject> observedObjects = new List<GameObject>();

    private List<GameObject> hiddenObservedObjects = new List<GameObject>();
    private Coroutine currentLookingAnimation;
    private GameObject lookPositionObj = new GameObject("Look position");

    private IEnumerator LookTimer(Vector2 position, float waitSec, List<GameObject> hiddenObservedObjects)
    {
        lookPositionObj.transform.position = position;
        List<GameObject> lookList = new List<GameObject> { lookPositionObj };
        observedObjects = lookList;

        yield return new WaitForSeconds(waitSec);
        observedObjects = hiddenObservedObjects;
    }

    #region Mutator methods
    public void LookAtPosition(Vector2 position, float waitSec)
    {
        StopCoroutine(currentLookingAnimation);
        currentLookingAnimation = StartCoroutine(LookTimer(position, waitSec, hiddenObservedObjects));
    }
    public void ObserveMe(GameObject obj)
    {
        if (!hiddenObservedObjects.Contains(obj))
        {
            hiddenObservedObjects.Add(obj);
            observedObjects = hiddenObservedObjects;
        }
    }
    public void UnobserveMe(GameObject obj)
    {
        if (hiddenObservedObjects.Contains(obj))
        {
            hiddenObservedObjects.Remove(obj);
            observedObjects = hiddenObservedObjects;
        }
    }
    #endregion
}
