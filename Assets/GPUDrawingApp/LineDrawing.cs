using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LineDrawing : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private Camera particlesCamera;

    private int missScore = 0;
    private int comboScore = 0;
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

            comboScore = 0;
            missScore++;
            MissScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }


        if (penJustReleased)
        {
            targetSpawner.ResetTargets();
        }
        if (hitScore == goalTargets)
        {
            WinLabel.active = true;
        }
        else
        {
            WinLabel.active = false;
        }


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

            if (ComboPrefab)
            {
                comboScore += 1;
                GameObject DamageText = Instantiate(ComboPrefab,Canvas.transform);
            DamageText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            DamageText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                DamageText.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(comboScore.ToString());
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}
