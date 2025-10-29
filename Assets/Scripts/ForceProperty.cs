using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class ForceProperty : MonoBehaviour
{
    [SerializeField] public ForceManager.ForceType forceType;
    [SerializeField] public bool forceApplier = true;
    [SerializeField] public bool forceReceiver = true;
    [SerializeField] public float maxForceValue = 100;
    [SerializeField] [Tooltip("Force will scale between 0 and forceMaxRange")] public float maxForceApplyRange;
    [SerializeField] public ForceManager.ForceFalloffType forceFalloffType;
    [SerializeField] [Tooltip("Only used if ForceFallofType is CustomCurve")] public AnimationCurve forceFalloffCurve;

    private void Start()
    {
        // Registration is handled in OnEnable
    }
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
