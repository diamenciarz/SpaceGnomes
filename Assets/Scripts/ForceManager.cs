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

    [SerializeField] [Range(0.1f, 1f)] float clumpThresholdFactor = 0.5f;
    [SerializeField] bool showDebugForces = false;

    private class Clump
    {
        public Vector3 position;
        public float forceValue;
        public float maxRange;
        public ForceFalloffType falloffType;
        public AnimationCurve falloffCurve;
    }
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
        List<GameObject> receivers = simulatedEntities[type].Where(go => go.GetComponent<ForceProperty>().forceReceiver).ToList();

        appliers = appliers.Distinct().ToList();
        receivers = receivers.Distinct().ToList();

        List<Clump> clumps = GroupIntoClumps(appliers);
        //if (type ==ForceType.Gravity)
        //{
        //    Debug.Log($"ForceManager: Handling {type} with {appliers.Count} appliers grouped into {clumps.Count} clumps affecting {receivers.Count} receivers.");
        //}
        ApplyForces(receivers, clumps);
    }

    private List<Clump> GroupIntoClumps(List<GameObject> appliers)
    {
        List<Clump> allClumps = new List<Clump>();

        // Separate attractors and repellers
        List<GameObject> attractors = appliers.Where(go => go.GetComponent<ForceProperty>().maxForceValue > 0).ToList();
        List<GameObject> repellers = appliers.Where(go => go.GetComponent<ForceProperty>().maxForceValue < 0).ToList();

        // Group attractors separately
        allClumps.AddRange(GroupSubList(attractors));

        // Group repellers separately
        allClumps.AddRange(GroupSubList(repellers));

        return allClumps;
    }

    private List<Clump> GroupSubList(List<GameObject> subAppliers)
    {
        List<Clump> clumps = new List<Clump>();
        HashSet<GameObject> assigned = new HashSet<GameObject>();
        float averageRange = subAppliers.Count > 0 ? subAppliers.Average(go => go.GetComponent<ForceProperty>().forceMaxRange) : 10f;
        float threshold = averageRange * clumpThresholdFactor;

        foreach (GameObject applier in subAppliers)
        {
            if (assigned.Contains(applier)) continue;

            List<GameObject> group = new List<GameObject>();
            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(applier);
            assigned.Add(applier);
            group.Add(applier);

            while (queue.Count > 0)
            {
                GameObject current = queue.Dequeue();
                foreach (GameObject other in subAppliers)
                {
                    bool isNotAssigned = !assigned.Contains(other);
                    bool isWithinThreshold = Vector3.Distance(current.transform.position, other.transform.position) < threshold;
                    if (isNotAssigned && isWithinThreshold)
                    {
                        assigned.Add(other);
                        queue.Enqueue(other);
                        group.Add(other);
                    }
                }
            }

            // Calculate average position, force, range
            Vector3 avgPos = Vector3.zero;
            float totalForce = 0;
            float totalRange = 0;
            ForceFalloffType falloffType = ForceFalloffType.Linear;
            AnimationCurve curve = null;

            foreach (GameObject go in group)
            {
                ForceProperty fp = go.GetComponent<ForceProperty>();
                avgPos += go.transform.position;
                totalForce += fp.maxForceValue;
                totalRange += fp.forceMaxRange;
                falloffType = fp.forceFalloffType; // Use the last one's falloff
                curve = fp.forceFalloffCurve;
            }

            avgPos /= group.Count;
            totalForce /= group.Count;
            totalRange /= group.Count;

            clumps.Add(new Clump
            {
                position = avgPos,
                forceValue = totalForce,
                maxRange = totalRange,
                falloffType = falloffType,
                falloffCurve = curve
            });
        }

        return clumps;
    }

    private void ApplyForces(List<GameObject> receivers, List<Clump> clumps)
    {
        foreach (GameObject receiver in receivers)
        {
            Rigidbody2D rb = receiver.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            ForceProperty fp = receiver.GetComponent<ForceProperty>();

            foreach (Clump clump in clumps)
            {
                Vector3 direction = clump.position - receiver.transform.position;
                float distance = direction.magnitude;

                bool isOutOfRange = distance > clump.maxRange || distance == 0;
                if (isOutOfRange) continue;

                direction.Normalize();

                bool isRepelling = clump.forceValue < 0;
                if (isRepelling)
                {
                    direction = -direction;
                }

                float forceMagnitude = CalculateForce(Mathf.Abs(clump.forceValue), distance, clump.maxRange, clump.falloffType, clump.falloffCurve);
                rb.AddForce(direction * forceMagnitude * Time.fixedDeltaTime);
                if (showDebugForces)
                {
                    Color forceColor = clump.forceValue > 0 ? Color.green : Color.red;
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
            //Debug.Log($"Registered entity {entity.name} for force type {type}");
        }
    }
    public void UnregisterEntity(GameObject entity) {
        foreach (var key in simulatedEntities.Keys)
        {
            if (simulatedEntities[key].Contains(entity))
            {
                simulatedEntities[key].Remove(entity);
            }
        }
    }
}
