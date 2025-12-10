using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSensor : MonoBehaviour, ISensor
{


    [Header("Camera settings")]
    [SerializeField][Tooltip("If false, the FieldOfView cone will never become visible")] private bool displayFOV;
    [SerializeField] private float range = 10f;
    [SerializeField] [Tooltip("Angle of vision in degrees")] private float fov = 60f;
    [SerializeField] bool debug = false;
    [SerializeField] HasEntityType.EntityType[] detectTypes;
    [Header("Instances")]
    [SerializeField] GameObject fieldOfViewPrefab;
    [SerializeField] Transform sensorViewPoint;

    private ProgressBar fovScript;
    private EntityTeam entityTeam;
    public EntityTeam.Team team => entityTeam.team;
    void Start()
    {
        entityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        if (!entityTeam) Debug.LogError("CameraSensor could not find EntityTeam on parent!");
        if (displayFOV) InstantiateFieldOfView();
    }

    private void InstantiateFieldOfView()
    {
        GameObject instance = Instantiate(fieldOfViewPrefab);
        instance.transform.SetParent(EntityCounter.Instance.canvas.gameObject.transform, false);
        fovScript = instance.GetComponent<ProgressBar>();
        fovScript.SetScale(range); // We assume that every ProgressBar is scaled to 1 unit by default
        fovScript.SetProgress(fov / 360f); // Normalize FOV to [0, 1] range
        ObjectFollower follower = instance.GetComponent<ObjectFollower>();
        follower.Follow(sensorViewPoint.gameObject, true, 0);
        follower.SetDeltaAngle(fov/2); // Rotate to face forward
    }
    public GameObject[] GetVisibleEnemies()
    {
        Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
        GeometryUtils.Cone cone = new GeometryUtils.Cone(sensorViewPoint.transform.position, dir, fov, range);
        GameObject[] visibleObjects = GeometryUtils.GetVisibleEnemiesInCone(cone, team, detectTypes, GeometryUtils.SensorType.Camera);
        if (debug)
        {
            foreach (GameObject enemy in visibleObjects)
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.red);
            }
        }
        return visibleObjects;
    }
    public GameObject[] GetVisibleAllies()
    {
        Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
        GeometryUtils.Cone cone = new GeometryUtils.Cone(sensorViewPoint.transform.position, dir, fov, range);
        GameObject[] visibleObjects = GeometryUtils.GetVisibleAlliesInCone(cone, team, GeometryUtils.SensorType.Camera);
        return EntityCounter.Instance.FilterEntityTypes(visibleObjects, detectTypes);
    }
    public GameObject[] GetVisibleObjects()
    {
        Vector2 dir = GeometryUtils.AngleToDirectionVector(sensorViewPoint.transform.rotation.eulerAngles.z);
        GeometryUtils.Cone cone = new GeometryUtils.Cone(sensorViewPoint.transform.position, dir, fov, range);
        GameObject[] visibleObjects = GeometryUtils.GetVisibleObjectsInCone(cone, team, GeometryUtils.SensorType.Camera);
        return EntityCounter.Instance.FilterEntityTypes(visibleObjects, detectTypes);
    }
}
