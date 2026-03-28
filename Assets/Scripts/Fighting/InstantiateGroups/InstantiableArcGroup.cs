using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates an arc of instantiable entities given a list of angle and forward offsets.
/// The objects will be rotated to look in the direction of the arc angle.
/// </summary>
[CreateAssetMenu(fileName = "ArcGroup", menuName = "ScriptableObjects/Instantiable/ArcGroup", order = 1)]
public class InstantiableArcGroup : InstantiableGroup
{
    [SerializeField] private List<Instantiable> instantiable = new List<Instantiable>();
    [SerializeField][Tooltip("Instantiated objects will be moved by a forwardOffset in the arcAngle (in degrees)")] private List<float> arcAngles = new List<float>();
    [SerializeField][Tooltip("The distance along the arc angle")] private List<float> forwardOffsets = new List<float>();
    [SerializeField][Tooltip("The directions (in degrees) in which the objects will be looking at")] private List<float> objectDirections = new List<float>();
    [SerializeField]
    protected override List<InstantiableData> GetObjectFormation()
    {
        List<InstantiableData> formation = new List<InstantiableData>();
        for (int i = 0; i < instantiable.Count; i++)
        {
            InstantiableData data = new InstantiableData();
            data.instantiable = instantiable[i];
            data.rotation = Quaternion.Euler(0f, 0f, objectDirections[i]);
            float rad = arcAngles[i] * Mathf.Deg2Rad;
            data.offset = new Vector2(forwardOffsets[i] * Mathf.Cos(rad), forwardOffsets[i] * Mathf.Sin(rad));
            formation.Add(data);
        }
        return formation;
    }
    private void OnValidate()
    {
        if (objectDirections.Count > instantiable.Count)
        {
            objectDirections.RemoveRange(instantiable.Count, objectDirections.Count - instantiable.Count);
        }
        if (objectDirections.Count < instantiable.Count)
        {
            while (objectDirections.Count < instantiable.Count) objectDirections.Add(0f);
        }
        if (forwardOffsets.Count > instantiable.Count)
        {
            forwardOffsets.RemoveRange(instantiable.Count, forwardOffsets.Count - instantiable.Count);
        }
        if (forwardOffsets.Count < instantiable.Count)
        {
            while (forwardOffsets.Count < instantiable.Count) forwardOffsets.Add(1f);
        }
        if (arcAngles.Count > instantiable.Count)
        {
            arcAngles.RemoveRange(instantiable.Count, arcAngles.Count - instantiable.Count);
        }
        if (arcAngles.Count < instantiable.Count)
        {
            while (arcAngles.Count < instantiable.Count) arcAngles.Add(0f);
        }
    }
}
