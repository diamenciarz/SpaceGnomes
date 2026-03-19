using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum UITypes
{
    FieldOfView,
    AmmoBar,
    HealthBar
}

[System.Serializable]
public struct UIPrefabEntry
{
    public UITypes type;
    public GameObject prefab;
}

public class UIManager : AbstractSingleton<UIManager>
{
    [Tooltip("Assign prefabs for each UI type here. This list will be converted to an internal dictionary at runtime.")]
    public List<UIPrefabEntry> prefabEntries = new List<UIPrefabEntry>();

    /// <summary>
    /// Runtime dictionary mapping UI type -> prefab. Built from `prefabEntries` so you can edit assignments in the inspector.
    /// </summary>
    public Dictionary<UITypes, GameObject> PrefabByType { get; private set; } = new Dictionary<UITypes, GameObject>();

    /// <summary>
    /// Cache of instantiated UI elements per UI type.
    /// Keys: UITypes, Values: HashSet of instantiated GameObjects of that type.
    /// </summary>
    private Dictionary<UITypes, HashSet<GameObject>> cachedUI = new Dictionary<UITypes, HashSet<GameObject>>();

    public IReadOnlyDictionary<UITypes, HashSet<GameObject>> CachedUI => cachedUI;

    protected override void Awake()
    {
        base.Awake(); // Call base singleton setup
        BuildPrefabMap();

        // Ensure all enum keys exist in the cache to avoid null checks elsewhere.
        foreach (UITypes t in System.Enum.GetValues(typeof(UITypes)))
        {
            if (!cachedUI.ContainsKey(t))
                cachedUI[t] = new HashSet<GameObject>();
        }
    }

    private void BuildPrefabMap()
    {
        PrefabByType.Clear();
        if (prefabEntries == null) return;
        foreach (var e in prefabEntries)
        {
            if (e.prefab == null) continue;
            PrefabByType[e.type] = e.prefab;
        }
    }

    private void OnValidate()
    {
        // Keep runtime map in sync while editing in the inspector.
        if (PrefabByType == null) PrefabByType = new Dictionary<UITypes, GameObject>();
        BuildPrefabMap();
    }

    /// <summary>
    /// Factory method: instantiate a UI element of the given type and cache it.
    /// If the prefab contains an ObjectFollower or ProgressBar, this will apply the provided parameters.
    /// </summary>
    public GameObject InstantiateUI(UITypes type, GameObject followObj = null, float radius = 1f, float arc = 360f, bool rotateWithParent = true, float deltaAngle = 180f)
    {
        if (!PrefabByType.TryGetValue(type, out GameObject prefab) || prefab == null)
        {
            Debug.LogError($"UIManager: No prefab assigned for UI type {type}");
            return null;
        }

        GameObject instance = Instantiate(prefab);
        if (EntityCounter.Instance != null && EntityCounter.Instance.canvas != null)
            instance.transform.SetParent(EntityCounter.Instance.canvas.gameObject.transform, false);

        ObjectFollower objectFollower = instance.GetComponent<ObjectFollower>();
        if (objectFollower != null && followObj != null)
        {
            // preserve original behavior (call both overloads if available)
            objectFollower.Follow(followObj);
            objectFollower.Follow(followObj, rotateWithParent, deltaAngle);
            objectFollower.SetDeltaAngle(deltaAngle);
        }

        ProgressBar barScript = instance.GetComponent<ProgressBar>();
        if (barScript != null)
        {
            barScript.SetScale(radius);
            barScript.SetProgress(arc / 360f);
        }

        if (!cachedUI.TryGetValue(type, out var set))
        {
            set = new HashSet<GameObject>();
            cachedUI[type] = set;
        }
        set.Add(instance);

        return instance;
    }

    /// <summary>
    /// Compatibility wrapper for existing code that used the old FOV-specific factory.
    /// Calls the new generic InstantiateUI and returns the ProgressBar component when available.
    /// </summary>
    public ProgressBar InstantiateFieldOfView(GameObject followObj, float radius, float arc = 360f, bool rotateWithParent = true, float deltaAngle = 180f)
    {
        GameObject go = InstantiateUI(UITypes.FieldOfView, followObj, radius, arc, rotateWithParent, deltaAngle);
        if (go == null) return null;
        return go.GetComponent<ProgressBar>();
    }

    /// <summary>
    /// Destroy a UI instance and remove it from the cache if present.
    /// Returns true if the instance was found in the cache and destroyed.
    /// </summary>
    public bool DestroyUI(GameObject instance)
    {
        if (instance == null) return false;
        foreach (var kv in cachedUI)
        {
            if (kv.Value.Remove(instance))
            {
                Destroy(instance);
                return true;
            }
        }
        // Not found in cache, still destroy.
        Destroy(instance);
        return false;
    }

    /// <summary>
    /// Destroy a UI instance for a specific type.
    /// </summary>
    public bool DestroyUI(UITypes type, GameObject instance)
    {
        if (instance == null) return false;
        if (cachedUI.TryGetValue(type, out var set) && set.Remove(instance))
        {
            Destroy(instance);
            return true;
        }
        return false;
    }
}
