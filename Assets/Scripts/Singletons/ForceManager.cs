using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.PlayerLoop;

public class ForceManager : MonoBehaviour
{
    public enum ForceType
    {
        Gravity,
        Magnetism,
        ForceField,
    }
    public enum ForceFalloffType
    {
        Linear,
        Quadratic,
        CustomCurve,
    }
    // Singleton instance
    public static ForceManager Instance { get; private set; }

    // Should only track active entities
    private Dictionary<ForceType, HashSet<GameObject>> simulatedEntities = new Dictionary<ForceType, HashSet<GameObject>>();

    [SerializeField] bool showDebugForces = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize dictionary for all entity types
        foreach (ForceType type in System.Enum.GetValues(typeof(ForceType)))
        {
            simulatedEntities[type] = new HashSet<GameObject>();
        }
    }
    private void FixedUpdate()
    {
        // Handle each force type separately
        foreach (ForceType type in System.Enum.GetValues(typeof(ForceType)))
        {
            HandleForceCategory(type);
        }
    }

    private void HandleForceCategory(ForceType type)
    {
        List<GameObject> appliers = simulatedEntities[type].Where(go => go.GetComponent<ForceProperty>().forceApplier).ToList();
        
        if(appliers.Count == 0)
        {
            return;
        }

        //Debug.Log("Found " + appliers.Count + " appliers and " + receivers.Count + " receivers for force type " + type);
        appliers = appliers.Distinct().ToList();

        ApplyForces(appliers);
    }

    private void ApplyForces(List<GameObject> appliers)
    {
        foreach (GameObject applier in appliers)
        {
            ForceProperty applierFp = applier.GetComponent<ForceProperty>();
            Vector2 applierPos = applier.transform.position;
            float applierMaxRange = applierFp.maxForceApplyRange;

            // Get nearby receivers using spatial query
            var nearbyReceivers = EntityCounter.Instance.GetNearbyForceEntities(applierFp.forceType, applierPos, applierMaxRange)
                .Where(go => go.GetComponent<ForceProperty>().forceReceiver)
                .ToList();
            foreach (GameObject receiver in nearbyReceivers)
            {
                Rigidbody2D rb = receiver.GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                Vector3 direction = applier.transform.position - receiver.transform.position;
                float distance = direction.magnitude;

                bool isOutOfRange = distance > applierMaxRange || distance == 0;
                if (isOutOfRange) continue;

                direction.Normalize();

                bool isRepelling = applierFp.maxForceValue < 0;
                if (isRepelling)
                {
                    direction = -direction;
                }

                float forceMagnitude = CalculateForce(Mathf.Abs(applierFp.maxForceValue), distance, applierMaxRange, applierFp.forceFalloffType, applierFp.forceFalloffCurve);
                rb.AddForce(direction * forceMagnitude * Time.fixedDeltaTime);

                if (showDebugForces)
                {
                    Color forceColor = applierFp.maxForceValue > 0 ? Color.green : Color.red;
                    Vector3 endPoint = receiver.transform.position + direction * forceMagnitude * 0.01f; // Scale for visibility
                    Debug.DrawLine(receiver.transform.position, endPoint, forceColor);
                }
            }
        }
    }

    private float CalculateForce(float maxForce, float distance, float maxRange, ForceFalloffType falloffType, AnimationCurve curve)
    {
        float t = distance / maxRange;
        float multiplier = 0;

        switch (falloffType)
        {
            case ForceFalloffType.Linear:
                multiplier = 1 - t;
                break;
            case ForceFalloffType.Quadratic:
                multiplier = 1 - t * t;
                break;
            case ForceFalloffType.CustomCurve:
                if (curve != null)
                {
                    multiplier = curve.Evaluate(t);
                }
                else
                {
                    multiplier = 1 - t;
                }
                break;
        }

        return maxForce * multiplier;
    }

    public void RegisterEntity(GameObject entity, ForceType type)
    {
        if (simulatedEntities.ContainsKey(type))
        {
            simulatedEntities[type].Add(entity);
            EntityCounter.Instance.RegisterForceEntity(entity, type);
        }
    }
    public void UnregisterEntity(GameObject entity) {
        foreach (var key in simulatedEntities.Keys)
        {
            if (simulatedEntities[key].Contains(entity))
            {
                simulatedEntities[key].Remove(entity);
                EntityCounter.Instance.UnregisterForceEntity(entity, key);
            }
        }
    }
}
