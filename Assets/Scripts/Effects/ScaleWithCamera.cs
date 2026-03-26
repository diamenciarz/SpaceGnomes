using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCamera : MonoBehaviour
{
    private const float DEFAULT_CAMERA_SIZE = 10f;
    private Vector3 initialLocalScale;

    void Start()
    {
        initialLocalScale = transform.localScale;
        UpdateScale(CameraInformation.Instance.ma orthographicSize);
        CameraInformation.Instance.OnCameraZoomChanged += UpdateScale;
    }

    private void OnDestroy()
    {
        if (CameraInformation.Instance != null)
        {
            CameraInformation.Instance.OnCameraZoomChanged -= UpdateScale;
        }
    }

    private void UpdateScale(float newOrthographicSize)
    {
        transform.localScale = initialLocalScale * DEFAULT_CAMERA_SIZE / newOrthographicSize;
        Debug.Log($"Updated scale to {transform.localScale} based on camera size {newOrthographicSize}");
    }
}
