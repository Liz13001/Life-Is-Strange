using UnityEngine;

/// <summary>
/// Attach this to the big cube trigger collider sitting above the car.
/// It gives the player a small vertical boost + horizontal pull toward the
/// zone center when they jump into it, so it's much easier to land on top
/// of the imprecise mesh collider.
/// Requires: the cube collider has "Is Trigger" enabled.
/// Requires: the player has a non-kinematic Rigidbody (Jess Case FPC already has this).
/// </summary>
public class CarJumpAssist : MonoBehaviour
{
    [Header("Player Filter")]
    [Tooltip("Only objects with this tag get assisted")]
    public string playerTag = "Player";

    [Header("Vertical Boost")]
    [Tooltip("Upward velocity applied when the player jumps into this zone")]
    public float upwardBoost = 4f;
    [Tooltip("Only assist if the player's current vertical velocity is inside this range (filters out just walking through)")]
    public float minVelocityY = -8f;
    public float maxVelocityY = 6f;
    [Tooltip("Seconds before the same player can be boosted again")]
    public float boostCooldown = 0.5f;

    [Header("Horizontal Pull")]
    [Tooltip("Gentle pull toward the zone's center so the player doesn't slide off the edge mid-air")]
    public float horizontalPull = 2f;
    [Tooltip("Set to 0 to disable horizontal pull entirely")]
    public bool enableHorizontalPull = true;

    private float lastBoostTime = -10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        TryVerticalBoost(other.attachedRigidbody);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!enableHorizontalPull) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector3 toCenter = transform.position - rb.position;
        toCenter.y = 0f;

        if (toCenter.magnitude > 0.1f)
        {
            rb.AddForce(toCenter.normalized * horizontalPull, ForceMode.Acceleration);
        }
    }

    private void TryVerticalBoost(Rigidbody rb)
    {
        if (rb == null) return;
        if (Time.time - lastBoostTime < boostCooldown) return;

        if (rb.linearVelocity.y >= minVelocityY && rb.linearVelocity.y <= maxVelocityY)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = upwardBoost;
            rb.linearVelocity = vel;
            lastBoostTime = Time.time;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}