using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds information about the map, such as its width and height, and provides methods to get positions on the map based on percentage values.
/// Center of map has coordinates (0,0)
/// </summary>
public class MapInformation: AbstractSingleton<MapInformation>
{
    [SerializeField] public float mapWidth;
    [SerializeField] public float mapHeight;

    [HideInInspector] public Vector2 bottomLeftCorner;
    [HideInInspector] public Vector2 topRightCorner;

    private void Start()
    {
        UpdateMapCorners();
    }   
    private void UpdateMapCorners()
    {
        bottomLeftCorner = new Vector2(transform.position.x - mapWidth / 2, transform.position.y - mapHeight / 2);
        topRightCorner = new Vector2(transform.position.x + mapWidth / 2, transform.position.y + mapHeight / 2);

    }
    public Vector2 GetMapPercentagePosition(float xPercentage, float yPercentage)
    {
        return new Vector2(bottomLeftCorner.x + mapWidth * Mathf.Clamp01(xPercentage), bottomLeftCorner.y + mapHeight * Mathf.Clamp01(yPercentage));
    }
}
