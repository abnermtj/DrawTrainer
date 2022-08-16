using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Target : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip[] sounds;
    public bool isActive = false;
    public bool isLastTarget = false;
    public bool isFirstTarget = false;
    private AudioSource source;

    public Color activeColor = Color.green;
    public Color inActiveColor = Color.white;

    [Range(0.1f, 0.5f)]
    public float volumeChangeMultiplier = 0.2f;

    [Range(0.1f, 0.5f)]
    public float pitchChangeMultiplier = 0.2f;

    // Start is called before the first frame update
    protected void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isActive)
        {
            gameObject.GetComponent<Image>().color = activeColor;
        }
        else
        {
            gameObject.GetComponent<Image>().color = inActiveColor;
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isActive = true;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (isFirstTarget)
        {
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer.press.ReadValue() != 0)
        {
            isActive = true;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (isLastTarget)
        {
            isActive = false;
        }
    }

    // Sets size of target to a width x width bounding box
    public void SetSize(float width, float height)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public void SetRandomSize(int minLen, int maxLen)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Random.Range(minLen, maxLen), Random.Range(minLen, maxLen));
    }

    public void SetRandomRotation()
    {
        transform.rotation = new Quaternion(0, 0, Random.Range(0, 90), Random.Range(0, 90));
    }

    public void Remove(bool playSound)
    {
        if (playSound)
        {
            source.clip = sounds[Random.Range(0, sounds.Length)];
            source.volume = Random.Range(1 - volumeChangeMultiplier, 1);
            source.pitch = Random.Range(1, 1 + pitchChangeMultiplier);
            source.PlayOneShot(source.clip);
            transform.position += new Vector3(1000000, 199000); // No other nice way to hide the
            Destroy(gameObject, 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}