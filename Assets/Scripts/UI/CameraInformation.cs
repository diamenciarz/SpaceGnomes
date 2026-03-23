using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This singleton class will keep track of the camera's size and position in world units 
/// and will provide methods to check if a position is on screen and to clamp positions to be on screen.
/// </summary>
public class CameraInformation : AbstractSingleton<CameraInformation>
{
    /// <summary>
    /// This is given in world units. Y means height, X means width
    /// </summary>
    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;


    Camera mainCamera;
    private bool hasCalculatedCameraSize = false;
    private Vector2 cameraSize;

    protected override void Awake()
    {
        base.Awake();
        CalculateCameraSize();
        RecountScreenEdges();
    }

    private void CalculateCameraSize()
    {
        SetMainCamera(Camera.main);
        cameraSize.y = 2f * mainCamera.orthographicSize;
        cameraSize.x = cameraSize.y * mainCamera.aspect;
        hasCalculatedCameraSize = true;
    }

    void Update()
    {
        RecountScreenEdges();
    }
    private void RecountScreenEdges()
    {
        if (!hasCalculatedCameraSize)
        {
            CalculateCameraSize();
        }
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }

    #region Accessor methods
    /// <summary>
    /// Camera size is given in world units. Y means height, X means width.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCameraSize()
    {
        if (!hasCalculatedCameraSize)
        {
            CalculateCameraSize();
        }
        return cameraSize;
    }
    public bool IsPositionOnScreen(Vector2 position)
    {
        return IsPositionOnScreen(position, 0);
    }
    /// <summary>
    /// <param name="position"></param>
    /// <param name="offsetToCenter">The distance from the edge of the screen that is considered to be outside</param>
    /// </summary>
    public bool IsPositionOnScreen(Vector2 position, float offsetToCenter)
    {
        return IsPositionOnScreen(position, offsetToCenter, offsetToCenter, offsetToCenter, offsetToCenter);
    }
    public bool IsPositionOnScreen(Vector2 position, float leftOffset, float rightOffset, float topOffset, float bottomOffset)
    {
        return position.x > xMin + leftOffset && position.x < xMax - rightOffset
            && position.y > yMin + bottomOffset && position.y < yMax - topOffset;
    }
    public Vector2 ClampPositionOnScreen(Vector2 position)
    {
        return ClampPositionOnScreen(position, 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="offsetToCenter">The distance from the edge of the screen that is considered to be outside</param>
    /// <returns></returns>
    public Vector2 ClampPositionOnScreen(Vector2 position, float offsetToCenter)
    {
        return ClampPositionOnScreen(position, offsetToCenter, offsetToCenter, offsetToCenter, offsetToCenter);
    }
    public Vector2 ClampPositionOnScreen(Vector2 position, float leftOffset, float rightOffset, float topOffset, float bottomOffset)
    {
        float newXPosition = Mathf.Clamp(position.x, xMin + leftOffset, xMax - rightOffset);
        float newYPosition = Mathf.Clamp(position.y, yMin + bottomOffset, yMax - topOffset);
        return new Vector2(newXPosition, newYPosition);
    }
    /// <returns>
    /// An array with the bottomLeft corner of the camera at index 0 and topRight corner at index 1 given in world units.
    /// </returns>
    public Vector2[] GetDiagonalCameraPoints()
    {
        return new Vector2[]
        {
            new Vector2(xMin, yMin),
            new Vector2(xMax, yMax)
        };
    }
    #endregion

    #region Mutator methods
    public void SetMainCamera(Camera cam)
    {
        mainCamera = cam;
    }
    #endregion
}
