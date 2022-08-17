using UnityEngine;
using UnityEngine.UI;

public class Elipse : DrawManager
{
    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();

        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            missScore++;
            missScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }

        foreach (Transform target in targets.transform)
        {
            if (pointerPressed)
            {
                target.gameObject.GetComponent<Target>().inActiveColor.a = 0.2f;
            }
            else
            {
                target.gameObject.GetComponent<Target>().inActiveColor.a = 1f;
            }
        }

        if (targetSpawner.isAllTargetsActive && pointerJustReleased)
        {
            ResetBoard(isWin: true);

            hitScore++;
            hitScoreLabel.GetComponent<Text>().text = hitScore.ToString();
            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}