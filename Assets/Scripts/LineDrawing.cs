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
        bool flip,
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
            double x = flip ? vals[i].y : vals[i].x;
            double y = flip ? vals[i].x : vals[i].y;

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
        double dblR = rNumerator / Mathf.Sqrt((float)rDenom);

        rSquared = dblR * dblR;
        yIntercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
    }

    // ax+by+c=0
    // This is a positive value
    private float distPosToLine(float a, float b, float c, Vector2 pos)
    {
        float numer = Mathf.Abs(a * pos.x + b * pos.y + c);
        float denom = Mathf.Sqrt(a * a + b * b);
        return numer / denom;
    }

    // 1.0f points for really straight lines
    // 0 points for poor lines
    private float MeasureStraightness2(List<Vector2> points)
    {
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

        int count = points.Count;
        float sumError = 0;
        for (int i = 1; i < count - 1; i++)
        {
            sumError += distPosToLine(a, b, c, points[i]);
        }

        //Debug.Log(sumError);
        //Debug.Log(count);
        float avgError = sumError / count;

        float score = Mathf.Clamp((100.0f - avgError) / 100.0f, 0.0f, 1.0f);

        Debug.Log(avgError);
        return score;
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
        LinearRegression(points, flip: false, out rSquared, out intercept, out slope);

        // If the points are vertical then need to flip x,y because X is the DV now
        if (Mathf.Abs((float)slope) > 2)
        {
            Debug.Log("FLIP");
            LinearRegression(points, flip: true, out rSquared, out intercept, out slope);
        }

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
            straightness = MeasureStraightness2(strokePoints);
            straightnessLabel.GetComponent<Text>().text = String.Format("{0:P2}", straightness);
            strokePoints = new List<Vector2>();
        }

        if ((targetSpawner.isAllTargetsActive && pointerJustReleased) || (targetSpawner.isAllTargetsActive && isStrikeThrough))
        {
            //straightnessLabel.GetComponent<Text>().text = String.Format("{0:P2}", straightness);
            //Debug.Log($"Straightness {straightness}");
            if (numTargets == 2)
            {
                straightness = MeasureStraightness2(strokePoints);
                if (float.IsNaN(straightness))
                {
                    return;
                }
                if (straightness < straightnessGoal)
                {
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
                    //source.PlayOneShot(source.clip);
                }
            }

            targetResetTimer = targetResetIntervalSeconds;
        }
    }
}