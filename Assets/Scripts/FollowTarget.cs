using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target; // drag Bobby Car Pivot here
    public Vector3 offset = new Vector3(0f, 2f, -5f); // camera distance

    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}