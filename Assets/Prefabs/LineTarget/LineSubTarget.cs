using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineSubTarget : Target
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("HERE");
        isActive = true;
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("HERE2");
        isActive = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
    }

}
