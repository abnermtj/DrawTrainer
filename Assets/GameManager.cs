using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject Button2;

    private GameObject Button3;
    private GameObject Button4;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public static void switchScenes(string name)
    {
        SceneManager.LoadScene(name);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}