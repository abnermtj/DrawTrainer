using UnityEngine;
using UnityEngine.EventSystems;

public class LineTarget : Target
{
    [SerializeField] private Target target;
    [SerializeField] private Target target2;
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
