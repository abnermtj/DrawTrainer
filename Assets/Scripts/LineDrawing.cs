using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LineDrawing : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject straightnessLabel;
    [SerializeField] protected bool isStrikeThrough;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float straightnessGoal;

    private List<Vector2> strokePoints = new List<Vector2>();
    private float straightness;

    private new void Start()
    {
        base.Start();
    }

    // Assume points are in order
    public double LengthBetweenPoints(List<Vector2> vals)
    {
        if (vals.Count < 2)
        {
            return double.NaN;
        }

        double dist = 0f;
        for (int i = 1; i < vals.Count; ++i)
        {
            dist += Vector2.Distance(vals[i], vals[i - 1]);
        }

        return dist;
    }

    // Line is in the form: ax+by+c=0
    // This is a positive value
    private float distPosToLine(float a, float b, float c, Vector2 pos)
    {
        float numer = Mathf.Abs(a * pos.x + b * pos.y + c);
        float denom = Mathf.Sqrt(a * a + b * b);
        return numer / denom;
    }

    // 1.0f points for really straight lines
    // 0 points for poor lines
    private float MeasureStraightness(List<Vector2> points)
    {
        int count = points.Count;
        if (count <= 1)
        {
            return 0;
        }
        // https://brilliant.org/wiki/dot-product-distance-between-point-and-a-line/

        // Create a line between end points
        // ax+by+c=0
        Vector2 startPos = points[0];
        Vector2 endPos = points[points.Count - 1];
        //A=y1-y2
        float a = startPos.y - endPos.y;

        //B = x2 - x1
        float b = endPos.x - startPos.x;

        //C = x1 * y2 - y1 * x2
        float c = startPos.x * endPos.y - startPos.y * endPos.x;

        float sumError = 0;
        for (int i = 1; i < count - 1; i++)
        {
            sumError += distPosToLine(a, b, c, points[i]);
        }

        float avgError = sumError / count;

        float score = Mathf.Clamp((100.0f - avgError) / 100.0f, 0.0f, 1.0f);

        Debug.Log(avgError);
        return score;
    }

    private void FixedUpdate()
    {
        if (pointerPressed)
        {
            strokePoints.Add(penPosition);
        }
    }

    private new void Update()
    {
        base.Update();

        if (targetResetTimer < 0)
        {
            ResetBoard(isWin: false);

            comboScore = 0;
            missScore++;
            missScoreLabel.GetComponent<Text>().text = missScore.ToString();
        }

        if (pointerJustReleased)
        {
            straightness = MeasureStraightness(strokePoints);
            straightnessLabel.GetComponent<Text>().text = String.Format("{0:P2}", straightness);
            strokePoints = new List<Vector2>();
        }

        if ((targetSpawner.isAllTargetsActive && pointerJustReleased) || (targetSpawner.isAllTargetsActive && isStrikeThrough))
        {
            if (isStrikeThrough)
            {
                strokeEndPos = penPosition;
            }

            //straightnessLabel.GetComponent<Text>().text = String.Format("{0:P2}", straightness);
            //Debug.Log($"Straightness {straightness}");
            if (numTargets == 2)
            {
                straightness = MeasureStraightness(strokePoints);
                if (float.IsNaN(straightness))
                {
                    return;
                }
                if (straightness < straightnessGoal)
                {
                    targetSpawner.ResetTargets();

                    Debug.Log("Not straight enough Straightness: " + straightness);
                    GameObject DamageText = Instantiate(comboPrefab, canvas.transform);
                    DamageText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                    DamageText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                    DamageText.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                    DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(String.Format("Not Straight"));
                    strokePoints = new List<Vector2>();
                    return;
                }
            }

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
                GameObject DamageText = Instantiate(comboPrefab, canvas.transform);
                DamageText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                DamageText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                DamageText.GetComponent<RectTransform>().anchoredPosition = strokeEndPos;
                //DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(comboScore.ToString());
                DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(String.Format("{0:P2}", straightness));

                if (numTargets > 1)
                {
                    source.clip = comboSounds[UnityEngine.Random.Range(0, comboSounds.Length)];
                    //source.volume = 0.1f + 0.05f * Mathf.Min(comboScore, 6);
                    source.pitch = 1 + 2.0f * (1.0f - Mathf.Max((straightness - 0.99f) / 0.01f, 0.0f));
                    //source.pitch = 2 + 0.2f * Mathf.Min(comboScore, 6);
                    //source.PlayOneShot(source.clip);
                }
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}