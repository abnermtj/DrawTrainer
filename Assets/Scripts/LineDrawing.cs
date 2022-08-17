using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LineDrawing : DrawManager
{
    [SerializeField] private GameObject particles;
    [SerializeField] private Camera particlesCamera;
    [SerializeField] private GameObject straightnessLabel;
    [SerializeField] protected bool isStrikeThrough;

    private List<Vector2> strokePoints = new List<Vector2>();

    private new void Start()
    {
        base.Start();
        if (!particlesCamera) particlesCamera = GetComponent<Camera>();
    }

    /// <summary>
    /// Fits a line to a collection of (x,y) points.
    /// </summary>
    public static void LinearRegression(
        List<Vector2> vals,
        out double rSquared,
        out double yIntercept,
        out double slope)
    {
        var count = vals.Count;
        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double sumCodeviates = 0;

        for (var i = 0; i < count; i++)
        {
            var x = vals[i].x;
            var y = vals[i].y;
            sumCodeviates += x * y;
            sumOfX += x;
            sumOfY += y;
            sumOfXSq += x * x;
            sumOfYSq += y * y;
        }

        var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);

        var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        var meanX = sumOfX / count;
        var meanY = sumOfY / count;
        var dblR = rNumerator / Mathf.Sqrt((float)rDenom);

        rSquared = dblR * dblR;
        yIntercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
    }

    // Measures the straightness of a line from 0 to 1. With 1 being the most straight.
    private float MeasureStraightness(List<Vector2> points)
    {
        double rSquared, intercept, slope;
        LinearRegression(points, out rSquared, out intercept, out slope);

        Debug.Log($"R-squared = {rSquared}");
        Debug.Log($"Intercept = {intercept}");
        Debug.Log($"Slope = {slope}");
        return (float)rSquared;
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

        if (pointerPressed)
        {
            strokePoints.Add(penPosition);
        }

        if (pointerJustReleased)
        {
            float straightness = MeasureStraightness(strokePoints);
            straightnessLabel.GetComponent<Text>().text = String.Format("{0:P2}", straightness);

            strokePoints = new List<Vector2>();
        }

        if ((targetSpawner.isAllTargetsActive && pointerJustReleased) || (targetSpawner.isAllTargetsActive && isStrikeThrough))
        {
            if (isStrikeThrough)
            {
                strokeEndPos = penPosition;
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
                DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(comboScore.ToString());

                source.clip = comboSounds[UnityEngine.Random.Range(0, comboSounds.Length)];
                source.volume = 0.1f + 0.05f * Mathf.Min(comboScore, 6);
                source.pitch = 2 + 0.2f * Mathf.Min(comboScore, 6);
                source.PlayOneShot(source.clip);
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}