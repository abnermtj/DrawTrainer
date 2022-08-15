using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HandlerType
{
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    TopLeft,
    Top
}

[RequireComponent(typeof(EventTrigger))]
public class FlexibleResizeHandler :  MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public HandlerType Type;
    public RectTransform Target;
    public Vector2 MinimumDimmensions = new Vector2(50, 50);
    public Vector2 MaximumDimmensions = new Vector2(800, 800);
        private bool isDragging = false;
    
    private float horiztonalInset = 0;
    private float verticalInset = 0;

    void Start()
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Need this defined for OnPointerUp to work
    }

    public void Update()
    {
    }
    public void OnDrag(PointerEventData data)
    {

        RectTransform.Edge? horizontalEdge = null;
        RectTransform.Edge? verticalEdge = null;

        var savePos = Target.localPosition;
        switch (Type)
        {
            case HandlerType.TopRight:
                horizontalEdge = RectTransform.Edge.Left;
                verticalEdge = RectTransform.Edge.Bottom;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.BottomLeft);
                break;
            case HandlerType.Right:
                horizontalEdge = RectTransform.Edge.Left;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleLeft);
                break;
            case HandlerType.BottomRight:
                horizontalEdge = RectTransform.Edge.Left;
                verticalEdge = RectTransform.Edge.Top;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopLeft);
                break;
            case HandlerType.Bottom:
                verticalEdge = RectTransform.Edge.Top;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopCenter);
                break;
            case HandlerType.BottomLeft:
                horizontalEdge = RectTransform.Edge.Right;
                verticalEdge = RectTransform.Edge.Top;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopRight);
                break;
            case HandlerType.Left:
                horizontalEdge = RectTransform.Edge.Right;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleRight);
                break;
            case HandlerType.TopLeft:
                horizontalEdge = RectTransform.Edge.Right;
                verticalEdge = RectTransform.Edge.Bottom;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.BottomRight);
                break;
            case HandlerType.Top:
                verticalEdge = RectTransform.Edge.Bottom;
                Target.GetComponent<RectTransform>().SetAnchor(AnchorPresets.BottonCenter);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Target.localPosition = savePos;

        if (!isDragging)
        {
            isDragging = true;
            switch (Type)
            {
                case HandlerType.TopRight:
                    horiztonalInset = Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    verticalInset = Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    break;
                case HandlerType.BottomRight:
                    horiztonalInset = Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    verticalInset = -Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    break;
                case HandlerType.Top:
                    verticalInset = Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    break;
                case HandlerType.Bottom:
                    verticalInset = -Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    break;

                case HandlerType.BottomLeft:
                    horiztonalInset = -Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    verticalInset = -Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    break;
                case HandlerType.Right:
                    horiztonalInset = Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    break;
                case HandlerType.Left:
                    horiztonalInset = -Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    break;
                case HandlerType.TopLeft:
                    verticalInset = Target.anchoredPosition.y - Target.pivot.y * Target.rect.height;
                    horiztonalInset = -Target.anchoredPosition.x - Target.pivot.x * Target.rect.width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        if (horizontalEdge != null)
        {
            if (horizontalEdge == RectTransform.Edge.Right)
                Target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)horizontalEdge, // Drag Left
                    horiztonalInset,
                    Mathf.Clamp(Target.rect.width - data.delta.x , MinimumDimmensions.x, MaximumDimmensions.x));
            else
                Target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)horizontalEdge, // Drag Right
                    horiztonalInset,
                    Mathf.Clamp(Target.rect.width + data.delta.x, MinimumDimmensions.x, MaximumDimmensions.x));
        }
        if (verticalEdge != null)
        {
            if (verticalEdge == RectTransform.Edge.Top)
                Target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)verticalEdge, // Drag Down
                    verticalInset,
                    Mathf.Clamp(Target.rect.height - data.delta.y, MinimumDimmensions.y, MaximumDimmensions.y));
            else
                Target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)verticalEdge, // Drag Up
                    verticalInset,
                    Mathf.Clamp(Target.rect.height + data.delta.y, MinimumDimmensions.y, MaximumDimmensions.y));
        }
    }
}
