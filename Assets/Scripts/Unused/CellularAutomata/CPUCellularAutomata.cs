using UnityEngine;
using UnityEngine.UI;
using System;

//public enum ActivationType
//{
//    Identity,      // f(x) = x
//    Abs,           // f(x) = |x|
//    Sin,           // f(x) = sin(x)
//    InverseGaussian,  // f(x) = 1 / (1 + x²)
//    SlimeMold      // Custom: f(x) = -1 / (0.89 * x² + 1) + 1 (your formula)
//}

//[System.Serializable]
//public class ConvolutionFilter
//{
//    [Header("3x3 Filter Weights (Row-Major)")]
//    public float[] kernel = new float[9]
//    {
//        0f, 1f, 0f,
//        1f, 0f, 1f,
//        0f, 1f, 0f
//    };

//    public float Get(int x, int y) => kernel[y * 3 + x];
//    public void Set(int x, int y, float val) { kernel[y * 3 + x] = val; }
//}

//[System.Serializable]
//public class ActivationSettings
//{
//    public ActivationType type = ActivationType.Identity;
//    [Header("SlimeMold Params")]
//    [Range(0.1f, 2.0f)] public float dampingFactor = 0.89f;
//    [Range(0.5f, 1.5f)] public float offset = 1.0f;
//}

public class CPUCellularAutomata : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 1000;
    [SerializeField] private int height = 1000;
    [SerializeField] private float aliveProbability = 0.3f;  // For binary init

    [Header("Convolution")]
    [SerializeField] private ConvolutionFilter filter = new ConvolutionFilter();
    [SerializeField] private bool applyThreshold = false;
    [SerializeField] private float threshold = 0.5f;

    [Header("Activation")]
    [SerializeField] private ActivationSettings activation = new ActivationSettings();

    [Header("Display")]
    [SerializeField] private RawImage rawImageDisplay;
    [SerializeField] private Material displayMaterial;
    [SerializeField] private Shader gridDisplayShader;  // Optional: Auto-find if null
    [SerializeField] private Color minColor = Color.black;
    [SerializeField] private Color maxColor = Color.white;

    private Texture2D currentGrid;
    private float[] currentData;
    private float[] nextData;

    void Start()
    {
        if (width > 2048 || height > 2048)
            Debug.LogWarning("Large CPU grid (>2048) may cause hitches—consider downsampling.");

        // Create double-buffer Texture2D (RFloat for [0,1] data)
        currentGrid = new Texture2D(width, height, TextureFormat.RFloat, false);
        currentData = new float[width * height];
        nextData = new float[width * height];

        InitializeGrid();
        SetupDisplay();

        // Initial display update
        UpdateTextureFromData(currentData);
        UpdateDisplay();
        //DebugSample();
    }

    void Update()
    {
        // CPU Step: Convolve + Activate + Threshold
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float sum = ConvolveAt(x, y);
                float activated = ApplyActivation(sum);
                nextData[idx] = applyThreshold ? (activated > threshold ? 1f : 0f) : Mathf.Clamp01(activated);
            }
        }

        // Swap buffers
        (currentData, nextData) = (nextData, currentData);
        UpdateTextureFromData(currentData);

        UpdateDisplay();
        //DebugSample();
    }

    private float ConvolveAt(int x, int y)
    {
        float sum = 0f;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = (x + dx + width) % width;   // Toroidal
                int ny = (y + dy + height) % height;
                int pixelIdx = ny * width + nx;
                sum += currentData[pixelIdx] * filter.Get(dx + 1, dy + 1);  // Offset for filter coords
            }
        }
        return sum;
    }

    private float ApplyActivation(float x)
    {
        return activation.type switch
        {
            ActivationType.Identity => x,
            ActivationType.Abs => Mathf.Abs(x),
            ActivationType.Sin => Mathf.Sin(x),
            ActivationType.InverseGaussian => 1f / (1f + x * x),
            ActivationType.SlimeMold => -1f / (activation.dampingFactor * x * x + 1f) + activation.offset,
            _ => x  // Fallback
        };
    }

    private void InitializeGrid()
    {
        UnityEngine.Random.InitState((int)(1000f));  // Seed
        for (int i = 0; i < currentData.Length; i++)
        {
            float rand = UnityEngine.Random.value;
            currentData[i] = aliveProbability >= 1f ? rand : (rand < aliveProbability ? 1f : 0f);
        }
    }

    private void UpdateTextureFromData(float[] data)
    {
        byte[] rawData = new byte[width * height * 4];  // 4 bytes per float (RFloat)
        for (int i = 0; i < data.Length; i++)
        {
            byte[] bytes = BitConverter.GetBytes(data[i]);
            bytes.CopyTo(rawData, i * 4);
        }
        currentGrid.LoadRawTextureData(rawData);
        currentGrid.Apply(false);  // No mips, CPU-only
    }

    private void SetupDisplay()
    {
        if (displayMaterial == null)
        {
            if (gridDisplayShader == null) gridDisplayShader = Shader.Find("Custom/GridDisplay");
            if (gridDisplayShader != null) displayMaterial = new Material(gridDisplayShader);
            else
            {
                Debug.LogError("GridDisplay shader missing—create as before!");
                return;
            }
        }
        displayMaterial.SetColor("_MinColor", minColor);
        displayMaterial.SetColor("_MaxColor", maxColor);

        if (rawImageDisplay != null)
        {
            rawImageDisplay.material = displayMaterial;
            rawImageDisplay.texture = currentGrid;
        }
    }

    private void UpdateDisplay()
    {
        if (rawImageDisplay != null)
        {
            rawImageDisplay.texture = currentGrid;
            displayMaterial.SetColor("_MinColor", minColor);  // Re-apply if changed
            displayMaterial.SetColor("_MaxColor", maxColor);
        }
    }

    // Debug: Sample patch (CPU-fast, direct from data)
    [ContextMenu("Debug Sample")]
    public void DebugSample()
    {
        string log = $"CPU Grid Sample (Top-Left 5x5, Avg: {GetAverage():F3}):\n";
        float avg = 0f;
        for (int y = 0; y < 5 && y < height; y++)
        {
            string row = "";
            for (int x = 0; x < 5 && x < width; x++)
            {
                float val = currentData[y * width + x];
                row += $"{val:F2} ";
                avg += val;
            }
            log += row + "\n";
        }
        Debug.Log(log + $"\nAvg: {avg / 25f:F3}");
    }

    private float GetAverage()
    {
        float sum = 0f;
        for (int i = 0; i < currentData.Length; i++) sum += currentData[i];
        return sum / currentData.Length;
    }

    void OnDestroy()
    {
        Destroy(currentGrid);
    }

    // Public reset (e.g., for button)
    public void ResetGrid()
    {
        InitializeGrid();
        UpdateTextureFromData(currentData);
    }
}