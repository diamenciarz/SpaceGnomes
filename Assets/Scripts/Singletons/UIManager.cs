using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject fieldOfViewPrefab;

    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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
