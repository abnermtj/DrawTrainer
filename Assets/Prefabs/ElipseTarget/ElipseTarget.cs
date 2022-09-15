using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ElipseTarget : Target
{
    [SerializeField] private List<Target> targets;
    [SerializeField] private Sprite visibleImage;
    [SerializeField] private float a;

    private List<Vector2> targetOrigLocalPos = new List<Vector2>();

    protected void Start()
    {
        base.Start(); // Not sure if this is called twice given the function header
        foreach (Target t in targets)
        {
            targetOrigLocalPos.Add(t.transform.localPosition);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // Do nothing
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        // Do nothing
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
    }

    public override void Deactivate()
    {
        foreach (Target t in targets)
        {
            t.isActive = false;
        }
        isActive = false;
    }

    public void Update()
    {
        base.Update();
        bool tempIsActive = true;
        int i = 0;
        foreach (Target t in targets)
        {
            t.transform.localPosition = targetOrigLocalPos[i] * GetComponent<RectTransform>().sizeDelta / origSizeDelta;
            if (!t.isActive)
            {
                tempIsActive = false;
            }
            i++;
        }
        isActive = tempIsActive;
    }
}