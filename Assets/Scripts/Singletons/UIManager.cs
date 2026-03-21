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
    /// Compatibility wrapper for existing code that used the old FOV-specific factory.
    /// Calls the new generic InstantiateUI and returns the ProgressBar component when available.
    /// </summary>
    public ProgressBar InstantiateFieldOfView(GameObject followObj, float radius, float arc = 360f, float deltaAngle = 0f, bool rotateWithParent = true)
    {
        GameObject go = InstantiateUI(UITypes.FieldOfView, followObj, radius, arc, 360f, rotateWithParent, deltaAngle);
        if (go == null) return null;
        Debug.Log($"Instantiated Field of View UI element: {go.name}");
        return go.GetComponent<ProgressBar>();
    }
    public void DestroyFieldOfView(GameObject fovObj)
    {
        if (fovObj == null) Debug.LogError("UIManager: Attempted to destroy null FOV object!");
        RemoveFromCache(UITypes.FieldOfView, fovObj);
        Debug.Log($"Destroying Field of View UI element: {fovObj.name}");
        Destroy(fovObj);
    }
    public ProgressBar InstantiateHealthBar(GameObject followObj, float scale, float maxHealth)
    {
        GameObject go = InstantiateUI(UITypes.HealthBar, followObj, scale, maxHealth, maxHealth, false, 0);
        if (go == null) return null;
        return go.GetComponent<ProgressBar>();
    }
    public void DestroyHealthBar(GameObject healthBarObj)
    {
        if (healthBarObj == null) Debug.LogError("UIManager: Attempted to destroy null Health Bar object!");
        RemoveFromCache(UITypes.HealthBar, healthBarObj);
        Destroy(healthBarObj);
    }
    public ProgressBar InstantiateAmmoBar(GameObject followObj, float scale, float maxHealth)
    {
        GameObject go = InstantiateUI(UITypes.AmmoBar, followObj, scale, maxHealth, maxHealth, false, 0);
        if (go == null) return null;
        return go.GetComponent<ProgressBar>();
    }
    public void DestroyAmmoBar(GameObject ammoBarObj)
    {
        if (ammoBarObj == null) Debug.LogError("UIManager: Attempted to destroy null Ammo Bar object!");
        RemoveFromCache(UITypes.AmmoBar, ammoBarObj);
        Destroy(ammoBarObj);
    }

    /// <summary>
    /// Factory method: instantiate a UI element of the given type and cache it.
    /// If the prefab contains an ObjectFollower or ProgressBar, this will apply the provided parameters.
    /// </summary>
    private GameObject InstantiateUI(UITypes type, GameObject followObj, float scale = 1f, float progress = 360f, float maxValue=360f, bool rotateWithParent = true, float deltaAngle = 180f)
    {
        GameObject prefab = GetPrefab(type);
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab);
        if (EntityCounter.Instance != null && EntityCounter.Instance.canvas != null)
            instance.transform.SetParent(EntityCounter.Instance.canvas.gameObject.transform, false);
        SetupObjectFollower(instance, followObj, rotateWithParent, deltaAngle);
        SetupProgressBar(instance, scale, progress, maxValue);
        AddToCache(type, instance);
        return instance;
    }
    private GameObject GetPrefab(UITypes type)
    {
        if (!PrefabByType.TryGetValue(type, out GameObject prefab) || prefab == null)
        {
            Debug.LogError($"UIManager: No prefab assigned for UI type {type}");
            return null;
        }
        return prefab;
    }
    private void SetupObjectFollower(GameObject instance, GameObject followObj, bool rotateWithParent, float deltaAngle)
    {
        ObjectFollower objectFollower = instance.GetComponent<ObjectFollower>();
        if (objectFollower != null && followObj != null)
        {
            // preserve original behavior (call both overloads if available)
            objectFollower.Follow(followObj);
            objectFollower.Follow(followObj, rotateWithParent, deltaAngle);
            objectFollower.SetDeltaAngle(deltaAngle);
        }
    }
    private void SetupProgressBar(GameObject instance, float scale, float progress, float maxValue)
    {
        ProgressBar barScript = instance.GetComponent<ProgressBar>();
        if (barScript != null)
        {
            barScript.SetScale(scale);
            barScript.SetProgress(progress / maxValue);
        }
    }
    private void AddToCache(UITypes type, GameObject instance)
    {
        if (!cachedUI.TryGetValue(type, out var set))
        {
            set = new HashSet<GameObject>();
            cachedUI[type] = set;
        }
        set.Add(instance);
    }
    private void RemoveFromCache(UITypes type, GameObject instance)
    {
        if (cachedUI.TryGetValue(type, out var set))
        {
            set.Remove(instance);
        }
    }
}
