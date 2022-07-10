using UnityEngine;
using System.Collections.Generic;

public class ElipseSpawner : MonoBehaviour
{
    [SerializeField] ElipseTarget targetPrefab;
    [SerializeField] GameObject parent;

    private static List<ElipseTarget> objects = new List<ElipseTarget>();

    // Spawns a set of targets within a bounding box
    public void Spawn(float originX, float originY, float width, float height, float targetWidth)
    {
        ElipseTarget target = (Instantiate(targetPrefab, new
        Vector3(originX + width * Random.Range(0, 1f),
        originY + height * Random.Range(0, 1f))
            , Quaternion.identity, parent.transform));

        target.setRandomSize();
        objects.Add(target);
    }

    // Removes all spawned targets
    public void ClearAll(bool playSound)
    {
        foreach (ElipseTarget obj in objects)
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