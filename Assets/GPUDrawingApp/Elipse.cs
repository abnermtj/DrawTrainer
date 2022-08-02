using UnityEngine;
using UnityEngine.UI;

public class Elipse : DrawManager
{
    [SerializeField] private float gameTimer = 500;

    private int missScore = 0;
    private int hitScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        gameTimer -= Time.deltaTime;
        GameTimerLabel.GetComponent<Text>().text = gameTimer.ToString();

        targetResetTimer -= Time.deltaTime;
        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            missScore++;
            MissScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }


        if (penJustReleased)
        {
            targetSpawner.ResetTargets();
        }


        if (targetSpawner.isAllTargetsActive && penJustReleased)
        {
            ResetBoard(isWin: true);

            hitScore++;
            HitScoreLabel.GetComponent<Text>().text = hitScore.ToString();
            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}
