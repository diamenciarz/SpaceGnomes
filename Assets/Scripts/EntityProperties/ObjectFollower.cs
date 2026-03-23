using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    [SerializeField] private Vector3 deltaPosition = Vector3.zero;
    [SerializeField] private float deltaAngle = 0f;
    [SerializeField] private bool rotateWithParent = false;

    private GameObject followedObject;
    private RectTransform rectTransform;
    private bool isUI;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        isUI = rectTransform != null;
    }

    public void Follow(GameObject obj, bool rotateWithParent= true, float angle = 0f)
    {
        this.followedObject = obj;
        this.rotateWithParent = rotateWithParent;
        this.deltaAngle = angle;
    }

    public void SetDeltaAngle(float angle)
    {
        deltaAngle = angle;
    }

    private void LateUpdate()
    {
        if (followedObject != null)
        {
            // Calculate rotated delta position
            Vector3 rotatedDeltaPosition;
            if (rotateWithParent)
            {
                rotatedDeltaPosition = followedObject.transform.rotation * deltaPosition;
            }
            else
            {
                rotatedDeltaPosition = deltaPosition;
            }

            if (isUI)
            {
                // For UI, translate world position to screen position
                Vector3 worldPos = followedObject.transform.position + rotatedDeltaPosition;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                rectTransform.position = screenPos;
            }
            else
            {
                // Update position relative to followed object
                transform.position = followedObject.transform.position + rotatedDeltaPosition;
            }

            // Handle rotation (same for UI and game objects)
            if (rotateWithParent)
            {
                transform.rotation = followedObject.transform.rotation * Quaternion.Euler(0f, 0f, deltaAngle);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, deltaAngle);
            }
        }
    }
}
