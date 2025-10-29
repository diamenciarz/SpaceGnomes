using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using static ForceManager;

public class ForceProperty : MonoBehaviour
{
    [SerializeField] public ForceType forceType = ForceType.Gravity;
    [SerializeField] public bool forceApplier = true;
    [SerializeField] public bool forceReceiver = true;
    [SerializeField] public float maxForceValue = 25;
    [SerializeField] [Tooltip("Force will scale between 0 and forceMaxRange")] public float maxForceApplyRange = 10;
    [SerializeField] public ForceManager.ForceFalloffType forceFalloffType = ForceFalloffType.Quadratic;
    [SerializeField] [Tooltip("Only used if ForceFallofType is CustomCurve")] public AnimationCurve forceFalloffCurve;

    private void OnEnable()
    {
        if (ForceManager.Instance != null)
        {
            ForceManager.Instance.RegisterEntity(gameObject, forceType);
        }
        else
        {
            StartCoroutine(WaitForForceManager());
        }
    }
    private void OnDisable()
    {
        if (ForceManager.Instance != null)
        {
            ForceManager.Instance.UnregisterEntity(gameObject);
        }
    }

    private IEnumerator WaitForForceManager()
    {
        while (ForceManager.Instance == null)
        {
            yield return null;
        }
        ForceManager.Instance.RegisterEntity(gameObject, forceType);
    }
}
