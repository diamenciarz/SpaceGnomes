using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : AbstractSingleton<UIManager>
{
    [SerializeField] GameObject fieldOfViewPrefab;

    protected override void Awake()
    {
        base.Awake(); // Call base singleton setup
    }

    public ProgressBar InstantiateFieldOfView(GameObject followObj)
    {
        GameObject instance = Instantiate(fieldOfViewPrefab);
        ObjectFollower objectFollower = instance.GetComponent<ObjectFollower>();
        objectFollower.Follow(followObj);
        ProgressBar barScript = instance.GetComponent<ProgressBar>();
        return barScript;
    }
}
