using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBox : MonoBehaviour
{
    public bool isEnabled = false;
    private bool prevIsEnabled = true;

    private void Start()
    {
        foreach (Transform handler in transform)
        {
            FlexibleResizeHandler resizeHandler = handler.gameObject.GetComponent<FlexibleResizeHandler>();
            if (resizeHandler) { resizeHandler.Target = GetComponent<RectTransform>(); }

            FlexibleDraggableObject dragHandler = handler.gameObject.GetComponent<FlexibleDraggableObject>();
            if (dragHandler) { dragHandler.Target = gameObject; }
        }
    }

    private void Update()
    {
        if (isEnabled != prevIsEnabled)
        {
            Debug.Log("CHANGE");
            prevIsEnabled = isEnabled;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(isEnabled);
            }
        }
    }
}