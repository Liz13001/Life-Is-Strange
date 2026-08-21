using UnityEngine;
using StarterAssets;

public class WallGravity : MonoBehaviour
{
    [Header("Gravity")]
    public float rotationSpeed = 5f;

    private Vector3 _gravityDirection = Vector3.down;
    private StarterAssets.FirstPersonController _fpc;

    void Start()
    {
        _fpc = GetComponent<StarterAssets.FirstPersonController>();
    }

    void Update()
    {
        // Nur Rotation anpassen
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -_gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public Vector3 GetGravityDirection()
    {
        return _gravityDirection;
    }

    public void SetGravityDirection(Vector3 direction)
    {
        _gravityDirection = direction.normalized;
        Debug.Log("[WallGravity] SetGravityDirection aufgerufen: " + _gravityDirection);
    }

    public void ResetGravity()
    {
        _gravityDirection = Vector3.down;
    }
}