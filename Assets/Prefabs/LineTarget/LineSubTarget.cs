using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineSubTarget : Target
{
    new void Start()
    {
        // Method intentionally left empty.
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        isActive = true;
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        isActive = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        // Method intentionally left empty.
    }

}
