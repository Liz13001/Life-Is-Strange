using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MoveAlongSpline : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 1f;
    float distancePercentage = 0f;
    float splineLength;
    private bool canMove = false;

    [Header("Animation")]
    [Tooltip("Animator on the rabbit (or wherever the walk/run animation lives)")]
    public Animator animator;
    [Tooltip("Name of the trigger parameter to fire when movement starts")]
    public string startTriggerName = "StartMoving";

    private void Start()
    {
        splineLength = spline.CalculateLength();

        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public void StartMoving()
    {
        canMove = true;

        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger(startTriggerName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        distancePercentage += speed * Time.deltaTime / splineLength;

        if (distancePercentage >= 1f)
        {
            distancePercentage = 1f;
            Vector3 finalPosition = spline.EvaluatePosition(distancePercentage);
            transform.position = finalPosition;

            canMove = false;

            if (animator != null)
            {
                animator.enabled = false;
            }

            return;
        }

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        Vector3 nextPosition = spline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }
}