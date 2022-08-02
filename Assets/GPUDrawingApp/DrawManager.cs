using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField] private ComputeShader drawComputeShader;
    [SerializeField] private Color backgroundColour;
    [SerializeField] private Color brushColour;
    [SerializeField] private float brushSize = 0.5f;
    [SerializeField] protected float targetResetIntervalSeconds = 1;
    [SerializeField] protected int numTargets = 2;

    [SerializeField] protected TargetSpawner targetSpawner;
    [SerializeField] private BrushSizeSlider brushSizeSlider;
    [SerializeField, Range(0.01f, 1)] private float strokePressIntervalSeconds = 0.1f;
    [SerializeField] protected float targetWidth, targetHeight;
    private RenderTexture canvasRenderTexture;

    private Vector4 prevPenPosition;

    [SerializeField] private GameObject DEBUG_BOX, DEBUG_BOX2;
    [SerializeField] protected GameObject GameTimerLabel;
    [SerializeField] protected GameObject HitScoreLabel;
    [SerializeField] protected GameObject MissScoreLabel;

    protected float targetResetTimer;
    protected float interpolatedPenPressure;
    protected bool mousePressed;
    protected bool penPressed;
    protected bool penJustPressed;
    protected bool penJustReleased;
    protected Vector2 strokeEndPos = Vector2.positiveInfinity;
    protected Vector4 penPosition;
    protected float brushSizePressure = 0;


    [SerializeField] private Texture2D cursorTexture;

    protected void Start()
    {
        brushSizeSlider.slider.SetValueWithoutNotify(brushSizePressure);

        canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24)
        {
            filterMode = FilterMode.Bilinear,
            enableRandomWrite = true
        };

        canvasRenderTexture.Create();

        ResetBoard(true);

        prevPenPosition = Pen.current.position.ReadValue();
        targetResetTimer = targetResetIntervalSeconds;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.ForceSoftware); // This centers a custom cursor on the mouse

    }

    // Removes all targets
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

        if (penJustReleased)
        {
            strokeEndPos = penPosition;
        }
    }

    protected void Update()
    {
        UpdatePen();
        UpdateBrush();

        // DEBUG
        DEBUG_BOX.GetComponent<Image>().color = Color.white;
        Vector4 pressureColor = Color.white * interpolatedPenPressure;
        pressureColor.w = 1;
        DEBUG_BOX2.GetComponent<Image>().color = pressureColor;
        // DEBUG

        if (!brushSizeSlider.isInUse && (penPressed || mousePressed))
        {
            DEBUG_BOX.GetComponent<Image>().color = Color.black;
            MarkCurPenPos();
        }
    }

    virtual protected void ResetBoard(bool isWin)
    {
        ClearBrushMarks();

        targetResetTimer = targetResetIntervalSeconds;
        targetSpawner.ClearAll(playSound: isWin);
        targetSpawner.Spawn(numTargets, Screen.width / 2 - 150, Screen.height / 2 - 100, 350, 280, targetWidth, targetHeight, 20, 140);
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