using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class FlexibleDraggableObject : MonoBehaviour, IDragHandler
{
    public GameObject Target;

    private void Start()
    {
    }

    public void OnDrag(PointerEventData data)
    {
        Debug.Log("Dragging drag handler");

        float ratio = Mathf.Abs((data.pressEventCamera.ScreenToWorldPoint(new Vector2(0, 0)) - data.pressEventCamera.ScreenToWorldPoint(new Vector2(1, 0))).x);
        Target.transform.Translate(data.delta * ratio);
    }
}