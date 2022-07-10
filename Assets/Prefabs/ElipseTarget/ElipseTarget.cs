using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElipseTarget : MonoBehaviour
{

    public AudioClip[] sounds;
    private AudioSource source;

    [Range(0.1f, 0.5f)]
    public float volumeChangeMultiplier = 0.2f;
    [Range(0.1f, 0.5f)]
    public float pitchChangeMultiplier = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Sets size of target to a width x width bounding box
    public void setRandomSize()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Random.Range(50, 250), Random.Range(50, 250));
        GetComponent<RectTransform>().rotation = new Quaternion(0,0, Random.Range(0, 90), Random.Range(0, 90));
    }

    public void RemoveNoSound()
    {
        Destroy(gameObject);
    }
    public void Remove()
    {
        source.clip = sounds[Random.Range(0, sounds.Length)];
        source.volume = Random.Range(1 - volumeChangeMultiplier, 1);
        source.pitch = Random.Range(1, 1 + pitchChangeMultiplier);
        source.PlayOneShot(source.clip);
        GetComponent<RectTransform>().position += new Vector3(1000000, 199000); // No other nice way to hide the 
        Destroy(gameObject, 1);
    }
}
