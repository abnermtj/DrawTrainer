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
    [SerializeField] protected float gameTimer = 500;

    [SerializeField] protected TargetSpawner targetSpawner;
    [SerializeField] private BrushSizeSlider brushSizeSlider;
    //[SerializeField] protected GameObject BG;
    [SerializeField, Range(0.01f, 1)] private float strokePressIntervalSeconds = 0.1f;
    [SerializeField] protected float targetWidth, targetHeight;
    private RenderTexture canvasRenderTexture;
    protected Camera camera;

    private Vector4 prevPenPosition;

    [SerializeField] private GameObject DEBUG_BOX, DEBUG_BOX2;
    [SerializeField] private GameObject DEBUG_LABEL;
    [SerializeField] protected GameObject GameTimerLabel;
    [SerializeField] protected GameObject HitScoreLabel;
    [SerializeField] protected GameObject MissScoreLabel;
    [SerializeField] protected GameObject Targets;

    protected float targetResetTimer;
    protected float interpolatedPenPressure;
    protected bool mousePressed;
    protected bool penPressed;
    protected bool penJustPressed;
    protected bool penJustReleased;
    protected Vector2 strokeEndPos = Vector2.positiveInfinity;
    protected Vector4 penPosition;
    protected float brushSizePressure = 0;

    [SerializeField] private float _diameter = 1;
    [SerializeField] private Material _blitMaterial;
    [SerializeField] private RenderTexture _renderTexture;
    private RenderTexture _bufferTexture;

    [SerializeField] private Texture2D cursorTexture;

    protected void Start()
    {
        brushSizeSlider.slider.SetValueWithoutNotify(brushSizePressure);

        canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24)
        {
            filterMode = FilterMode.Point,
            enableRandomWrite = true,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm
        };

        //Graphics.Blit(canvasRenderTexture, _renderTexture);
        //_bufferTexture = RenderTexture.GetTemporary(canvasRenderTexture.width, canvasRenderTexture.height);
        //BG.GetComponent<RawImage>().texture = canvasRenderTexture;


        canvasRenderTexture.Create();


        ResetBoard(true);

        prevPenPosition = Pen.current.position.ReadValue();
        targetResetTimer = targetResetIntervalSeconds;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.ForceSoftware); // This centers a custom cursor on the mouse

        camera = GetComponent<Camera>();
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
        DEBUG_LABEL.GetComponent<Text>().text = penPosition.ToString();

        if (penJustReleased)
        {
            strokeEndPos = penPosition;
        }
    }

    protected void Update()
    {
        UpdatePen();
        UpdateBrush();
        UpdateTimers();

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

    private void UpdateTimers()
    {
        gameTimer -= Time.deltaTime;
        GameTimerLabel.GetComponent<Text>().text = ((int)gameTimer).ToString();
    }

    virtual protected void ResetBoard(bool isWin)
    {
        ClearBrushMarks();

        targetResetTimer = targetResetIntervalSeconds;
        targetSpawner.ClearAll(playSound: isWin);
        targetSpawner.Spawn(numTargets, Screen.width / 2.0f, Screen.height / 2.0f , 350, 250, targetWidth, targetHeight, 20, 140, camera);
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
        //Stamp(new Vector2Int((int)penPosition.x, (int)penPosition.y));
    }



    //private void UpdateTexture()
    //{
    //    Debug.Log("STAMPED)");
    //    Graphics.Blit(_renderTexture, _bufferTexture, _blitMaterial);
    //    Graphics.Blit(_bufferTexture, _renderTexture);
    //}

    //private void Stamp(Vector2Int pos)
    //{
    //    var radius = _diameter / 2f;
    //    var size = new Vector2(_diameter, _diameter);
    //    var offset = new Vector2(radius, radius);
    //    _blitMaterial.SetVector("_size", size);
    //    _blitMaterial.SetVector("_sPos", pos - offset);
    //    _blitMaterial.SetColor("_color", brushColour);
    //    UpdateTexture();
    //}


    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(canvasRenderTexture, dest);
    }

    public void OnBrushSizeChanged(float newValue)
    {
        brushSize = newValue;
    }
}