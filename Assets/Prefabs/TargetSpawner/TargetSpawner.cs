using UnityEngine;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    public Target objectToSpawn;
    public GameObject parent;
    public int numberToSpawn;
    public int limit = 20;
    public int distance = 200;
    public float rate;
    private static List<Target> objects = new List<Target>();

    float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = rate;
    }

    // Update is called once per frame
    void Update()
    {
        if (parent.transform.childCount < limit)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                for (int i = 0; i < numberToSpawn; i++)
                {
                    Debug.Log("SPAWN");
                    Target clone = (Instantiate(objectToSpawn, new
                    Vector3(this.transform.position.x + distance *
                    GetModifier(), this.transform.position.y + distance *
                    GetModifier())
                        , Quaternion.identity, parent.transform));

                    objects.Add(clone);
                }
                spawnTimer = rate;
            }
        }
    }

    public void ClearAll()
    {
        //Debug.Log("CLearing");

        foreach (Target obj in objects)
        {
            //Debug.Log(objects.Count);
            obj.ClearAll();
        }

        objects.Clear();
    }
    float GetModifier()
    {
        float modifier = Random.Range(0f, 1f);
        if (Random.Range(0, 2) > 0)
            return -modifier;
        else
            return modifier;
    }
}