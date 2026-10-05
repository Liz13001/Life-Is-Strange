using UnityEngine;

/// <summary>
/// GravityWalker
///
/// Verantwortlich nur für:
/// 1. Gravity-Richtung von GravityZone.cs empfangen
/// 2. GravityOrientation in diese Richtung drehen
/// 3. Eine einfache Custom Gravity auf den Player-Rigidbody anwenden
///
/// KEINE Ground Checks.
/// KEINE SphereCasts.
/// KEIN Ground Offset.
/// KEIN automatisches Positionieren des Players.
///
/// Die tatsächliche Position auf Boden/Wand/Decke ergibt sich
/// durch Rigidbody + Collider.
/// </summary>

public class GravityWalker : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Player Rigidbody")]
    public Rigidbody playerRb;


    [Header("Gravity")]

    [Tooltip("Stärke der Custom Gravity. Unity Standard ist ungefähr 9.81.")]
    public float gravityStrength = 9.81f;


    [Header("Rotation")]

    [Tooltip("Wie schnell sich der Player an eine neue Gravity-Richtung dreht.")]
    public float rotationSpeed = 6f;


    [Header("Debug")]

    public bool logDetection = false;


    // Welche Richtung aktuell "oben" ist.
    private Vector3 targetUp = Vector3.up;


    void Start()
    {
        targetUp = transform.up;
    }


    void FixedUpdate()
    {
        ApplyGravity();
        AlignOrientation();
    }


    /// <summary>
    /// Wird weiterhin von GravityZone.cs aufgerufen.
    ///
    /// newUp bestimmt, welche Richtung für den Player "oben" ist.
    /// </summary>
    public void SetGravityDirection(Vector3 newUp)
    {
        targetUp = newUp.normalized;

        if (logDetection)
        {
            Debug.Log(
                $"[GravityWalker] Neue Gravity Orientation: {targetUp}"
            );
        }
    }


    /// <summary>
    /// Custom Gravity.
    ///
    /// Statt Unity World-Y Gravity benutzen wir die aktuelle
    /// Gravity-Richtung.
    /// </summary>
    void ApplyGravity()
    {
        if (playerRb == null)
            return;

        playerRb.AddForce(
            -targetUp * gravityStrength,
            ForceMode.Acceleration
        );
    }


    /// <summary>
    /// Dreht GravityOrientation weich in die neue Orientierung.
    /// </summary>
    void AlignOrientation()
    {
        Quaternion targetRotation =
            Quaternion.FromToRotation(
                transform.up,
                targetUp
            )
            * transform.rotation;

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotationSpeed
            );
    }
}