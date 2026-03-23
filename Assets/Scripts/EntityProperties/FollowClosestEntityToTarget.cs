using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FollowClosestEntityToTarget : MonoBehaviour
{
    [SerializeField] bool followMouseInstead;

    [SerializeField] private List<EntityTypeProperty.EntityType> targetTypesToFollow = new List<EntityTypeProperty.EntityType>();
    private GameObject parentObject;
    private GameObject currentTarget;
    private EntityTeam parentEntityTeam;
    private SpriteRenderer spriteRenderer;

    #region Initialization
    private void Start()
    {
        parentEntityTeam = TeamManager.Instance.GetParentEntityTeam(gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        FindObjectToFollow();
    }
    private void FindObjectToFollow()
    {
        if (followMouseInstead)
        {
            parentObject = EntityCounter.Instance.MouseCursor;
        }
    }
    #endregion

    #region Update
    void Update()
    {
        FindTarget();
        FollowTarget();
    }
    private void FindTarget()
    {
        if (parentObject != null)
        {
            spriteRenderer.enabled = true;
            float cameraRadius = CameraInformation.Instance.GetCameraSize().magnitude;
            List<GameObject> potentialTargets = TeamManager.Instance.GetNearbyEnemies(transform.position, parentEntityTeam.team, targetTypesToFollow, cameraRadius);
            if (potentialTargets.Count > 0) currentTarget = GeometryUtils.FindClosestEntityToObject(potentialTargets, parentObject);
        }
    }
    private void FollowTarget()
    {
        if (currentTarget) transform.position = currentTarget.transform.position;
    }
    #endregion

    #region Mutator Methods
    /// <summary>
    /// Will set the object to follow to the given object, and will stop following the mouse if it was previously set to do so.
    /// Then, will teleport to the closest entity to the object to follow.
    /// </summary>
    /// <param name="newObj"></param>
    public void SetObjectToFollow(GameObject newObj)
    {
        parentObject = newObj;
        followMouseInstead = false;
    }
    public void SetFollowMouse(bool set)
    {
        followMouseInstead = set;
        parentObject = null;
        FindObjectToFollow();
    }
    public void SetTargetTypesToFollow(List<EntityTypeProperty.EntityType> types)
    {
        targetTypesToFollow = types;
    }
    #endregion
}
