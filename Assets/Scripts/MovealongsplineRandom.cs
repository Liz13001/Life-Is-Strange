using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MoveAlongSplineRandom : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 1f;

    [Header("Random Pause Between Loops")]
    public bool useRandomPauses = false;
    public float minPauseDuration = 1f;
    public float maxPauseDuration = 4f;

    float distancePercentage = 0f;
    float splineLength;
    bool isPaused = false;

    private void Start()
    {
        splineLength = spline.CalculateLength();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;

        distancePercentage += speed * Time.deltaTime / splineLength;

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        if (distancePercentage > 1f)
        {
            distancePercentage = 0f;

            if (useRandomPauses)
            {
                StartCoroutine(PauseThenContinue());
                return;
            }
        }

        Vector3 nextPosition = spline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }

    IEnumerator PauseThenContinue()
    {
        isPaused = true;
        float pauseTime = Random.Range(minPauseDuration, maxPauseDuration);
        yield return new WaitForSeconds(pauseTime);
        isPaused = false;
    }
}