using UnityEngine;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] Target targetPrefab;
    [SerializeField] GameObject parent;

    private static List<Target> objects = new List<Target>();

    // Spawns a set of targets within a bounding box
    public void Spawn(int numberToSpawn, float originX, float originY, float width, float height, float targetWidth)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Target target = (Instantiate(targetPrefab, new
            Vector3(originX + width * Random.Range(0, 1f),
            originY + height * Random.Range(0,1f))
                , Quaternion.identity, parent.transform));

            target.setSize(targetWidth);
            objects.Add(target);
        }
    }

    // Spawns a set of targets within a bounding box
    public  List<Target> SpawnTwo(float originX, float originY, float width, float height, float targetWidth)
    {
        List<Target> spawnedObjects = new List<Target>();
        for (int i = 0; i < 2; i++)
        {
            Target target = (Instantiate(targetPrefab, new
            Vector3(originX + width * Random.Range(0, 1f),
            originY + height * Random.Range(0,1f))
                , Quaternion.identity, parent.transform));

            target.setSize(targetWidth);
            objects.Add(target);
            spawnedObjects.Add(target);
        }

        return spawnedObjects;
    }


    // Removes all spawned targets
    public void ClearAll(bool playSound)
    {
        foreach (Target obj in objects)
        {
            if (obj)
            {
                if (playSound)
                {
                    obj.Remove();
                }
                else
                {
                    obj.RemoveNoSound();
                }
            }
        }

        objects.Clear();
    }
}