using UnityEngine;

/// <summary>
/// Kommt auf "GravityOrientation" (leerer Parent über dem Player-Rig).
///
/// KEIN Fallen, KEINE Physik-Kraft: Statt Gravitation zu simulieren, wird die
/// Position des Spielers jeden FixedUpdate direkt auf die nächstgelegene
/// Fläche "gemagnetet" (SphereCast nach unten + Positionskorrektur). Die
/// Gravitationsrichtung selbst (welche Richtung gerade "unten" ist) wird von
/// GravityZone.cs gesetzt, wenn der Spieler eine manuell platzierte
/// Trigger-Zone an einer Wand betritt.
///
/// WICHTIG - Pivot-Fix: GravityOrientation wird jeden FixedUpdate exakt auf
/// die aktuelle Weltposition des Spielers zentriert (RecenterPivot), BEVOR
/// rotiert wird. Ohne das würde die Rotation um den (meist weit entfernten,
/// z.B. am Spawnpunkt liegenden) Ursprung von GravityOrientation erfolgen -
/// der Spieler haengt dann wie an einem Hebelarm und wird bei jeder Drehung
/// ueber eine grosse Distanz durch die Luft geschleudert.
///
/// WICHTIG: "Enable Jump" in FirstPersonController.cs sollte deaktiviert
/// werden (Inspector-Checkbox), da Springen mit diesem Snap-System nicht
/// sinnvoll zusammenspielt - der Spieler würde sofort wieder auf die
/// Fläche zurückgezogen.
///
/// FirstPersonController.cs muss weiterhin transform.up statt Vector3.up
/// für CheckGround/Movement nutzen (siehe dortige Kommentare).
/// </summary>
public class GravityWalker : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Das Player-Objekt mit Rigidbody + FirstPersonController (Kind dieses Objekts)")]
    public Transform player;
    public Rigidbody playerRb;

    [Header("Ground Snapping")]
    [Tooltip("Layer der begehbaren Geometrie (Boden, Wände, Decke)")]
    public LayerMask groundMask;
    [Tooltip("Wie weit nach einer Fläche gesucht wird")]
    public float groundCheckDistance = 2f;
    [Tooltip("Radius des SphereCast (0 = normaler Raycast)")]
    public float groundCheckRadius = 0.3f;
    [Tooltip("Gewünschter Abstand zwischen Spieler-Pivot und Fläche (z.B. halbe Capsule-Höhe)")]
    public float groundOffset = 1f;
    [Tooltip("Wie schnell die Positionskorrektur zur Fläche hin erfolgt (0 = sofort einrasten, höher = weicher)")]
    public float snapSpeed = 15f;

    [Header("Rotation")]
    [Tooltip("Wie schnell sich die Ausrichtung an die neue Zone anpasst")]
    public float rotationSpeed = 6f;

    [Header("Debug")]
    public bool logDetection = false;

    private Vector3 targetUp = Vector3.up;

    void Start()
    {
        targetUp = transform.up;
    }

    void FixedUpdate()
    {
        RecenterPivot();
        SnapToGround();
        AlignOrientation();
    }

    /// <summary>
    /// Wird von GravityZone.cs aufgerufen, wenn der Spieler eine Zone betritt/verlässt.
    /// </summary>
    public void SetGravityDirection(Vector3 newUp)
    {
        targetUp = newUp.normalized;
        if (logDetection)
            Debug.Log($"[GravityWalker] Neue Gravitationsrichtung gesetzt: {targetUp}");
    }

    /// <summary>
    /// Zentriert GravityOrientation exakt auf der aktuellen Weltposition des
    /// Spielers, OHNE dessen sichtbare Position zu verändern. Dadurch dreht
    /// AlignOrientation() immer um den Spieler selbst statt um einen
    /// entfernten Punkt (z.B. den Spawnpunkt) - kein Hebelarm-Schleudern mehr.
    /// </summary>
    void RecenterPivot()
    {
        transform.position = player.position;
        player.localPosition = Vector3.zero;
    }

    void SnapToGround()
    {
        // Wichtig: mit targetUp (feste Zielrichtung von der Zone) casten,
        // nicht mit transform.up - letzteres dreht sich erst über mehrere
        // Frames sanft dorthin (rotationSpeed) und würde während der
        // Übergangsphase in die falsche Richtung suchen.
        Vector3 down = -targetUp;
        RaycastHit hit;

        bool found = groundCheckRadius > 0f
            ? Physics.SphereCast(player.position, groundCheckRadius, down, out hit, groundCheckDistance, groundMask)
            : Physics.Raycast(player.position, down, out hit, groundCheckDistance, groundMask);

        if (!found)
        {
            if (logDetection)
                Debug.Log("[GravityWalker] Kein Boden in Reichweite - Position bleibt unverändert (kein Fallen).");
            return;
        }

        // Zielposition: Abstand "groundOffset" entlang der Zielrichtung über dem Treffpunkt
        Vector3 desiredPosition = hit.point + targetUp * groundOffset;

        // Nur die Komponente ENTLANG der Zielrichtung korrigieren,
        // damit seitliche Bewegung aus der FPC unangetastet bleibt.
        Vector3 currentAlongUp = Vector3.Project(player.position, targetUp);
        Vector3 desiredAlongUp = Vector3.Project(desiredPosition, targetUp);
        Vector3 correction = desiredAlongUp - currentAlongUp;

        Vector3 smoothedCorrection = snapSpeed > 0f
            ? Vector3.Lerp(Vector3.zero, correction, Time.fixedDeltaTime * snapSpeed)
            : correction;

        playerRb.MovePosition(playerRb.position + smoothedCorrection);

        if (logDetection)
            Debug.Log($"[GravityWalker] Snap auf {hit.collider.name}, Distanz: {hit.distance:F2}");
    }

    void AlignOrientation()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(player.position, player.position - transform.up * groundCheckDistance);
    }
}