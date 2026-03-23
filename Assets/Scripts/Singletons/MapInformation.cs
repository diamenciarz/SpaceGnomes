using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Center of map has coordinates (0,0)
/// </summary>
public class MapInformation: AbstractSingleton<MapInformation>
{
    public Vector2 bottomLeftCorner;
    public Vector2 topRightCorner;

    [SerializeField] public float mapWidth;
    [SerializeField] public float mapHeight;

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
