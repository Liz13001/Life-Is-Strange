using UnityEngine;

public class Rotator : MonoBehaviour
{
    public enum Direction { Clockwise, CounterClockwise }

    [Header("Rotation Settings")]
    [Tooltip("Speed in degrees per second")]
    public float speed = 90f;

    public Direction direction = Direction.Clockwise;

    void Update()
    {
        float sign = direction == Direction.Clockwise ? -1f : 1f;
        transform.Rotate(0f, sign * speed * Time.deltaTime, 0f, Space.World);
    }
}