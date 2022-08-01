using UnityEngine;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] Target targetPrefab;
    [SerializeField] GameObject parent;

    public bool isAllTargetsActive;
    public bool isfirstOrLastTarget = true;

    private static List<Target> objects = new List<Target>();
    private int numTargets = 0;

    // Spawns a set of targets within a bounding box
    public void Spawn(int numberToSpawn, float originX, float originY, float width, float height, float targetWidth)
    {
        numTargets = numberToSpawn;

        for (int i = 0; i < numberToSpawn; i++)
        {
            Target target = (Instantiate(targetPrefab, new
            Vector3(originX + width * Random.Range(0, 1f),
            originY + height * Random.Range(0, 1f))
                , Quaternion.identity, parent.transform));

            target.SetSize(targetWidth);
            objects.Add(target);
        }
    }

    // Spawns a set of targets within a bounding box
    public List<Target> SpawnTwo(float originX, float originY, float width, float height, float targetWidth, float minDist, float maxDist)
    {
        numTargets = 2;
        List<Target> spawnedObjects = new List<Target>();
        while (true)
        {
            float xPos = originX + width * Random.Range(0, 1f);
            float yPos = originY + height * Random.Range(0, 1f);
            float xPos2 = originX + width * Random.Range(0, 1f);
            float yPos2 = originY + height * Random.Range(0, 1f);

            float dist = Vector2.Distance(new Vector2(xPos, yPos), new Vector2(xPos2, yPos2));
            if (dist < minDist || dist > maxDist)
            {
                continue;
            }

            Target target = (Instantiate(targetPrefab, new
            Vector3(xPos, yPos), Quaternion.identity, parent.transform));

            target.SetSize(targetWidth);
            objects.Add(target);
            spawnedObjects.Add(target);

            Target target2 = (Instantiate(targetPrefab, new
            Vector3(xPos2, yPos2), Quaternion.identity, parent.transform));

            target2.SetSize(targetWidth);
            objects.Add(target2);
            spawnedObjects.Add(target2);
            break;
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
                obj.Remove(playSound);
            }
        }

        objects.Clear();
    }
    public void ResetTargets()
    {
        foreach (Target obj in objects)
        {
            if (obj)
            {
                obj.isActive = false;
            }
        }
    }
    private int getNumActiveTargets()
    {
        int count = 0;
        foreach (Target obj in objects)
        {
            if (obj.isActive)
            {
                count++;
            }
        }
        return count;
    }
    private void Update()
    {
        int numActiveTargets = getNumActiveTargets();

        isAllTargetsActive = (numActiveTargets == numTargets);

        foreach (Target obj in objects)
        {
            if (numActiveTargets == 0)
            {
                obj.isFirstTarget = true;
            }
            else if (numActiveTargets > 0 && numActiveTargets < (numTargets - 1))
            {
                if (!obj.isActive)
                {
                    obj.isFirstTarget = false;
                    obj.isLastTarget = false;
                }
            }
            else if (numActiveTargets == numTargets - 1)
            {
                if (!obj.isActive)
                {
                    obj.isFirstTarget = false;
                    obj.isLastTarget = true;
                }

            }
        }
    }
}