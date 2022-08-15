using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LineDrawing : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private Camera particlesCamera;

    new void Start()
    {
        base.Start();
        if (!particlesCamera) particlesCamera = GetComponent<Camera>();
    }

    new void Update()
    {
        base.Update();

        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            comboScore = 0;
            missScore++;
            missScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }

        if (targetSpawner.isAllTargetsActive && penJustReleased)
        {
            ResetBoard(isWin: true);

            hitScore++;
            hitScoreLabel.GetComponent<Text>().text = hitScore.ToString();

            if (particles)
            {
                particles.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                particles.GetComponent<ParticleSystem>().Play();
            }

            if (comboPrefab)
            {
                comboScore += 1;
                GameObject DamageText = Instantiate(comboPrefab,canvas.transform);
                DamageText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                DamageText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                DamageText.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(comboScore.ToString());
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}
