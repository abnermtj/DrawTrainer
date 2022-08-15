using HSVPicker;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.UI;

public class ColorPickerTraining : MonoBehaviour
{

    public GameObject goalColorObj;
	public GameObject curColorObj;
	public ColorPicker picker;
	public bool IsGrayScaleGeneration = false; 
	public bool isGrayScaleMatching = false;

	[SerializeField] GameObject gameTimerLabel;
	[SerializeField] GameObject hitScoreLabel;
	[SerializeField] GameObject missScoreLabel;
	[SerializeField] float targetResetIntervalSeconds = 5;
	[SerializeField] float colorTolerence = 0.05f;

    private int _missScore = 0;
    private int _hitScore = 0;
	private float _gameTimer;

	Color curColor;

    Color generateNextColor()
    {
        if (IsGrayScaleGeneration)
        {
            float value = Random.Range(0f, 1f);
            return new Color(value, value, value);
        }
        else
        {
            return new Color(
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              Random.Range(0f, 1f)
              );
        }
    }
    void Start()
	{
        goalColorObj.GetComponent<Image>().color = generateNextColor();

        picker.onValueChanged.AddListener((UnityEngine.Events.UnityAction<Color>)(color =>
        {
            this.curColorObj.GetComponent<Image>().color = color;
            this.curColor = color;
        }));
    }

    bool AreColorsSimilar(Color c1, Color c2, float tolerance)
    {
        if (isGrayScaleMatching)
        {
            float goalColorLightness = RGBToLab(goalColorObj.GetComponent<Image>().color * 255).x;
            float curColorLightness = RGBToLab(curColor * 255).x;
            Debug.Log("cur " + RGBToLab(curColor * 255));
            Debug.Log("goal " + RGBToLab(goalColorObj.GetComponent<Image>().color * 255));
            return Mathf.Abs(goalColorLightness - curColorLightness) < tolerance * 255;
        }
        else
        {
            return Mathf.Abs(c1.r - c2.r) < tolerance &&
                   Mathf.Abs(c1.g - c2.g) < tolerance &&
                   Mathf.Abs(c1.b - c2.b) < tolerance;
        }
    }

    /// Converts RGB to LAB Color space
    /// r g b expected to be based on 255
    /// <returns> [L, a, b, alpha] </returns>
    private Vector4 RGBToLab(Vector4 color)
    {
        float[] xyz = new float[3];
        float[] lab = new float[3];
        float[] rgb = new float[] { color[0], color[1], color[2], color[3] };

        rgb[0] = color[0] / 255.0f;
        rgb[1] = color[1] / 255.0f;
        rgb[2] = color[2] / 255.0f;

        if (rgb[0] > .04045f)
        {
            rgb[0] = (float)Mathf.Pow((rgb[0] + .055f) / 1.055f, 2.4f);
        }
        else
        {
            rgb[0] = rgb[0] / 12.92f;
        }

        if (rgb[1] > .04045f)
        {
            rgb[1] = (float)Mathf.Pow((rgb[1] + .055f) / 1.055f, 2.4f);
        }
        else
        {
            rgb[1] = rgb[1] / 12.92f;
        }

        if (rgb[2] > .04045f)
        {
            rgb[2] = (float)Mathf.Pow((rgb[2] + .055f) / 1.055f, 2.4f);
        }
        else
        {
            rgb[2] = rgb[2] / 12.92f;
        }
        rgb[0] = rgb[0] * 100.0f;
        rgb[1] = rgb[1] * 100.0f;
        rgb[2] = rgb[2] * 100.0f;


        xyz[0] = ((rgb[0] * .412453f) + (rgb[1] * .357580f) + (rgb[2] * .180423f));
        xyz[1] = ((rgb[0] * .212671f) + (rgb[1] * .715160f) + (rgb[2] * .072169f));
        xyz[2] = ((rgb[0] * .019334f) + (rgb[1] * .119193f) + (rgb[2] * .950227f));


        xyz[0] = xyz[0] / 95.047f;
        xyz[1] = xyz[1] / 100.0f;
        xyz[2] = xyz[2] / 108.883f;

        if (xyz[0] > .008856f)
        {
            xyz[0] = (float)Mathf.Pow(xyz[0], (1.0f / 3.0f));
        }
        else
        {
            xyz[0] = (xyz[0] * 7.787f) + (16.0f / 116.0f);
        }

        if (xyz[1] > .008856f)
        {
            xyz[1] = (float)Mathf.Pow(xyz[1], 1.0f / 3.0f);
        }
        else
        {
            xyz[1] = (xyz[1] * 7.787f) + (16.0f / 116.0f);
        }

        if (xyz[2] > .008856f)
        {
            xyz[2] = (float)Mathf.Pow(xyz[2], 1.0f / 3.0f);
        }
        else
        {
            xyz[2] = (xyz[2] * 7.787f) + (16.0f / 116.0f);
        }

        lab[0] = (116.0f * xyz[1]) - 16.0f;
        lab[1] = 500.0f * (xyz[0] - xyz[1]);
        lab[2] = 200.0f * (xyz[1] - xyz[2]);

        return new Vector4(lab[0], lab[1], lab[2], color[3]);
    }

    void Update()
    {

        _gameTimer -= Time.deltaTime;
        gameTimerLabel.GetComponent<Text>().text = ((int)_gameTimer).ToString();

        if (_gameTimer < 0f)
        {
            goalColorObj.GetComponent<Image>().color = generateNextColor();

            _missScore++;
            missScoreLabel.GetComponent<Text>().text = _missScore.ToString();

            _gameTimer = targetResetIntervalSeconds;
        }


        // Win condition 
        Pointer pointer = Pointer.current;
        bool _penPressed = pointer.press.ReadValue() != 0 ? true : false;
        if (!_penPressed && AreColorsSimilar(curColor, goalColorObj.GetComponent<Image>().color, colorTolerence))
        {
            goalColorObj.GetComponent<Image>().color = generateNextColor();

            _hitScore++;
            hitScoreLabel.GetComponent<Text>().text = _hitScore.ToString();

            _gameTimer = targetResetIntervalSeconds;
        }

    }
}


