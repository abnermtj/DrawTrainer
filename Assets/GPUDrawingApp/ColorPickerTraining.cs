using HSVPicker;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.UI;

public class ColorPickerTraining : MonoBehaviour
{

	public GameObject GoalColor;
	public GameObject CurColor;
	public ColorPicker picker;

	[SerializeField] GameObject _GameTimerLabel;
	[SerializeField] GameObject _HitScoreLabel;
	[SerializeField] GameObject _MissScoreLabel;
	[SerializeField] float _gameTimerResetTime = 5;

    private int _missScore = 0;
    private int _hitScore = 0;
	private float _gameTimer;

	Color curColor;


	// Use this for initialization
	void Start()
	{
        Color randomColor= new Color(
          Random.Range(0f, 1f),
          Random.Range(0f, 1f),
          Random.Range(0f, 1f)
          );
        GoalColor.GetComponent<Image>().color = randomColor;
		picker.onValueChanged.AddListener(color =>
		{
            CurColor.GetComponent<Image>().color = color;
			curColor = color;
		});
	}

	bool AreColorsSimilar(Color c1, Color c2, float tolerance)
	{
		return Mathf.Abs(c1.r - c2.r) < tolerance &&
			   Mathf.Abs(c1.g - c2.g) < tolerance &&
			   Mathf.Abs(c1.b - c2.b) < tolerance;
	}

	// Update is called once per frame
	void Update()
	{

		_gameTimer -= Time.deltaTime;
        _GameTimerLabel.GetComponent<Text>().text = _gameTimer.ToString();

		if (_gameTimer < 0f) {
			Color randomColor = new Color(
			  Random.Range(0f, 1f),
			  Random.Range(0f, 1f),
			  Random.Range(0f, 1f)
		  );
			GoalColor.GetComponent<Image>().color = randomColor;
			_missScore++;
			_MissScoreLabel.GetComponent<Text>().text = _missScore.ToString();
			_gameTimer = _gameTimerResetTime;
		}


		Pointer pointer = Pointer.current;
		bool _penPressed = pointer.press.ReadValue() != 0 ? true : false;

		if (!_penPressed && AreColorsSimilar(curColor, GoalColor.GetComponent<Image>().color, 0.1f))
        {
			Color randomColor= new Color(
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              Random.Range(0f, 1f)
          );
			GoalColor.GetComponent<Image>().color = randomColor;
			_hitScore++;
			_HitScoreLabel.GetComponent<Text>().text = _hitScore.ToString();
			_gameTimer = _gameTimerResetTime;
		}

	}
}


