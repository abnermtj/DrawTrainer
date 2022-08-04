using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class LineTarget : Target
{
    [SerializeField] private Target target;
    [SerializeField] private Target target2;
    [SerializeField] private Sprite visibleImage;
    [Range (0.0f, 0.5f)]
    public float percentageTarget = 0;


    void Start()
    {
        base.Start();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("as");
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("xs");
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("av");

        if (!(Pen.current.press.ReadValue() != 0)) {
            return;
        }

        target.GetComponent<Image>().sprite = visibleImage;
        target.inActiveColor = Color.black;
        target2.GetComponent<Image>().sprite = visibleImage;
        target2.inActiveColor = Color.black;
    }
    public void Update()
    {
        float width = GetComponent<RectTransform>().rect.width;
        float actualPercent = .5f - percentageTarget; // inverts the percentage 
        target.transform.localPosition = new Vector3 (width * actualPercent , target.transform.localPosition.y);
        target2.transform.localPosition = new Vector3 (-width * actualPercent, target.transform.localPosition.y);


        if (target.isActive || target2.isActive)
        {
            isActive = true;
        }
    }
}
