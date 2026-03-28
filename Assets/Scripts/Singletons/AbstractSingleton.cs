using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSingleton<T> : MonoBehaviour where T : AbstractSingleton<T>
{
    private static T instance;
    private static bool isDestroyed = false;

    public static T Instance
    {
        get
        {
            if (isDestroyed) return null;
            if (instance == null)
            {
                //Debug.LogWarning($"Singleton instance of {typeof(T).Name} not found. Creating a new one.");
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    private void OnDestroy()
    {
        isDestroyed = true;
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
            //Debug.Log($"Singleton instance of {typeof(T).Name} created.");
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
