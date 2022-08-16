using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    private static Rect GetScreenPositionFromRect(RectTransform rt, Camera camera)
    {
        // getting the world corners
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // getting the screen corners
        for (var i = 0; i < corners.Length; i++)
        {
            //Debug.Log(corners[i]);
            corners[i] = camera.WorldToScreenPoint(corners[i]);
            //Debug.Log(corners[i]);
        }
        // getting the top left position of the transform
        var position = (Vector2)corners[1];
        // inverting the y axis values, making the top left corner = 0.
        position.y = Screen.height - position.y;
        // calculate the size, width and height, in pixel format
        var size = corners[2] - corners[0];

        return new Rect(position, size);
    }

    private static void CopyRectTransformSize(RectTransform copyFrom, RectTransform copyTo)
    {
        copyTo.anchorMin = copyFrom.anchorMin;
        copyTo.anchorMax = copyFrom.anchorMax;
        copyTo.anchoredPosition = copyFrom.anchoredPosition;
        copyTo.sizeDelta = copyFrom.sizeDelta;
    }
}