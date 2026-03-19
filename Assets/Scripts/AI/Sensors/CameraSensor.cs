using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class CameraSensor : MonoBehaviour, ISensor
{


    [Header("Camera settings")]
    [SerializeField][Tooltip("If false, the FieldOfView cone will never become visible")] private bool displayFOV;
    [SerializeField] private float range = 10f;
    [SerializeField] [Tooltip("Angle of vision in degrees")] private float fov = 60f;
    [Header("Sense Types")]
    [SerializeField] HasEntityType.EntityType[] detectTypes;
    [SerializeField] bool detectMouse = false;
    [Header("Instances")]
    [SerializeField] Transform sensorViewPoint;

    public EntityTeam.Team team => entityTeam.team;
    
    private ProgressBar fovScript;
    private EntityTeam entityTeam;

    void Start()
    {
        entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        if (!entityTeam) Debug.LogError("CameraSensor could not find EntityTeam on parent!");
        if (displayFOV) UIManager.Instance.InstantiateFieldOfView(sensorViewPoint.gameObject, range, fov, true, fov/2);
    }

    public void SetFOV(float newFov)
    {
        fov = newFov;
    }

    public void SetRange(float newRange)
    {
        range = newRange;
    }

    /*
     * <summary>The cone must be recalculated every frame because the sensor can rotate.</summary>
     */
    private Cone GetVisionCone()
    {
        Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
        return new Cone(sensorViewPoint.transform.position, dir, fov, range);
    }
    private bool DetectMouse(out GameObject mouseFollower)
    {
        mouseFollower = null;
        if (detectMouse)
        {
            Vector2 mousePosition = GeometryUtils.GetMousePosition();
            if (GetVisionCone().IsPositionInCone(mousePosition)) mouseFollower = EntityCounter.Instance.MouseCursor;
            return true;
        }
        return false;
    }
    public List<GameObject> GetVisibleEnemies()
    {
        // If mouse detection is enabled, check if the mouse is in the vision cone. If it is, make it the only target.
        if (DetectMouse(out GameObject mouseFollower)) return new List<GameObject> { mouseFollower };
        return GetVisionCone().GetVisibleEnemiesInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    }
    public List<GameObject> GetVisibleAllies()
    {
        return GetVisionCone().GetVisibleAlliesInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    }
    public List<GameObject> GetVisibleObjects()
    {
        List<GameObject> visibleObjects = GetVisionCone().GetVisibleObjectsInCone(team, GeometryUtils.SensorType.Camera);
        return EntityCounter.Instance.FilterEntityTypes(visibleObjects, new List<HasEntityType.EntityType>(detectTypes));
    }
    public GameObject GetClosestVisibleEnemy()
    {
        // If mouse detection is enabled, check if the mouse is in the vision cone. If it is, make it the only target.
        if (DetectMouse(out GameObject mouseFollower)) return mouseFollower;
        return GetVisionCone().GetClosestVisibleEnemyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    }
    public GameObject GetClosestVisibleAlly() => GetVisionCone().GetClosestVisibleAllyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    public GameObject GetClosestVisibleObject() => GetVisionCone().GetClosestVisibleObjectInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
}
