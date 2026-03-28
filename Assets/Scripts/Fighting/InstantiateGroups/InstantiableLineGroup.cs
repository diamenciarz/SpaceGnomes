using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a fan of instantiable entities given a list of angle and coordinate offsets
/// </summary>
[CreateAssetMenu(fileName = "LineGroup", menuName = "ScriptableObjects/Instantiable/LineGroup", order = 1)]
public class InstantiableLineGroup : InstantiableGroup
{
    [SerializeField] private List<Instantiable> instantiable = new List<Instantiable>();
    [SerializeField][Tooltip("The directions (in degrees) in which the objects will be looking at")] private List<float> objectDirections = new List<float>();
    [SerializeField][Tooltip("The relative position from group origin in local X axis. Negative means left, positive means right.")] private List<float> sidewaysOffsets = new List<float>();
    [SerializeField]
    [Tooltip("If straight line, all sidewaysOffsets will happen purely on the X axis. If Arc, the sidewaysOffsets will be applied in the objectDirections")]
    protected override List<InstantiableData> GetObjectFormation()
    {
        List<InstantiableData> formation = new List<InstantiableData>();
        for (int i = 0; i < instantiable.Count; i++)
        {
            InstantiableData data = new InstantiableData();
            data.instantiable = instantiable[i];
            data.rotation = Quaternion.Euler(0f, 0f, objectDirections[i]);
            data.offset = new Vector2(0f, sidewaysOffsets[i]);
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
        if (sidewaysOffsets.Count > instantiable.Count)
        {
            sidewaysOffsets.RemoveRange(instantiable.Count, sidewaysOffsets.Count - instantiable.Count);
        }
        if (sidewaysOffsets.Count < instantiable.Count)
        {
            while (sidewaysOffsets.Count < instantiable.Count) sidewaysOffsets.Add(1f);
        }
    }
}
