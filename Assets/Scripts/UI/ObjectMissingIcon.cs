using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMissingIcon : MonoBehaviour
{

    [Tooltip("Fraction of the full size")]
    [SerializeField] [Range(0.1f, 1)] float minimumSpriteSize = 0.4f;
    [Tooltip("If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear")]
    [SerializeField] float hideDistance = 6f;

    [Tooltip("The full size of the sprite. This will be used in the transform of the game object")]
    [SerializeField] float spriteScale = 1;
    [Tooltip("Delta position from the screen edge to regard the icon as outside the screen space")]
    [SerializeField] float screenEdgeOffset = 0.5f;
    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;
    [SerializeField] Color neutralColor;


    //Private variables
    private GameObject objectToFollow;
    private Camera mainCamera;
    private SpriteRenderer[] mySpriteRenderers;
    private EntityTeam followedObjectTeam;
    private bool isVisible = true;
    private bool isDestroyed = false;

    #region Initialization
    void Start()
    {
        SetupStartingVariables();
        UpdateSpriteColor();
        SetSpriteVisibility(false);
    }
    private void SetupStartingVariables()
    {
        mySpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        mainCamera = Camera.main;
        followedObjectTeam = TeamManager.Instance.GetParentEntityTeam(objectToFollow);
    }
    #endregion

    #region Mutator Methods
    public void SetObjectToFollow(GameObject followThis)
    {
        if (followThis == null)
        {
            Destroy(gameObject);
        }
        objectToFollow = followThis;
    }
    private void UpdateSpriteColor()
    {
        if (followedObjectTeam == null)
        {
            return;
        }
        if (TeamManager.Instance.IsAlly(followedObjectTeam.team, EntityTeam.playerTeam))
        {
            GetComponent<SpriteRenderer>().color = allyColor;
            return;
        }
        if (TeamManager.Instance.IsEnemy(followedObjectTeam.team, EntityTeam.playerTeam))
        {
            GetComponent<SpriteRenderer>().color = enemyColor;
            return;
        }
        if (followedObjectTeam.team == EntityTeam.Team.Neutral)
        {
            GetComponent<SpriteRenderer>().color = neutralColor;
            return;
        }
    }
    #endregion

    #region Update
    void Update()
    {
        if (!isDestroyed)
        {
            UpdateVisibility();
            UpdateTransform();

            CheckDestroy();
        }
    }
    private void CheckDestroy()
    {
        if (objectToFollow == null)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    #region Transform
    private void UpdateTransform()
    {
        if (!objectToFollow)
        {
            return;
        }
        if (isVisible)
        {
            UpdateRotation();
            UpdatePosition();
            UpdateScale();
        }
    }
    private void UpdateRotation()
    {
        transform.rotation = objectToFollow.transform.rotation;
    }
    private void UpdatePosition()
    {
        Vector2 newPosition = CameraInformation.Instance.ClampPositionOnScreen(objectToFollow.transform.position, screenEdgeOffset);
        transform.position = new Vector3(newPosition.x, newPosition.y, 1);
    }
    private void UpdateScale()
    {
        float distanceToObject = Mathf.Abs((transform.position - objectToFollow.transform.position).magnitude);
        //Sets the new scale ilamped in between <0,4;1>
        float newScale = (1 - (Mathf.Clamp((distanceToObject / hideDistance), 0, 1f) * (1 - minimumSpriteSize))) * spriteScale;
        Vector3 newScaleVector3 = new Vector3(newScale, newScale, 0);

        transform.localScale = newScaleVector3;
    }
    #endregion

    #region Visibility
    private void UpdateVisibility()
    {
        if (!objectToFollow)
        {
            return;
        }
        if (CameraInformation.Instance.IsPositionOnScreen(objectToFollow.transform.position, screenEdgeOffset))
        {
            SetSpriteVisibility(false);
            return;
        }
        UpdatePosition();
        //If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear
        float distanceToFollowedObject = (transform.position - objectToFollow.transform.position).magnitude;
        bool objectIsTooFar = (distanceToFollowedObject / hideDistance > 1f);
        if (objectIsTooFar)
        {
            SetSpriteVisibility(false);
        }
        else
        {
            SetSpriteVisibility(true);
        }
    }

    private void SetSpriteVisibility(bool setVisibility)
    {
        if (isVisible == setVisibility)
        {
            return;
        }

        isVisible = setVisibility;
        foreach (SpriteRenderer sprite in mySpriteRenderers)
        {
            sprite.enabled = isVisible;
        }
    }
    #endregion
    #endregion
}
