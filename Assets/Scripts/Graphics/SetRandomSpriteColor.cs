using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SetRandomSpriteColor : MonoBehaviour
{
    public enum ColorMode
    {
        InterpolateHues,
        WeightedHues
    }

    [Header("Brightness Settings")]
    [SerializeField] [Range(0f, 1f)] private float minBrightness = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float maxBrightness = 1f;

    [Header("Saturation Settings")]
    [SerializeField] [Range(0f, 1f)] private float minSaturation = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float maxSaturation = 1f;

    [Header("Alpha Settings")]
    [SerializeField] [Range(0f, 1f)] private float minAlpha = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float maxAlpha = 1f;

    [Header("Color Mode")]
    [SerializeField] private ColorMode colorMode = ColorMode.InterpolateHues;

    [Header("Interpolate Hues Mode")]
    [SerializeField] [Range(0f, 360f)] private float hue1 = 0f;
    [SerializeField] [Range(0f, 360f)] private float hue2 = 180f;

    [System.Serializable]
    public struct HueWeight
    {
        [Range(0f, 360f)] public float hue;
        public float weight;
    }
    [Header("Weighted Hues Mode")]
    [SerializeField] private List<HueWeight> weightedHues = new List<HueWeight>();

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        float hue = GenerateHue();
        float saturation = Random.Range(minSaturation, maxSaturation);
        float brightness = Random.Range(minBrightness, maxBrightness);
        float alpha = Random.Range(minAlpha, maxAlpha);
        Color color = Color.HSVToRGB(hue, saturation, brightness);
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private float GenerateHue()
    {
        switch (colorMode)
        {
            case ColorMode.InterpolateHues:
                return Mathf.Lerp(hue1 / 360f, hue2 / 360f, Random.Range(0f, 1f));
            case ColorMode.WeightedHues:
                return GetWeightedHue();
            default:
                return 0f;
        }
    }

    private float GetWeightedHue()
    {
        if (weightedHues.Count == 0) throw new System.Exception("No weighted hues defined!");
        float totalWeight = weightedHues.Sum((hw)=>hw.weight);

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var hw in weightedHues)
        {
            cumulative += hw.weight;
            if (randomValue <= cumulative)
            {
                return hw.hue / 360f;
            }
        }
        return weightedHues[weightedHues.Count - 1].hue / 360f;
    }
}
