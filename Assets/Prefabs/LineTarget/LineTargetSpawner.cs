using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTargetSpawner : TargetSpawner
{
    [SerializeField] LineTarget lineTargetPrefab;
    public float percentageTarget;
    override protected void CreateTarget(float targetWidth, float targetHeight, Vector3 pos)
    {
        LineTarget target = Instantiate(lineTargetPrefab, pos, Quaternion.identity, targetParent.transform);
        target.SetSize(targetWidth, targetHeight);
        target.percentageTarget = percentageTarget;

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
}
