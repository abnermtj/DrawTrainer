using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField] ComputeShader _drawComputeShader;
    [SerializeField] Color _backgroundColour;
    [SerializeField] Color _brushColour;
    [SerializeField] float _brushSize= 0.5f;
    [SerializeField] float _brushResetIntervalSeconds = 2;
    [SerializeField] float _targetResetIntervalSeconds = 1;

    [SerializeField] TargetSpawner _targetSpawner;
    [SerializeField] Target _target;
    [SerializeField] BrushSizeSlider _brushSizeSlider;
    [SerializeField, Range(0.01f, 1)] float _strokePressIntervalSeconds = 0.1f;
    RenderTexture _canvasRenderTexture;

    Vector4 _prevPenPosition;

    [SerializeField] GameObject DEBUG_BOX;
    [SerializeField] GameObject DEBUG_BOX2;

    private float _brushResetTimer;
    private float _penPressure;
    private bool _penPressed;
    private Vector4 _penPosition;
    private float _brushSizePressure = 0;
    void Start()
    {
        _brushSizeSlider.slider.SetValueWithoutNotify(_brushSizePressure);

        _canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _canvasRenderTexture.filterMode = FilterMode.Point;
        _canvasRenderTexture.enableRandomWrite = true;
        _canvasRenderTexture.Create();

        ClearScreen();

        _prevPenPosition = Pen.current.position.ReadValue();
    }

    private void ClearScreen()
    {
        int initBackgroundKernel = _drawComputeShader.FindKernel("InitBackground");
        _drawComputeShader.SetVector("_BackgroundColour", _backgroundColour);
        _drawComputeShader.SetTexture(initBackgroundKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.SetFloat("_CanvasWidth", _canvasRenderTexture.width);
        _drawComputeShader.SetFloat("_CanvasHeight", _canvasRenderTexture.height);
        _drawComputeShader.GetKernelThreadGroupSizes(initBackgroundKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        _drawComputeShader.Dispatch(initBackgroundKernel,
            Mathf.CeilToInt(_canvasRenderTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(_canvasRenderTexture.height / (float)yGroupSize),
            1);

        // clear targets
        _targetSpawner.ClearAll();
    }

    // Pen pressure in [0, 1] where 1 is max pressure
    void UpdatePen()
    {
        Pointer pointer = Pointer.current;
        Pen pen = Pen.current;
        _penPressure = pen.pressure.ReadValue();
        _penPressed = pointer.press.ReadValue() != 0? true : false; 
        _penPosition = pointer.position.ReadValue();

    }

    void Update()
    {
        UpdatePen();
        Debug.Log("_penPressure");
        Debug.Log(_penPressure);
        _brushSizePressure = _brushSize * _penPressure;

        _brushResetTimer -= Time.deltaTime;
        if (_brushResetTimer < 0)
        {
            _brushResetTimer = _brushResetIntervalSeconds;
            ClearScreen();
        }

        DEBUG_BOX.GetComponent<Image>().color = Color.white;
        Vector4 pressureColor = Color.white * _penPressure;
        pressureColor.w = 1;
        DEBUG_BOX2.GetComponent<Image>().color = pressureColor;
        if (!_brushSizeSlider.isInUse && _penPressed)
        {
            DEBUG_BOX.GetComponent<Image>().color = Color.black;
            MarkCurPenPos(); 
        }

        //Debug.Log(_penPosition);
        //Debug.Log(_prevPenPosition);
        _prevPenPosition = _penPosition;
    }

    private void MarkCurPenPos()
    {
        int updateKernel = _drawComputeShader.FindKernel("Update");
        _drawComputeShader.SetVector("_PreviousMousePosition", _prevPenPosition);
        _drawComputeShader.SetVector("_MousePosition", _penPosition);
        _drawComputeShader.SetBool("_MouseDown", true);
        _drawComputeShader.SetFloat("_BrushSize", _brushSizePressure);
        _drawComputeShader.SetVector("_BrushColour", _brushColour);
        _drawComputeShader.SetFloat("_StrokeSmoothingInterval", _strokePressIntervalSeconds);
        _drawComputeShader.SetTexture(updateKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.SetFloat("_CanvasWidth", _canvasRenderTexture.width);
        _drawComputeShader.SetFloat("_CanvasHeight", _canvasRenderTexture.height);

        _drawComputeShader.GetKernelThreadGroupSizes(updateKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        _drawComputeShader.Dispatch(updateKernel,
            Mathf.CeilToInt(_canvasRenderTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(_canvasRenderTexture.height / (float)yGroupSize),
            1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_canvasRenderTexture, dest);
    }

    public void OnBrushSizeChanged(float newValue)
    {
        _brushSize = newValue;
    }
}