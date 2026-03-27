using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InstantiateGroup", menuName = "ScriptableObjects/InstantiateGroup", order = 1)]
public class InstantiateGroup : ScriptableObject
{
    [SerializeField] private List<string> poolIds = new List<string>();
    [SerializeField] private List<float> angularOffsets = new List<float>();
    [SerializeField] private List<float> sidewaysOffsets = new List<float>();

    public void Spawn(Vector3 basePosition, Quaternion baseRotation)
    {
        float cumulativeSideways = 0f;
        for (int i = 0; i < poolIds.Count; i++)
        {
            cumulativeSideways += sidewaysOffsets[i];
            Quaternion rot = baseRotation * Quaternion.Euler(0, 0, angularOffsets[i]);
            Vector3 pos = basePosition + rot * Vector3.right * cumulativeSideways;
            GameObject spawnedObj = ObjectPoolManager.Instance.Spawn(poolIds[i], pos, rot);
        }
    }
    private void OnValidate()
    {
        if (angularOffsets.Count > poolIds.Count)
        {
            angularOffsets.RemoveRange(poolIds.Count, angularOffsets.Count - poolIds.Count);
        }
        if (angularOffsets.Count < poolIds.Count)
        {
            while (angularOffsets.Count < poolIds.Count) angularOffsets.Add(0f);
        }
        if (sidewaysOffsets.Count > poolIds.Count)
        {
            sidewaysOffsets.RemoveRange(poolIds.Count, sidewaysOffsets.Count - poolIds.Count);
        }
        if (sidewaysOffsets.Count < poolIds.Count)
        {
            while (sidewaysOffsets.Count < poolIds.Count) sidewaysOffsets.Add(0f);
        }
    }
}
