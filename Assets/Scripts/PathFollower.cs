using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class PathFollower : MonoBehaviour
{
    public SplineContainer spline;
    [SerializeField] private float speed = 2f;

    private float _t = 0f;

    void Update()
    {
        float splineLength = spline.Splines[0].GetLength();
        _t += (speed / splineLength) * Time.deltaTime;
        _t %= 1f;

        SplineUtility.Evaluate(
            spline.Splines[0],
            _t,
            out float3 pos,
            out float3 forward,
            out float3 upVector
        );

        transform.position = spline.transform.TransformPoint((Vector3)pos);

        if ((Vector3)forward != Vector3.zero)
            transform.rotation = Quaternion.LookRotation((Vector3)forward, (Vector3)upVector);
    }
}