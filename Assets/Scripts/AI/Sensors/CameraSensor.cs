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
    [SerializeField] bool debug = false;
    [Header("Sense Types")]
    [SerializeField] HasEntityType.EntityType[] detectTypes;
    [SerializeField] bool detectMouse = false;
    [Header("Instances")]
    [SerializeField] Transform sensorViewPoint;

    public EntityTeam.Team team => entityTeam.team;
    
    private ProgressBar fovScript;
    private EntityTeam entityTeam;
    private Cone visionCone;

    void Start()
    {
        entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        if (!entityTeam) Debug.LogError("CameraSensor could not find EntityTeam on parent!");
        if (displayFOV) UIManager.Instance.InstantiateFieldOfView(sensorViewPoint.gameObject, range, fov, true, fov/2);
        visionCone = GetVisionCone();
    }

    public void SetFOV(float newFov)
    {
        fov = newFov;
        visionCone = GetVisionCone();
    }

    public void SetRange(float newRange)
    {
        range = newRange;
        visionCone = GetVisionCone();
    }

    private Cone GetVisionCone()
    {
        Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
        return new Cone(sensorViewPoint.transform.position, dir, fov, range);
    }
    public List<GameObject> GetVisibleEnemies()
    {
        List<GameObject> visibleObjects = visionCone.GetVisibleEnemiesInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
        // If mouse detection is enabled, check if the mouse is in the vision cone. If it is, make it the only target.
        HandleMouseDetection(visibleObjects);

        if (debug) visibleObjects.ForEach(enemy => Debug.DrawLine(transform.position, enemy.transform.position, Color.red));
        return visibleObjects;
    }
    private void HandleMouseDetection(List<GameObject> visibleObjects)
    {
        if (detectMouse)
        {
            Vector2 mousePosition = GeometryUtils.GetMousePosition();
            if (visionCone.IsPositionInCone(mousePosition)) visibleObjects = new List<GameObject> { EntityCounter.Instance.MouseCursor };
        }
    }
    public List<GameObject> GetVisibleAllies()
    {
        List<GameObject> visibleObjects = visionCone.GetVisibleAlliesInCone(team, GeometryUtils.SensorType.Camera);
        return EntityCounter.Instance.FilterEntityTypes(visibleObjects, new List<HasEntityType.EntityType>(detectTypes));
    }
    public List<GameObject> GetVisibleObjects()
    {
        List<GameObject> visibleObjects = visionCone.GetVisibleObjectsInCone(team, GeometryUtils.SensorType.Camera);
        return EntityCounter.Instance.FilterEntityTypes(visibleObjects, new List<HasEntityType.EntityType>(detectTypes));
    }
    public GameObject GetClosestVisibleEnemy() => visionCone.GetClosestVisibleEnemyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    public GameObject GetClosestVisibleAlly() => visionCone.GetClosestVisibleAllyInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
    public GameObject GetClosestVisibleObject() => visionCone.GetClosestVisibleObjectInCone(team, detectTypes, GeometryUtils.SensorType.Camera);
}
