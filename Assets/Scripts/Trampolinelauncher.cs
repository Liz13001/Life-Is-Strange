using System.Collections;
using UnityEngine;

/// <summary>
/// Put this on the trampoline object (needs a Collider, "Is Trigger" checked).
/// Built for Jess Case's "Modular First Person Controller" (Rigidbody-based).
/// When the player enters, it pauses their normal movement input and applies
/// a launch velocity computed via projectile physics so they land exactly on
/// targetLocation, then hands control back.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrampolineLauncher : MonoBehaviour
{
    [Header("Where the player gets launched to")]
    [Tooltip("Drag an empty GameObject placed at the landing spot")]
    public Transform targetLocation;

    [Header("Arc shape")]
    [Tooltip("Peak height of the arc above the higher of start/end point")]
    public float arcHeight = 6f;

    [Header("Player detection")]
    public string playerTag = "Player";

    [Tooltip("Player's downward speed must be at least this fast for it to count as a jump-on (not just walking into the side)")]
    public float minFallSpeed = 0.5f;

    [Tooltip("Log collision details to the Console to help debug detection")]
    public bool debugLog = true;

    [Header("Optional")]
    public UnityEngine.Events.UnityEvent onLaunch;
    public UnityEngine.Events.UnityEvent onLand;

    private bool _isLaunching;

    // NOTE: the trampoline's Collider should NOT be "Is Trigger" - leave it
    // solid so the player physically lands on top of it instead of passing
    // through.

    private void OnCollisionEnter(Collision collision)
    {
        if (debugLog) Debug.Log($"{name}: OnCollisionEnter with '{collision.collider.name}' (tag: {collision.collider.tag})");

        if (_isLaunching) return;
        if (!collision.collider.CompareTag(playerTag))
        {
            if (debugLog) Debug.Log($"{name}: ignored - tag '{collision.collider.tag}' != '{playerTag}'");
            return;
        }
        if (targetLocation == null)
        {
            Debug.LogWarning($"{name}: No targetLocation assigned.");
            return;
        }

        Rigidbody rb = collision.rigidbody;
        if (rb == null)
        {
            if (debugLog) Debug.Log($"{name}: ignored - no Rigidbody found on collider (collision.rigidbody is null)");
            return;
        }

        // Only launch if the player fell onto the TOP of the trampoline
        // (contact normal points roughly upward) and was actually falling
        // fast enough - so walking into the side doesn't trigger it.
        bool landedOnTop = false;
        float bestDot = 0f;
        foreach (ContactPoint contact in collision.contacts)
        {
            float dot = Vector3.Dot(contact.normal, Vector3.up);
            if (Mathf.Abs(dot) > Mathf.Abs(bestDot)) bestDot = dot;
            if (Mathf.Abs(dot) > 0.5f)
            {
                landedOnTop = true;
                break;
            }
        }

        if (debugLog) Debug.Log($"{name}: bestDot(normal,up)={bestDot:F2}, relativeVelocity={collision.relativeVelocity} (magnitude {collision.relativeVelocity.magnitude:F2}), minFallSpeed={minFallSpeed}");

        if (!landedOnTop)
        {
            if (debugLog) Debug.Log($"{name}: ignored - didn't land on top (|bestDot| {Mathf.Abs(bestDot):F2} <= 0.5). Hit the side/edge instead.");
            return;
        }
        if (collision.relativeVelocity.magnitude < minFallSpeed)
        {
            if (debugLog) Debug.Log($"{name}: ignored - impact speed {collision.relativeVelocity.magnitude:F2} < minFallSpeed {minFallSpeed}. Try jumping harder or lowering minFallSpeed.");
            return;
        }

        FirstPersonController fpc = collision.collider.GetComponent<FirstPersonController>();
        if (debugLog) Debug.Log($"{name}: LAUNCHING! fpc found: {fpc != null}");
        StartCoroutine(Launch(rb, fpc));
    }

    private IEnumerator Launch(Rigidbody rb, FirstPersonController fpc)
    {
        _isLaunching = true;
        onLaunch?.Invoke();

        // Stop the controller's per-FixedUpdate input steering from
        // cancelling out our launch velocity while airborne.
        bool hadMovement = fpc != null && fpc.playerCanMove;
        if (fpc != null) fpc.playerCanMove = false;

        Vector3 start = rb.position;
        Vector3 end = targetLocation.position;
        float gravity = Mathf.Abs(Physics.gravity.y);

        float displacementY = end.y - start.y;
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0f, end.z - start.z);

        // Make sure the apex is always above both start and end
        float effectiveArc = Mathf.Max(arcHeight, displacementY + 0.5f);

        float timeUp = Mathf.Sqrt(2f * effectiveArc / gravity);
        float timeDown = Mathf.Sqrt(Mathf.Max(0.01f, 2f * (effectiveArc - displacementY) / gravity));
        float totalTime = timeUp + timeDown;

        float velocityY = gravity * timeUp;
        Vector3 velocityXZ = displacementXZ / totalTime;

        rb.linearVelocity = velocityXZ + Vector3.up * velocityY;

        // Wait out the flight, then re-enable movement and snap-correct
        // any drift so landing feels precise.
        yield return new WaitForSeconds(totalTime);

        rb.position = end;
        rb.linearVelocity = Vector3.zero;

        if (fpc != null) fpc.playerCanMove = hadMovement;

        _isLaunching = false;
        onLand?.Invoke();
    }
}