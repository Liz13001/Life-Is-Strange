using UnityEngine;

public class RandomSpin : MonoBehaviour
{
    [Header("Speed")]
    public float minSpeed = 20f;
    public float maxSpeed = 100f;

    private float _speed;

    void Start()
    {
        _speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        transform.Rotate(0f, _speed * Time.deltaTime, 0f);
    }
}