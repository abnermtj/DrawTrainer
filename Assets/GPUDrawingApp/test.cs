using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    void OnMouseOver()
    {
        gameObject.GetComponent<Image>().color = Color.green;
    }

    private void OnMouseExit()
    {
        gameObject.GetComponent<Image>().color = Color.white;
        
    }
}

