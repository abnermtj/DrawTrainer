using UnityEngine;
using UnityEngine.UI;

public class LineDrawing : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private Camera particlesCamera;

    private int missScore = 0;
    private int hitScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        if (!particlesCamera) particlesCamera = GetComponent<Camera>();
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


        //particles.GetComponent<RectTransform>().anchoredPosition = penPosition;
        //particles.GetComponent<ParticleSystem>().Play();
        if (targetSpawner.isAllTargetsActive && penJustReleased)
        {
            ResetBoard(isWin: true);

            hitScore++;
            HitScoreLabel.GetComponent<Text>().text = hitScore.ToString();

            if (particles)
            {
                particles.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                particles.GetComponent<ParticleSystem>().Play();
            }

            targetResetTimer = targetResetIntervalSeconds;
        }


    }
}
