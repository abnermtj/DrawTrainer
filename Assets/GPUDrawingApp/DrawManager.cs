using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField] private ComputeShader drawComputeShader;
    [SerializeField] private Color backgroundColour;

    // Brush 
    [SerializeField] private BrushSizeSlider brushSizeSlider;
    [SerializeField] private Color brushColour;
    [SerializeField] private float brushSize = 0.5f;
    [SerializeField, Range(0.01f, 1)] private float strokePressIntervalSeconds = 0.1f;

    protected bool penPressed;
    protected bool penJustPressed;
    protected bool penJustReleased;
    protected bool mousePressed;
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
    [SerializeField] private GameObject DEBUG_BOX, DEBUG_BOX2;
    [SerializeField] private GameObject DEBUG_LABEL;
    [SerializeField] protected GameObject comboPrefab;
    [SerializeField] protected GameObject canvas;
    [SerializeField] protected GameObject winLabel;
    [SerializeField] protected GameObject targets;
    [SerializeField] private Texture2D cursorTexture;
    private RenderTexture canvasRenderTexture;
    new protected Camera camera;

    // Score 
    [SerializeField] protected GameObject gameTimerLabel;
    [SerializeField] protected GameObject hitScoreLabel;
    [SerializeField] protected GameObject missScoreLabel;
    [SerializeField] protected float gameTimer = 500;
    protected int missScore = 0;
    protected int comboScore = 0;
    protected int hitScore = 0;

    protected void Start()
    {
        // Render texture is used for line drawing
        canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm
        };
        canvasRenderTexture.Create();

        ResetBoard(isWin: true);

        brushSizeSlider.slider.SetValueWithoutNotify(brushSizePressure);

        prevPenPosition = Pen.current.position.ReadValue();
        targetResetTimer = targetResetIntervalSeconds;
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.ForceSoftware); // This centers a custom cursor on the mouse

        camera = GetComponent<Camera>();
    }

    protected void ClearBrushMarks()
    {
        int initBackgroundKernel = drawComputeShader.FindKernel("InitBackground");
        drawComputeShader.SetVector("_BackgroundColour", backgroundColour);
        drawComputeShader.SetTexture(initBackgroundKernel, "_Canvas", canvasRenderTexture);
        drawComputeShader.SetFloat("_CanvasWidth", canvasRenderTexture.width);
        drawComputeShader.SetFloat("_CanvasHeight", canvasRenderTexture.height);
        drawComputeShader.GetKernelThreadGroupSizes(initBackgroundKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        drawComputeShader.Dispatch(initBackgroundKernel,
            Mathf.CeilToInt(canvasRenderTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(canvasRenderTexture.height / (float)yGroupSize),
            1);
    }

    protected virtual void ResetBoard(bool isWin)
    {
        ClearBrushMarks();

        targetResetTimer = targetResetIntervalSeconds;
        targetSpawner.ClearAll(playSound: isWin);

        int rInt = Random.Range(minTargets, maxTargets + 1);
        targetSpawner.Spawn(rInt, Screen.width / 2.0f, Screen.height / 2.0f - 100, 350, 250, targetWidth, targetHeight, 20, 140, camera);
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
        bool _prevPenPressed = penPressed;
        penPressed = pen.press.ReadValue() != 0;
        mousePressed = pointer.press.ReadValue() != 0 && !penPressed;
        if (penPressed && !_prevPenPressed)
        {
            penJustPressed = true;
            penJustReleased = false;
        }
        else if (!penPressed && _prevPenPressed)
        {
            penJustPressed = false;
            penJustReleased = true;
        }
        else
        {
            penJustPressed = false;
            penJustReleased = false;
        }

        // Pen position
        prevPenPosition = penPosition;
        penPosition = pointer.position.ReadValue();
        DEBUG_LABEL.GetComponent<Text>().text = penPosition.ToString();

        if (penJustReleased)
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
        UpdatePen();
        UpdateBrush();
        UpdateTimers();
        UpdateCommonLabels();

        // DEBUG
        if (DEBUG_BOX && DEBUG_BOX2)
        {
            DEBUG_BOX.GetComponent<Image>().color = Color.white;
            Vector4 pressureColor = Color.white * interpolatedPenPressure;
            pressureColor.w = 1;
            DEBUG_BOX2.GetComponent<Image>().color = pressureColor;
        }
        // DEBUG

        if (!brushSizeSlider.isInUse && (penPressed || mousePressed))
        {
            if (DEBUG_BOX && DEBUG_BOX2)
                DEBUG_BOX.GetComponent<Image>().color = Color.black;
            MarkCurPenPos();
        }

        if (penJustReleased)
        {
            targetSpawner.ResetTargets();
        }
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
        drawComputeShader.SetTexture(updateKernel, "_Canvas", canvasRenderTexture);
        drawComputeShader.SetFloat("_CanvasWidth", canvasRenderTexture.width);
        drawComputeShader.SetFloat("_CanvasHeight", canvasRenderTexture.height);

        drawComputeShader.GetKernelThreadGroupSizes(updateKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        drawComputeShader.Dispatch(updateKernel,
            Mathf.CeilToInt(canvasRenderTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(canvasRenderTexture.height / (float)yGroupSize),
            1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(canvasRenderTexture, dest);
    }

    public void OnBrushSizeChanged(float newValue)
    {
        brushSize = newValue;
    }
}