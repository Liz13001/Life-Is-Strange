using UnityEngine;

public class FloatScript : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("How high/low the object floats from its starting position (in units)")]
    public float amplitude = 0.5f;

    [Tooltip("How fast the object floats up and down")]
    public float speed = 1f;

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        float newY = _startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
    }
}