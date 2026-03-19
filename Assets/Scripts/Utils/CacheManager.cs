using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CacheBehavior
{
    EndOfUpdate,
    Interval
}

public abstract class CacheBase
{
    public CacheBehavior behavior;
    public float interval;
    public float lastResetTime;

    public CacheBase(CacheBehavior b, float i)
    {
        behavior = b;
        interval = i;
        lastResetTime = Time.time;
    }

    public abstract void Reset();
}

public class Cache<T> : CacheBase
{
    public T cachedValue;
    public bool isCached;

    public Cache(CacheBehavior b, float i) : base(b, i)
    {
    }

    public void Set(T value)
    {
        cachedValue = value;
        isCached = true;
    }

    public T Get()
    {
        if (!isCached) throw new System.Exception("Cache not set");
        return cachedValue;
    }

    public override void Reset()
    {
        isCached = false;
        lastResetTime = Time.time;
    }
}

public class CacheManager : MonoBehaviour
{
    public static CacheManager Instance { get; private set; }

    private List<CacheBase> caches = new List<CacheBase>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        foreach (var cache in caches)
        {
            if (cache.behavior == CacheBehavior.EndOfUpdate)
            {
                cache.Reset();
            }
            else if (cache.behavior == CacheBehavior.Interval && Time.time - cache.lastResetTime >= cache.interval)
            {
                cache.Reset();
            }
        }
    }

    public Cache<T> CreateCache<T>(CacheBehavior behavior, float interval = 0f)
    {
        var cache = new Cache<T>(behavior, interval);
        caches.Add(cache);
        return cache;
    }
}
