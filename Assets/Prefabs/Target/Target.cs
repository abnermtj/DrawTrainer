using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Sets size of target to a width x width bounding box
    public void setSize(float width)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, width);
    }
    public void Remove()
    {
        Destroy(gameObject);
    }
}
