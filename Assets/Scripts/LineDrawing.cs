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

    public static void LinearRegression(
        List<Vector2> vals,
        out double rSquared,
        out double yIntercept,
        out double slope)
    {
        var count = vals.Count;
        Debug.Log(count);
        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double sumCodeviates = 0;

        for (var i = 0; i < count; i++)
        {
            double x = vals[i].x;
            double y = vals[i].y;
            sumCodeviates += x * y;
            sumOfX += x;
            sumOfY += y;
            sumOfXSq += x * x;
            sumOfYSq += y * y;
        }

        double ssX = sumOfXSq - ((sumOfX * sumOfX) / count);

        double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        double sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        double meanX = sumOfX / count;
        double meanY = sumOfY / count;
        Debug.Log(rNumerator);
        Debug.Log(rDenom);
        double dblR = rNumerator / Mathf.Sqrt((float)rDenom);

        rSquared = dblR * dblR;
        yIntercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
    }

    //// Measures the straightness of a line from 0 to 1. With 1 being the most straight.
    //// rSquared is Nan if there are not enough points
    //private float MeasureStraightness(List<Vector2> points)
    //{
    //    double rSquared, intercept, slope;
    //    LinearRegression(points, out rSquared, out intercept, out slope);

    //    Debug.Log($"R-squared = {rSquared}");
    //    Debug.Log($"Intercept = {intercept}");
    //    Debug.Log($"Slope = {slope}");
    //    return (float)rSquared;
    //}

    // Measures the straightness of a line from 0 to 1. With 1 being the most straight.
    // rSquared is Nan if there are not enough points
    //private float MeasureStraightness(List<Vector2> points)
    //{
    //    if (points.Count < 2)
    //    {
    //        Debug.Log($"Not enough points to measure straightness");
    //        return 0.0f;
    //    }

    //    double len = LengthBetweenPoints(points);
    //    double straightness = len / Vector2.Distance(points[0], points[points.Count - 1]) - 1.0f;
    //    return (float)straightness;
    //}

    //// Measures the straightness of a line from 0 to 1. With 1 being the most straight.
    //// rSquared is Nan if there are not enough points
    private float MeasureStraightness(List<Vector2> points)
    {
        double rSquared, intercept, slope;
        LinearRegression(points, out rSquared, out intercept, out slope);

        foreach (Vector2 point in points)
            Debug.Log(point);
        Debug.Log($"slope = {slope}");
        Debug.Log($"y = {intercept}");
        Debug.Log($"R-squared = {rSquared}");
        return (float)rSquared;
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
                    Debug.Log("Not straight enough");
                    return;
                }
            }

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
                //DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(comboScore.ToString());
                DamageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(String.Format("{0:P2}", straightness));

                if (numTargets > 1)
                {
                    source.clip = comboSounds[UnityEngine.Random.Range(0, comboSounds.Length)];
                    //source.volume = 0.1f + 0.05f * Mathf.Min(comboScore, 6);
                    source.pitch = 1 + 2.0f * (1.0f - Mathf.Max((straightness - 0.99f) / 0.01f, 0.0f));
                    //source.pitch = 2 + 0.2f * Mathf.Min(comboScore, 6);
                    source.PlayOneShot(source.clip);
                }
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}