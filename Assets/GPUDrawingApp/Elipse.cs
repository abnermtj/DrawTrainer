using UnityEngine;
using UnityEngine.UI;

public class Elipse : DrawManager
{

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
