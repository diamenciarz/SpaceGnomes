using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomScale : MonoBehaviour
{
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 3f;

    void Awake()
    {
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * randomScale;
    }
}
