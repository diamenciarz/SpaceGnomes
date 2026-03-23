using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public class CameraSensor : AbstractSensor
{
    [Header("Camera settings")]
    [SerializeField][Tooltip("If false, the FieldOfView cone will never become visible")] private bool displayFOV;
    [SerializeField] private float range = 10f;
    [SerializeField] [Tooltip("Angle of vision in degrees")] private float fov = 60f;
    [Header("Sense Types")]
    [SerializeField] EntityTypeProperty.EntityType[] detectTypes;
    [SerializeField] bool detectMouse = false;
    [Header("Instances")]
    [SerializeField] Transform sensorViewPoint;

    public EntityTeam.Team team => entityTeam.team;
    
    private ProgressBar fovScript;
    private EntityTeam entityTeam;

    private struct MouseResult
    {
        public bool detected;
        public GameObject follower;
    }

    // Cache instances
    private Cache<Cone> coneCache;
    private Cache<MouseResult> mouseCache;
    private Cache<List<GameObject>> visibleEnemiesCache;
    private Cache<List<GameObject>> visibleAlliesCache;
    private Cache<List<GameObject>> visibleObjectsCache;
    private Cache<GameObject> closestEnemyCache;
    private Cache<GameObject> closestAllyCache;
    private Cache<GameObject> closestObjectCache;

    private void OnEnable()
    {
        if(fovScript) fovScript.gameObject.SetActive(displayFOV);
    }
    private void OnDisable()
    {
        if(fovScript) fovScript.gameObject.SetActive(false);
    }
    void Start()
    {
        entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        if (!entityTeam) Debug.LogError("CameraSensor could not find EntityTeam on parent!");
        if (displayFOV) fovScript = UIManager.Instance.InstantiateFieldOfView(sensorViewPoint.gameObject, range, fov, fov / 2, true);
        // Initialize caches
        coneCache = CacheManager.Instance.CreateCache<Cone>(CacheBehavior.EndOfUpdate);
        mouseCache = CacheManager.Instance.CreateCache<MouseResult>(CacheBehavior.EndOfUpdate);
        visibleEnemiesCache = CacheManager.Instance.CreateCache<List<GameObject>>(CacheBehavior.EndOfUpdate);
        visibleAlliesCache = CacheManager.Instance.CreateCache<List<GameObject>>(CacheBehavior.EndOfUpdate);
        visibleObjectsCache = CacheManager.Instance.CreateCache<List<GameObject>>(CacheBehavior.EndOfUpdate);
        closestEnemyCache = CacheManager.Instance.CreateCache<GameObject>(CacheBehavior.EndOfUpdate);
        closestAllyCache = CacheManager.Instance.CreateCache<GameObject>(CacheBehavior.EndOfUpdate);
        closestObjectCache = CacheManager.Instance.CreateCache<GameObject>(CacheBehavior.EndOfUpdate);
    }
    public void SetFOV(float newFov)
    {
        fov = newFov;
    }
    private void Update()
    {
        //GetVisionCone().DebugDisplayCone(Color.blue);

    }
    public void SetRange(float newRange)
    {
        range = newRange;
    }
    /// <summary>
    /// The cone must be recalculated every frame because the sensor can rotate.
    /// </summary>
    public Cone GetVisionCone()
    {
        if (!coneCache.isCached)
        {
            Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
            coneCache.Set(new Cone(sensorViewPoint.transform.position, dir, fov, range));
        }
        return coneCache.Get();
    }
    /// <summary>
    /// If the sensor is set to detect the mouse, it will check if the mouse is within its vision cone.
    /// If it is, it will return the mouse as the only visible enemy. Otherwise, it will return the enemies detected by the vision cone as usual.
    /// </summary>
    /// <returns></returns>
    public override List<GameObject> GetVisibleEnemies()
    {
        if (!visibleEnemiesCache.isCached)
        {
            List<GameObject> result;
            if (DetectMouse(out GameObject mouseFollower))
            {
                result = new List<GameObject> { mouseFollower };
            }
            else
            {
                result = GetVisionCone().GetVisibleEnemiesInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            }
            visibleEnemiesCache.Set(result);
        }
        return visibleEnemiesCache.Get();
    }

    public override List<GameObject> GetVisibleAllies()
    {
        if (!visibleAlliesCache.isCached)
        {
            List<GameObject> result = GetVisionCone().GetVisibleAlliesInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            visibleAlliesCache.Set(result);
        }
        return visibleAlliesCache.Get();
    }

    public override List<GameObject> GetVisibleObjects()
    {
        if (!visibleObjectsCache.isCached)
        {
            List<GameObject> result = GetVisionCone().GetVisibleObjectsInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            visibleObjectsCache.Set(result);
        }
        return visibleObjectsCache.Get();
    }
    /// <summary>
    /// If the sensor is set to detect the mouse, it will check if the mouse is within its vision cone.
    /// If it is, it will return the mouse as the closest visible enemy. Otherwise, it will return the closest enemy detected by the vision cone as usual.
    /// </summary>
    /// <returns></returns>
    public override GameObject GetClosestVisibleEnemy()
    {
        if (!closestEnemyCache.isCached)
        {
            GameObject result;
            if (DetectMouse(out GameObject mouseFollower))
            {
                result = mouseFollower;
            }
            else
            {
                result = GetVisionCone().GetClosestVisibleEnemyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            }
            closestEnemyCache.Set(result);
        }
        return closestEnemyCache.Get();
    }

    public override GameObject GetClosestVisibleAlly()
    {
        if (!closestAllyCache.isCached)
        {
            GameObject result = GetVisionCone().GetClosestVisibleAllyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            closestAllyCache.Set(result);
        }
        return closestAllyCache.Get();
    }

    public override GameObject GetClosestVisibleObject()
    {
        if (!closestObjectCache.isCached)
        {
            GameObject result = GetVisionCone().GetClosestVisibleObjectInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
            closestObjectCache.Set(result);
        }
        return closestObjectCache.Get();
    }
    private bool DetectMouse(out GameObject mouseFollower)
    {
        if(team != EntityTeam.playerTeam)
        {
            mouseFollower = null;
            return false; // Player's own camera sensor should not detect the mouse as an enemy
        }
        if (!mouseCache.isCached)
        {
            bool detected = false;
            GameObject follower = null;
            if (detectMouse)
            {
                Vector2 mousePosition = GeometryUtils.GetMousePosition();
                if (GetVisionCone().IsPositionInCone(mousePosition))
                {
                    follower = EntityCounter.Instance.MouseCursor;
                    detected = true;
                }
            }
            mouseCache.Set(new MouseResult { detected = detected, follower = follower });
        }
        MouseResult res = mouseCache.Get();
        mouseFollower = res.follower;
        return res.detected;
    }
}
