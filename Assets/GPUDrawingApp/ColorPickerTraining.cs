using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerTraining : MonoBehaviour
{

	public GameObject ChosenColorLabel;
	public ColorPicker picker;

	// Use this for initialization
	void Start()
	{
		picker.onValueChanged.AddListener(color =>
		{
			ChosenColorLabel.GetComponent<Image>().color= color;
		});
        ChosenColorLabel.GetComponent<Image>().color= picker.CurrentColor;
	}

	// Update is called once per frame
	void Update()
	{
	}
}


