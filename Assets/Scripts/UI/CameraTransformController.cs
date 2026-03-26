using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CameraTransformController is a script to smoothly transition the camera's position to follow observed objects or allow mouse control when no objects are observed.
/// Zoom is controlled by mouse scrolling with min and max limits.
/// It subscribes to CameraFocusManager's OnObservedObjectsChanged event for smooth transitions.
/// </summary>
public class CameraTransformController : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 1f; // Duration of transition to new target
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Curve for smooth transition
    [SerializeField] private Vector3 offset; // Offset from the target position
    [SerializeField] private float minZoom = 1f; // Minimum camera zoom size
    [SerializeField] private float maxZoom = 20f; // Maximum camera zoom size
    [SerializeField] private float zoomSpeed = 5f; // Speed of zoom change with mouse scroll
    [SerializeField] private float mouseMoveSpeed = 10f; // Speed of camera movement when controlled by mouse

    private Camera mainCamera;
    private Vector3 startPosition; // Starting position for transition
    private float transitionProgress; // Progress of the current transition (0 to 1)
    private bool isTransitioning; // Whether a transition is active
    private Vector3 targetPosition;
    private bool wasObserving = false; // Track if we were observing objects last frame

    private void Start()
    {
        mainCamera = Camera.main;
        CameraFocusManager.Instance.OnObservedObjectsChanged += OnObservedObjectsChanged;
    }

    private void OnDestroy()
    {
        if (CameraFocusManager.Instance != null) CameraFocusManager.Instance.OnObservedObjectsChanged -= OnObservedObjectsChanged;
    }

    private void OnObservedObjectsChanged()
    {
        // Start transition when observed objects change
        StartTransition();
    }

    private void StartTransition()
    {
        startPosition = transform.position;
        transitionProgress = 0f;
        isTransitioning = true;
    }

    private void LateUpdate()
    {
        List<GameObject> observed = CameraFocusManager.Instance.observedObjects;
        bool isObserving = observed.Count > 0;

        if (isObserving)
        {
            // Calculate target position
            targetPosition = CalculateAveragePosition(observed) + offset;

            if (!wasObserving)
            {
                // Just started observing, start transition
                StartTransition();
            }
        }
        else
        {
            // No objects, handle mouse control
            HandleMouseMovement();
        }

        wasObserving = isObserving;

        if (isTransitioning)
        {
            // Update transition progress
            transitionProgress += Time.deltaTime / transitionDuration;
            float t = transitionCurve.Evaluate(transitionProgress);

            // Lerp position
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // End transition when complete
            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                transitionProgress = 1f;
            }
        }
        else if (isObserving)
        {
            // Directly follow if not transitioning
            transform.position = targetPosition;
        }

        // Handle zoom with mouse scroll
        HandleZoom();
    }

    private Vector3 CalculateAveragePosition(List<GameObject> objects)
    {
        Vector3 sum = Vector3.zero;
        foreach (GameObject obj in objects)
        {
            sum += obj.transform.position;
        }
        return sum / objects.Count;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;
        
        float oldSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize -= scroll * zoomSpeed;
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);

        if (mainCamera.orthographicSize != oldSize) CameraInformation.Instance.NotifyZoomChanged(mainCamera.orthographicSize);
    }

    private void HandleMouseMovement()
    {
        if (CameraInformation.Instance.IsMouseAtEdge())
        {
            Vector2 moveDir = CameraInformation.Instance.GetMouseEdgeDirection();
            float speedFactor = CameraInformation.Instance.GetMouseEdgeSpeedFactor();
            Vector3 move = new Vector3(moveDir.x, moveDir.y, 0) * mouseMoveSpeed * speedFactor * Time.deltaTime;
            transform.position += move;
        }
    }
}