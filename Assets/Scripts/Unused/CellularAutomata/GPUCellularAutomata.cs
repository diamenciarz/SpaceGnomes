using UnityEngine;
using UnityEngine.UI;
using System;

public enum ActivationType
{
    Identity,      // f(x) = x
    Abs,           // f(x) = |x|
    Sin,           // f(x) = sin(x)
    InverseGaussian,  // f(x) = 1 / (1 + x²)
    SlimeMold      // f(x) = -1 / (0.89 * x² + 1) + 1
}

[Serializable]
public class ConvolutionFilter
{
    [Header("3x3 Filter Weights (Row-Major)")]
    public float[] kernel = new float[9]
    {
        0f, 1f, 0f,
        1f, 0f, 1f,
        0f, 1f, 0f
    };

    public float Get(int x, int y) => kernel[y * 3 + x];
    public void Set(int x, int y, float val) { kernel[y * 3 + x] = val; }
}

[Serializable]
public class ActivationSettings
{
    public ActivationType type = ActivationType.Identity;
    [Header("SlimeMold Params")]
    [Range(0.1f, 2.0f)] public float dampingFactor = 0.89f;
    [Range(0.5f, 1.5f)] public float offset = 1.0f;
}

public class GPUCellularAutomata : MonoBehaviour
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

    [Header("Compute")]
    [SerializeField] private ComputeShader computeShader;

    [Header("Display")]
    [SerializeField] private RawImage rawImageDisplay;
    [SerializeField] private Material displayMaterial;
    [SerializeField] private Shader gridDisplayShader;  // Optional: Auto-find if null
    [SerializeField] private Color minColor = Color.black;
    [SerializeField] private Color maxColor = Color.white;

    private RenderTexture currentGrid;
    private RenderTexture nextGrid;
    private int kernelIndex;
    private int noiseKernelIndex;

    void Start()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Compute Shaders not supported!");
            return;
        }

        if (width > 8192 || height > 8192)
            Debug.LogWarning("Large GPU grid (>8192) may exceed texture limits—downsample.");

        kernelIndex = computeShader.FindKernel("ApplyCellularAutomata");
        noiseKernelIndex = computeShader.FindKernel("InitializeNoise");

        // Create ping-pong RFloat RenderTextures
        currentGrid = CreateRT(width, height);
        nextGrid = CreateRT(width, height);

        InitializeGrid();
        SetupDisplay();

        // Initial display
        if (rawImageDisplay != null) rawImageDisplay.texture = currentGrid;
    }

    void Update()
    {
        // Set uniforms (filter, activation, threshold)
        SetUniforms();

        // Dispatch: Convo + Activate + Threshold in one kernel
        int threadGroupsX = Mathf.CeilToInt(width / 8f);
        int threadGroupsY = Mathf.CeilToInt(height / 8f);

        computeShader.SetTexture(kernelIndex, "InputGrid", currentGrid);
        computeShader.SetTexture(kernelIndex, "OutputGrid", nextGrid);
        computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);

        // Swap
        (currentGrid, nextGrid) = (nextGrid, currentGrid);

        // Update display
        if (rawImageDisplay != null) rawImageDisplay.texture = currentGrid;
    }

    private void SetUniforms()
    {
        // Filter
        for (int i = 0; i < 9; i++)
            computeShader.SetFloat($"_Filter{i:D2}", filter.kernel[i]);

        // Activation
        computeShader.SetInt("_ActivationType", (int)activation.type);
        computeShader.SetFloat("_DampingFactor", activation.dampingFactor);
        computeShader.SetFloat("_Offset", activation.offset);

        // Threshold
        computeShader.SetBool("_ApplyThreshold", applyThreshold);
        computeShader.SetFloat("_Threshold", threshold);
    }

    private void InitializeGrid()
    {
        // Exact same RNG as CPU for identical sequence
        UnityEngine.Random.InitState(1000);  // Fixed seed; change to (int)(Time.time * 1000f) for variety

        // Temp data arrays (match CPU)
        float[] tempData = new float[width * height];

        // Fill with same loop
        for (int i = 0; i < tempData.Length; i++)
        {
            float rand = UnityEngine.Random.value;
            tempData[i] = (aliveProbability >= 1f) ? rand : (rand < aliveProbability ? 1f : 0f);
        }

        // Pack to bytes like CPU (4 bytes/float for RFloat)
        byte[] rawData = new byte[width * height * 4];
        for (int i = 0; i < tempData.Length; i++)
        {
            byte[] bytes = System.BitConverter.GetBytes(tempData[i]);
            bytes.CopyTo(rawData, i * 4);
        }

        // Temp Texture2D (RFloat) + LoadRaw (exact CPU match)
        Texture2D tempTex = new Texture2D(width, height, TextureFormat.RFloat, false);
        tempTex.LoadRawTextureData(rawData);
        tempTex.Apply();

        // Blit raw data to RT (preserves floats perfectly)
        Graphics.Blit(tempTex, currentGrid);

        // Immediate cleanup (editor-safe)
        DestroyImmediate(tempTex);

        // Optional: Quick validation log (remove after testing)
        Debug.Log($"GPU Init: {tempData.Length} pixels, avg val = {System.Linq.Enumerable.Average(tempData):F3}, ones count = {System.Linq.Enumerable.Count(tempData, v => Mathf.Approximately(v, 1f))}");
    }

    //private void InitializeGrid()
    //{
    //    // Use same RNG as CPU for identical randomness
    //    UnityEngine.Random.InitState((int)(1000f));  // Or fixed seed like 42 for repro

    //    // Temp Texture2D for CPU fill (RFloat)
    //    Texture2D tempTex = new Texture2D(width, height, TextureFormat.RFloat, false);
    //    //byte[] rawData = new byte[width * height * 4];
    //    Color[] pixels = new Color[width * height];

    //    for (int i = 0; i < pixels.Length; i++)
    //    {
    //        float rand = UnityEngine.Random.value;
    //        float val = (aliveProbability >= 1f) ? rand : (rand < aliveProbability ? 1f : 0f);
    //        pixels[i] = new Color(val, 0f, 0f, 1f);  // R channel only
    //        //BitConverter.GetBytes(val).CopyTo(rawData, i * 4);
    //    }

    //    //tempTex.LoadRawTextureData(rawData);
    //    //tempTex.Apply(false);  // No mips, CPU-only
    //    tempTex.SetPixels(pixels);
    //    tempTex.Apply();

    //    //Blit to RenderTexture (fast GPU copy)
    //    Graphics.Blit(tempTex, currentGrid);

    //    // Cleanup
    //    Destroy(tempTex);
    //}

    //private void InitializeGrid()
    //{
    //    computeShader.SetFloat("_RandomSeed", UnityEngine.Random.value * 100.0f);
    //    computeShader.SetFloat("_AliveProb", aliveProbability >= 1f ? 1.0f : aliveProbability);

    //    int threadGroupsX = Mathf.CeilToInt(width / 8f);
    //    int threadGroupsY = Mathf.CeilToInt(height / 8f);
    //    computeShader.SetTexture(noiseKernelIndex, "NoiseOutput", currentGrid);
    //    computeShader.Dispatch(noiseKernelIndex, threadGroupsX, threadGroupsY, 1);
    //}

    private RenderTexture CreateRT(int w, int h)
    {
        RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    private void SetupDisplay()
    {
        if (displayMaterial == null)
        {
            Debug.LogError("GridDisplay shader missing—create as before!");
            return;
        }
        displayMaterial.SetColor("_MinColor", minColor);
        displayMaterial.SetColor("_MaxColor", maxColor);

        if (rawImageDisplay != null)
        {
            rawImageDisplay.material = displayMaterial;
            rawImageDisplay.texture = currentGrid;
        }
    }

    // Debug: CPU readback sample (use sparingly)
    [ContextMenu("Debug Sample")]
    public void DebugSample()
    {
        Texture2D tempTex = new Texture2D(width, height, TextureFormat.RFloat, false);
        RenderTexture.active = currentGrid;
        tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTex.Apply();

        float[] pixels = new float[25];
        Color[] colors = tempTex.GetPixels(0, height - 5, 5, 5);  // Top-left 5x5
        for (int i = 0; i < 25; i++) pixels[i] = colors[i].r;

        float avg = 0f;
        string log = "GPU Grid Sample (Top-Left 5x5, Avg: ";
        for (int y = 0; y < 5; y++)
        {
            string row = "";
            for (int x = 0; x < 5; x++)
            {
                float val = pixels[y * 5 + x];
                row += $"{val:F2} ";
                avg += val;
            }
            log += row + "\n";
        }
        Debug.Log(log + $"{avg / 25f:F3})");

        Destroy(tempTex);
        RenderTexture.active = null;
    }

    void OnDestroy()
    {
        currentGrid.Release();
        nextGrid.Release();
        //Destroy(currentGrid);
        //Destroy(nextGrid);
    }

    // Public reset
    public void ResetGrid() => InitializeGrid();
}