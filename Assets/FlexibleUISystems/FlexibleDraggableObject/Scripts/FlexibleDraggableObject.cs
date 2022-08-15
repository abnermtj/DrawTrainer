using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class FlexibleDraggableObject : MonoBehaviour
{
    public GameObject Target;
    private EventTrigger _eventTrigger;

    void Start ()
    {
        _eventTrigger = GetComponent<EventTrigger>();
        _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
    }

    void OnDrag(BaseEventData data)
    {
        PointerEventData pointerEventData = (PointerEventData) data;
        float ratio = Mathf.Abs((pointerEventData.pressEventCamera.ScreenToWorldPoint(new Vector2(0, 0)) - pointerEventData.pressEventCamera.ScreenToWorldPoint(new Vector2(1, 0))).x);
        PointerEventData ped = (PointerEventData) data;
        Target.transform.Translate(ped.delta * ratio);
    }
}