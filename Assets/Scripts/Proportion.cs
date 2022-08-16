using System;
using UnityEngine;
using UnityEngine.UI;

public class Proportion : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject percentLabel;

    [Range(0.0f, 0.5f)]
    [SerializeField] private float percentageTarget = 0;

    [SerializeField] private float missResetTime = 0.4f;
    [SerializeField] protected LineTargetSpawner lineTargetSpawner;

    private new void Start()
    {
        percentageTarget = UnityEngine.Random.Range(0.0f, 0.5f);
        lineTargetSpawner.percentageTarget = percentageTarget;
        base.Start();
    }

    protected override void ResetBoard(bool isWin)
    {
        ClearBrushMarks();

        targetResetTimer = targetResetIntervalSeconds;
        lineTargetSpawner.ClearAll(playSound: isWin);
        targetSpawner.Spawn(1, spawnBox.GetComponent<RectTransform>(), targetWidth, targetHeight, camera);
    }

    private new void Update()
    {
        base.Update();

        if (penJustReleased)
        {
            targetResetTimer = missResetTime;
            lineTargetSpawner.ResetTargets();
        }

        lineTargetSpawner.percentageTarget = percentageTarget;
        percentLabel.GetComponent<Text>().text = String.Format("{0:P2}", percentageTarget);

        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            missScore++;
            missScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }

        if (targetSpawner.isAllTargetsActive && penJustReleased)
        {
            percentageTarget = UnityEngine.Random.Range(0.0f, 0.5f);
            lineTargetSpawner.percentageTarget = percentageTarget;

            hitScore++;
            hitScoreLabel.GetComponent<Text>().text = hitScore.ToString();

            if (particles)
            {
                particles.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                particles.GetComponent<ParticleSystem>().Play();
            }

            targetResetTimer = targetResetIntervalSeconds;
            ResetBoard(isWin: true);
        }
    }
}