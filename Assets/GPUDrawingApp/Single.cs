using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;


public class Single : MonoBehaviour
{
    [SerializeField] ComputeShader _drawComputeShader;
    [SerializeField] Color _backgroundColour;
    [SerializeField] Color _brushColour;
    [SerializeField] float _brushSize= 0.5f;
    [SerializeField] float _brushResetIntervalSeconds = 2;
    [SerializeField] float _targetResetIntervalSeconds = 1;
    [SerializeField] float _gameTimer = 500;

    [SerializeField] TargetSpawner _targetSpawner;
    [SerializeField] BrushSizeSlider _brushSizeSlider;
    [SerializeField, Range(0.01f, 1)] float _strokePressIntervalSeconds = 0.1f;
    [SerializeField] float _targetSize;
    RenderTexture _canvasRenderTexture;

    Vector4 _prevPenPosition;

    [SerializeField] GameObject DEBUG_BOX;
    [SerializeField] GameObject DEBUG_BOX2;

    [SerializeField] GameObject _GameTimerLabel;
    [SerializeField] GameObject _HitScoreLabel;
    [SerializeField] GameObject _MissScoreLabel;

    private int _missScore = 0;
    private int _hitScore = 0;
    private float _brushResetTimer;
    private float _targetResetTimer;
    private float _penPressure;
    private bool _penPressed;
    private bool _penJustPressed;
    private bool _penJustReleased;
    private Vector2 _strokeStartPos = Vector2.positiveInfinity;  // Needs to be different from target pos so that it doesn't match immediately
    private Vector2 _strokeEndPos = Vector2.positiveInfinity;
    private Vector2 _targetPos1;
    private Vector2 _targetPos2;
    private Vector4 _penPosition;
    private float _brushSizePressure = 0;

    [SerializeField] private GameObject particles;


    // Here reference the camera component of the particles camera
    [SerializeField] private Camera particlesCamera;

    // Adjust the resolution in pixels
    [SerializeField] private Vector2Int imageResolution = new Vector2Int(256, 256);

    // Reference the RawImage in your UI
    [SerializeField] private RawImage targetImage;

    private RenderTexture renderTexture;
    public Texture2D cursorTexture;

    void Start()
    {
        _brushSizeSlider.slider.SetValueWithoutNotify(_brushSizePressure);

        _canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _canvasRenderTexture.filterMode = FilterMode.Bilinear;
        _canvasRenderTexture.enableRandomWrite = true;
        _canvasRenderTexture.Create();
        //_canvasRenderTexture.pare

        ClearScreen();

        _prevPenPosition = Pen.current.position.ReadValue();
        _brushResetTimer = _brushResetIntervalSeconds;
        _targetResetTimer = _targetResetIntervalSeconds;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(cursorTexture, new Vector2 (cursorTexture.width/2, cursorTexture.height/2), CursorMode.Auto); // This centers a custom cursor on the mouse

        if (!particlesCamera) particlesCamera = GetComponent<Camera>();

        //renderTexture = new RenderTexture(imageResolution.x, imageResolution.y, 32);
        //particlesCamera.targetTexture = renderTexture;

        //targetImage.texture = renderTexture;
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
        _targetSpawner.ClearAll(false);
    }

    // Pen pressure in [0, 1] where 1 is max pressure
    void UpdatePen()
    {
        Pointer pointer = Pointer.current;
        Pen pen = Pen.current;
        float cur_pen_pressure = pen.pressure.ReadValue();
        _penPressure = Mathf.Lerp(_penPressure, cur_pen_pressure, 0.9f); // TODO Take out float into a constant


        bool _prevPenPressed = _penPressed;
        _penPressed = pointer.press.ReadValue() != 0 ? true : false;
        if (_penPressed && !_prevPenPressed)
        {
            _penJustPressed = true;
            _penJustReleased = false;
        }
        else if (!_penPressed && _prevPenPressed)
        {
            _penJustPressed = false;
            _penJustReleased = true;
        }
        else
        {
            _penJustPressed = false;
            _penJustReleased = false;
        }

        _penPosition = pointer.position.ReadValue();

    }

    void Update()
    {
        UpdatePen();
        _brushSizePressure = _brushSize * _penPressure;

        _gameTimer -= Time.deltaTime;
        _GameTimerLabel.GetComponent<Text>().text = _gameTimer.ToString();

        _brushResetTimer -= Time.deltaTime;
        if (_brushResetTimer < 0)
        {
            _brushResetTimer = _brushResetIntervalSeconds;
            ClearScreen();
        }

        _targetResetTimer -= Time.deltaTime;
        if (_targetResetTimer < 0)
        {
            _targetResetTimer = _targetResetIntervalSeconds;
            _targetSpawner.ClearAll(false);
            //List<Target> targets = _targetSpawner.SpawnTwo(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200, _targetSize); // TODO refactor this duplicate code
            List<Target> targets = _targetSpawner.SpawnTwo(Screen.width / 2 - 150, Screen.height / 2 - 100, 350, 280, _targetSize, 20, 140);
            _targetPos1 = targets[0].GetComponent<RectTransform>().position;
            _targetPos2 = targets[1].GetComponent<RectTransform>().position;

            _missScore++;
            _MissScoreLabel.GetComponent<Text>().text = _missScore.ToString();
        }

        // DEBUG
        DEBUG_BOX.GetComponent<Image>().color = Color.white;
        Vector4 pressureColor = Color.white * _penPressure;
        pressureColor.w = 1;
        DEBUG_BOX2.GetComponent<Image>().color = pressureColor;
        // DEBUG

        if (_penJustPressed)
        {
            _strokeStartPos = _penPosition;
        }
        if (_penJustReleased)
        {
            _strokeEndPos = _penPosition;
        }

        Debug.Log("_penPosition");
        Debug.Log(_penPosition);
        Debug.Log("Storkre");
        Debug.Log(particles.GetComponent<RectTransform>().position);
        if ( Vector3.Distance(_strokeEndPos, _targetPos2) < _targetSize || Vector3.Distance(_strokeEndPos, _targetPos1) < _targetSize)
        {
            particles.GetComponent<RectTransform>().position = _strokeEndPos;
            particles.GetComponent<ParticleSystem>().Play();

            // Here we have successfully hit the points
            _hitScore++;
            _HitScoreLabel.GetComponent<Text>().text = _hitScore.ToString();

            _targetSpawner.ClearAll(true);
            ClearScreen();
            //List<Target> targets = _targetSpawner.SpawnTwo(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200, _targetSize);
            List<Target> targets = _targetSpawner.SpawnTwo(Screen.width / 2 - 150, Screen.height / 2 - 100, 350, 280, _targetSize,20,140);
            _targetPos1 = targets[0].GetComponent<RectTransform>().position;
            _targetPos2 = targets[1].GetComponent<RectTransform>().position;


            _targetResetTimer = _targetResetIntervalSeconds;
            _brushResetTimer = _brushResetIntervalSeconds;
        }

        if (!_brushSizeSlider.isInUse && _penPressed)
        {
            DEBUG_BOX.GetComponent<Image>().color = Color.black;
            MarkCurPenPos();
        }

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