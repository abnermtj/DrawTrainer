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
    public void Spawn(int numberToSpawn, float originX, float originY, float width, float height, float targetWidth, float minDist, float maxDist)
    {
        numTargets = numberToSpawn;

        float xPos, yPos, xPos2, yPos2;
        if (numberToSpawn == 1)
        {
            xPos = originX + width * Random.Range(0, 1f);
            yPos = originY + height * Random.Range(0, 1f);

            Target target = Instantiate(targetPrefab, new Vector3(xPos, yPos), Quaternion.identity, parent.transform);
            target.SetSize(targetWidth);
            objects.Add(target);

        }
        else
        {

            // Get start and end points
            List<Vector2> positions = new List<Vector2>();
            while (true)
            {
                xPos = originX + width * Random.Range(0, 1f);
                yPos = originY + height * Random.Range(0, 1f);
                xPos2 = originX + width * Random.Range(0, 1f);
                yPos2 = originY + height * Random.Range(0, 1f);

                float dist = Vector2.Distance(new Vector2(xPos, yPos), new Vector2(xPos2, yPos2));
                if (dist > minDist && dist < maxDist)
                {
                    break;
                }
            }
            positions.Add(new Vector2(xPos, yPos));
            positions.Add(new Vector2(xPos2, yPos2));

            // Generate intermediate points in a bounding box
            int numIntermediatePoints = numberToSpawn - 2;
            for (int i = 0; i < numIntermediatePoints; i++)
            {
                positions.Add(new Vector2(Random.Range(xPos, xPos2), Random.Range(yPos, yPos2)));
            }


            // Create all targets
            Debug.Log(positions.Count);
            foreach (Vector3 pos in positions)
            {
                Target target = Instantiate(targetPrefab, pos, Quaternion.identity, parent.transform);
                target.SetSize(targetWidth);
                objects.Add(target);
            }
        }
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