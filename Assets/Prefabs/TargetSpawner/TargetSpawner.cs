using UnityEngine;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] protected Target targetPrefab;
    [SerializeField] protected GameObject targetParent;
    [SerializeField] protected bool randomSize = false;
    [SerializeField] protected bool randomRotation = false;
    [SerializeField] protected int randomLenMin = 50;
    [SerializeField] protected int randomLenMax = 130;

    [HideInInspector]
    public bool isAllTargetsActive;
    [HideInInspector]
    public bool isFirstOrLastTarget = true;

    protected static List<Target> objects = new List<Target>();
    private int numTargets = 0;

    // Spawns a set of targets within a bounding box
    // Spawn Rect = [topleft pos, size] in local space when middle is 0,0
    // 0,0 is top left, right and up is positive
    // TODO remove camera if not used
    public void Spawn(int numberToSpawn, RectTransform spawnRect, float targetWidth, float targetHeight, float minDist, float maxDist, Camera camera)
    {
        numTargets = numberToSpawn;
        Debug.Log("Spawning " + numberToSpawn + " targets inside " + spawnRect.rect);
        for (int i = 0; i < numberToSpawn; i++)
        {
            float ratio = Mathf.Abs((camera.ScreenToWorldPoint(new Vector2(0, 0)) - camera.ScreenToWorldPoint(new Vector2(1, 0))).x);
            float randomXOffset = Random.Range(spawnRect.rect.xMin, spawnRect.rect.xMax) * ratio; // This is in screen space, need to convert to global space
            float randomYOffset = Random.Range(spawnRect.rect.yMin, spawnRect.rect.yMax) * ratio;
            Vector3 randomGlobalPos = new Vector3(randomXOffset, randomYOffset) + spawnRect.transform.position;
            CreateTarget(targetWidth, targetHeight, randomGlobalPos);
        }
    }

    virtual protected void CreateTarget(float targetWidth, float targetHeight, Vector3 localPos)
    {
        Target target = Instantiate(targetPrefab, localPos, Quaternion.identity, targetParent.transform);
        Debug.Log(localPos);
        target.SetSize(targetWidth, targetHeight);

        if (randomSize)
        {
            target.SetRandomSize(randomLenMin, randomLenMax);
        }
        if (randomRotation)
        {
            target.SetRandomRotation();
        }
        objects.Add(target);
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