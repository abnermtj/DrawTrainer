using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DrawManager : MonoBehaviour
{
    [SerializeField] private ComputeShader drawComputeShader;
    [SerializeField] private ComputeShader alphaBlendComputeShader;
    [SerializeField] private ComputeShader alphaComputeShader;
    [SerializeField] private Color backgroundColour;

    // Brush
    [SerializeField] private BrushSizeSlider brushSizeSlider;

    [SerializeField] private Color brushColour;
    [SerializeField] private float brushSize = 0.5f;
    [SerializeField, Range(0.001f, 1)] private float strokePressIntervalSeconds = 0.001f;

    protected bool penPressed;
    protected bool mousePressed;
    protected bool pointerJustPressed;
    protected bool pointerJustReleased;
    protected bool pointerPressed;
    protected float brushSizePressure = 0;
    protected float interpolatedPenPressure;
    protected Vector2 strokeEndPos = Vector2.positiveInfinity;
    protected Vector4 penPosition;
    private Vector4 prevPenPosition;

    // Target
    [SerializeField] protected TargetSpawner targetSpawner;

    [SerializeField] protected float targetResetIntervalSeconds = 1;
    [SerializeField] protected int minTargets = 2;
    [SerializeField] protected int maxTargets = 2;
    [SerializeField] protected int goalTargets = 50;
    [SerializeField] protected float targetWidth, targetHeight;

    protected float targetResetTimer;

    // Game
    [SerializeField] private GameObject DEBUG_BOX, DEBUG_BOX2, DEBUG_BOX3;

    [SerializeField] private RenderTexture finalBrushRenderTexture;
    [SerializeField] private RenderTexture tempFinalBrushRenderTexture;
    public RenderTexture curBrushRenderTexture;
    public RenderTexture _finalBrushRenderTexture;
    public RenderTexture _tempFinalBrushRenderTexture;

    [SerializeField] private GameObject DEBUG_LABEL;

    [SerializeField] private GameObject curBrushLayer; // TODO this currently uses a render texture that is 4k x 4k which is not necessary for lower end systems. Find a way to adapt render texture to the user.

    [SerializeField] private GameObject bgLayer;

    [SerializeField] protected GameObject comboPrefab;
    [SerializeField] protected GameObject canvas;
    [SerializeField] protected GameObject winLabel;
    [SerializeField] protected GameObject spawnBox;
    [SerializeField] protected GameObject targets;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] protected Camera camera;

    // Score
    [SerializeField] protected GameObject gameTimerLabel;

    [SerializeField] protected GameObject hitScoreLabel;
    [SerializeField] protected GameObject missScoreLabel;
    [SerializeField] protected float gameTimer = 500;
    protected int missScore = 0;
    protected int comboScore = 0;
    protected int hitScore = 0;

    protected AudioSource source;
    public AudioClip[] comboSounds;

    protected void Start()
    {
        // Render texture is used for line drawing
        curBrushRenderTexture = new RenderTexture(Screen.width, Screen.height, 32)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true,
        };
        curBrushRenderTexture.Create();

        // Place Holder textures
        _finalBrushRenderTexture = new RenderTexture(Screen.width, Screen.height, 32)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true,
        };
        _finalBrushRenderTexture.Create();
        _tempFinalBrushRenderTexture = new RenderTexture(Screen.width, Screen.height, 32)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true,
        };
        _tempFinalBrushRenderTexture.Create();


        ResetBoard(isWin: true);

        brushSizeSlider.slider.SetValueWithoutNotify(brushSizePressure);

        prevPenPosition = Pen.current.position.ReadValue();
        targetResetTimer = targetResetIntervalSeconds;
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.ForceSoftware); // This centers a custom cursor on the mouse

        source = GetComponent<AudioSource>();

        bgLayer.GetComponent<Image>().color = backgroundColour;
        InitBrushLayer();
    }

    private void InitBrushLayer()
    {
        RectTransform rt = curBrushLayer.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    protected void ClearBrushMarks(RenderTexture targetTexture)
    {
        int initBackgroundKernel = drawComputeShader.FindKernel("InitBackground");
        drawComputeShader.SetVector("_BackgroundColour", backgroundColour);
        drawComputeShader.SetTexture(initBackgroundKernel, "_Canvas", targetTexture);
        drawComputeShader.SetFloat("_CanvasWidth", targetTexture.width);
        drawComputeShader.SetFloat("_CanvasHeight", targetTexture.height);
        drawComputeShader.GetKernelThreadGroupSizes(initBackgroundKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        drawComputeShader.Dispatch(initBackgroundKernel,
            Mathf.CeilToInt(targetTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(targetTexture.height / (float)yGroupSize),
            1);
    }

    protected virtual void ResetBoard(bool isWin)
    {
        ClearBrushMarks(curBrushRenderTexture);

        targetResetTimer = targetResetIntervalSeconds;
        targetSpawner.ClearAll(playSound: isWin);

        int rInt = Random.Range(minTargets, maxTargets + 1);
        targetSpawner.Spawn(rInt, spawnBox.GetComponent<RectTransform>(), targetWidth, targetHeight, camera);
    }

    private void UpdateBrush()
    {
        if (penPressed)
        {
            brushSizePressure = brushSize * interpolatedPenPressure;
        }
        else
        {
            brushSizePressure = brushSize;
        }
    }

    private void UpdatePen()
    {
        Pointer pointer = Pointer.current;
        Pen pen = Pen.current;

        // Pen pressure in [0, 1] where 1 is max pressure
        float curPenPressure = pen.pressure.ReadValue();
        interpolatedPenPressure = Mathf.Lerp(interpolatedPenPressure, curPenPressure, 0.9f);

        // Pen Pressed
        bool prevPointerPressed = pointerPressed;
        penPressed = pen.press.ReadValue() != 0;
        pointerPressed = pointer.press.ReadValue() != 0;
        mousePressed = pointerPressed && !penPressed;
        if (pointerPressed && !prevPointerPressed)
        {
            pointerJustPressed = true;
            pointerJustReleased = false;
        }
        else if (!pointerPressed && prevPointerPressed)
        {
            pointerJustPressed = false;
            pointerJustReleased = true;
        }
        else
        {
            pointerJustPressed = false;
            pointerJustReleased = false;
        }

        // Pen position
        prevPenPosition = penPosition;
        penPosition = pointer.position.ReadValue();
        DEBUG_LABEL.GetComponent<Text>().text = penPosition.ToString();

        if (pointerJustReleased)
        {
            strokeEndPos = penPosition;
        }
    }

    private void UpdateTimers()
    {
        gameTimer -= Time.deltaTime;
        targetResetTimer -= Time.deltaTime;
    }

    private void UpdateCommonLabels()
    {
        gameTimerLabel.GetComponent<Text>().text = ((int)gameTimer).ToString();
        winLabel.SetActive(hitScore == goalTargets);
    }

    protected void Update()
    {
        // Pre Input logic
        UpdatePen();
        UpdateBrush();
        UpdateTimers();
        UpdateCommonLabels();

        // Input logic
        // DEBUG
        if (DEBUG_BOX && DEBUG_BOX2)
        {
            DEBUG_BOX.GetComponent<Image>().color = Color.white;
            Vector4 pressureColor = Color.white * interpolatedPenPressure;
            pressureColor.w = 1;
            DEBUG_BOX2.GetComponent<Image>().color = pressureColor;
        }
        // DEBUG

        if (!brushSizeSlider.isInUse && pointerPressed)
        {
            if (DEBUG_BOX && DEBUG_BOX2)
            {
                DEBUG_BOX.GetComponent<Image>().color = Color.black;
            }
            MarkCurPenPos();
            Graphics.Blit(curBrushRenderTexture, _tempFinalBrushRenderTexture);
            //AlphaBlendNewBrushLayer(curBrushRenderTexture, _tempFinalBrushRenderTexture);
            setAlpha(_tempFinalBrushRenderTexture, brushColour.a);
            Graphics.Blit(_tempFinalBrushRenderTexture, tempFinalBrushRenderTexture); // Should be visuall equivalent to the blit bloc below
        }

        // Post input Logic

        if (pointerJustReleased)
        {
            targetSpawner.ResetTargets();

            //Graphics.Blit(curBrushRenderTexture, _tempFinalBrushRenderTexture);
            //setAlpha(_tempFinalBrushRenderTexture, 1.0f);
            //Graphics.Blit(_tempFinalBrushRenderTexture, tempFinalBrushRenderTexture);

            Graphics.Blit(tempFinalBrushRenderTexture, _tempFinalBrushRenderTexture);
            Graphics.Blit(finalBrushRenderTexture, _finalBrushRenderTexture);
            AlphaBlendNewBrushLayer(_tempFinalBrushRenderTexture, _finalBrushRenderTexture);
            ClearBrushMarks(_tempFinalBrushRenderTexture);
            Graphics.Blit(_tempFinalBrushRenderTexture, tempFinalBrushRenderTexture);
            Graphics.Blit(_finalBrushRenderTexture, finalBrushRenderTexture);
        }
    }

    private void setAlpha(RenderTexture dest, float alpha)
    {
        int updateKernel = alphaComputeShader.FindKernel("Update");
        alphaComputeShader.SetFloat("_CanvasBrushAlpha", alpha);
        alphaComputeShader.SetTexture(updateKernel, "_CanvasOut", dest);
        alphaComputeShader.SetFloat("_CanvasWidth", dest.width);
        alphaComputeShader.SetFloat("_CanvasHeight", dest.height);
        alphaComputeShader.GetKernelThreadGroupSizes(updateKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        alphaComputeShader.Dispatch(updateKernel,
            Mathf.CeilToInt(dest.width / (float)xGroupSize),
            Mathf.CeilToInt(dest.height / (float)yGroupSize),
            1);
    }
    // dest +=  source using alpha render
    private void AlphaBlendNewBrushLayer(RenderTexture source, RenderTexture dest)
    {
        int updateKernel = alphaBlendComputeShader.FindKernel("Update");
        alphaBlendComputeShader.SetFloat("_CanvasBrushAlpha", brushColour.a);
        alphaBlendComputeShader.SetTexture(updateKernel, "_CanvasBg", dest);
        alphaBlendComputeShader.SetTexture(updateKernel, "_CanvasBrush", source);
        alphaBlendComputeShader.SetTexture(updateKernel, "_CanvasOut", dest);
        alphaBlendComputeShader.SetFloat("_CanvasWidth", source.width);
        alphaBlendComputeShader.SetFloat("_CanvasHeight", source.height);
        alphaBlendComputeShader.GetKernelThreadGroupSizes(updateKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        alphaBlendComputeShader.Dispatch(updateKernel,
            Mathf.CeilToInt(dest.width / (float)xGroupSize),
            Mathf.CeilToInt(dest.height / (float)yGroupSize),
            1);
    }

    // Draws pixels into the current pen pos
    private void MarkCurPenPos()
    {
        int updateKernel = drawComputeShader.FindKernel("Update");
        drawComputeShader.SetVector("_PreviousMousePosition", prevPenPosition);
        drawComputeShader.SetVector("_MousePosition", penPosition);
        drawComputeShader.SetBool("_MouseDown", true);
        drawComputeShader.SetFloat("_BrushSize", brushSizePressure);
        drawComputeShader.SetVector("_BrushColour", brushColour);
        drawComputeShader.SetFloat("_StrokeSmoothingInterval", strokePressIntervalSeconds);
        drawComputeShader.SetTexture(updateKernel, "_Canvas", curBrushRenderTexture);
        drawComputeShader.SetFloat("_CanvasWidth", curBrushRenderTexture.width);
        drawComputeShader.SetFloat("_CanvasHeight", curBrushRenderTexture.height);
        drawComputeShader.GetKernelThreadGroupSizes(updateKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        drawComputeShader.Dispatch(updateKernel,
            Mathf.CeilToInt(curBrushRenderTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(curBrushRenderTexture.height / (float)yGroupSize),
            1);
    }

    public void OnBrushSizeChanged(float newValue)
    {
        brushSize = newValue;
    }
}