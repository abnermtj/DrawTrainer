using System;
using UnityEngine;
using UnityEngine.UI;

public class Proportion : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject percentLabel;
    [SerializeField] private Camera particlesCamera;

    [Range (0.0f, 0.5f)]
    [SerializeField] private float percentageTarget = 0;
    [SerializeField] private float missResetTime = 0.4f;
    [SerializeField] protected LineTargetSpawner lineTargetSpawner;

    private int missScore = 0;
    private int hitScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        percentageTarget = UnityEngine.Random.Range(0.0f, 0.5f);
        lineTargetSpawner.percentageTarget = percentageTarget;
        if (!particlesCamera) particlesCamera = GetComponent<Camera>();

        base.Start();
    }
    override protected void ResetBoard(bool isWin)
    {
        ClearBrushMarks();

        targetResetTimer = targetResetIntervalSeconds;
        lineTargetSpawner.ClearAll(playSound: isWin);
        lineTargetSpawner.Spawn(numTargets, Screen.width / 2 - 150, Screen.height / 2 - 100, 350, 100, targetWidth, targetHeight, 20, 140, camera);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        gameTimer -= Time.deltaTime;
        GameTimerLabel.GetComponent<Text>().text = gameTimer.ToString();

        
        if (penJustReleased)
        {
            targetResetTimer = missResetTime;
            lineTargetSpawner.ResetTargets();
        }

        lineTargetSpawner.percentageTarget = percentageTarget;
        percentLabel.GetComponent<Text>().text = String.Format("{0:P2}", percentageTarget);

        targetResetTimer -= Time.deltaTime;
        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            missScore++;
            MissScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }




        if (targetSpawner.isAllTargetsActive && penJustReleased)
        {
            percentageTarget = UnityEngine.Random.Range(0.0f, 0.5f);
            lineTargetSpawner.percentageTarget = percentageTarget;

            hitScore++;
            HitScoreLabel.GetComponent<Text>().text = hitScore.ToString();

            if (particles)
            {
                particles.GetComponent<RectTransform>().position = strokeEndPos;
                particles.GetComponent<ParticleSystem>().Play();
            }

            targetResetTimer = targetResetIntervalSeconds;
            ResetBoard(isWin: true);
        }


    }
}
