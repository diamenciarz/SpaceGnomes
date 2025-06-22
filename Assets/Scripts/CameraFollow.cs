using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The object to follow
    [SerializeField] private float transitionDuration = 1f; // Duration of transition to new target
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Curve for smooth transition
    [SerializeField] private Vector3 offset; // Offset from the target position

    private Vector3 startPosition; // Starting position for transition
    private float transitionProgress; // Progress of the current transition (0 to 1)
    private bool isTransitioning; // Whether a transition to a new target is active
    private Transform previousTarget; // Previous target before transition started

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (isTransitioning)
        {
            // Update transition progress
            transitionProgress += Time.deltaTime / transitionDuration;
            float t = transitionCurve.Evaluate(transitionProgress);

            // Lerp between start position and new target position
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // End transition when complete
            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                transitionProgress = 1f;
            }
        }
        else
        {
            // Directly follow the target
            transform.position = target.position + offset;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == target)
            return;

        // Start a new transition
        previousTarget = target;
        target = newTarget;
        isTransitioning = true;
        transitionProgress = 0f;

        // Set start position for transition
        if (previousTarget != null)
        {
            startPosition = previousTarget.position + offset;
        }
        else
        {
            startPosition = transform.position;
        }
    }
}