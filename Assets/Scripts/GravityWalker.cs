using UnityEngine;

/// <summary>
/// Kommt auf "GravityOrientation" (leerer Parent über dem Player-Rig).
///
/// KEIN Fallen, KEINE Physik-Kraft: Statt Gravitation zu simulieren, wird die
/// Position des Spielers jeden FixedUpdate direkt auf die nächstgelegene
/// Fläche "gemagnetet" (mehrere SphereCasts nach unten, gemittelt, + weiche
/// Positionskorrektur per SmoothDamp). Die Gravitationsrichtung selbst
/// (welche Richtung gerade "unten" ist) wird von GravityZone.cs gesetzt,
/// wenn der Spieler eine manuell platzierte Trigger-Zone an einer Wand/Boden
/// betritt.
///
/// TERRAIN-ADAPTION (fuer unebene Scan-Meshes): Statt nur EINEM zentralen
/// Raycast/SphereCast werden "sampleCount" Punkte in einem kleinen Kreis um
/// die Spielerposition abgetastet und gemittelt. Das gleicht einzelne
/// Dellen/Buckel im Mesh aus (aehnlich einer Fuss-Aufstandsflaeche statt
/// eines einzelnen Punkts). Die Positionskorrektur selbst laeuft ueber
/// SmoothDamp statt einem einfachen Lerp - das federt kleine Sprünge in der
/// erkannten Hoehe zusaetzlich ab.
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
    [Tooltip("Cast-Ursprung wird um diesen Betrag entlang targetUp angehoben, bevor nach unten gecastet wird. Fängt ab, wenn die Fläche nach einer Kuppe abfällt und sonst außer Reichweite wäre.")]
    public float castStartOffset = 1f;

    [Header("Terrain-Adaption (unebene Flächen)")]
    [Tooltip("Anzahl zusätzlicher Abtastpunkte im Kreis um die Spielerposition (0 = nur zentraler Cast, keine Mittelung)")]
    [Range(0, 8)]
    public int sampleCount = 4;
    [Tooltip("Radius des Abtastkreises um die Spielerposition")]
    public float sampleRadius = 0.25f;
    [Tooltip("Wie weich die Höhenkorrektur nachzieht (größer = weicher/träger, kleiner = direkter)")]
    public float positionSmoothTime = 0.08f;

    [Header("Rotation")]
    [Tooltip("Wie schnell sich die Ausrichtung an die neue Zone anpasst")]
    public float rotationSpeed = 6f;

    [Header("Debug")]
    public bool logDetection = false;

    private Vector3 targetUp = Vector3.up;
    private Vector3 correctionVelocity = Vector3.zero;

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

        Vector3 averagedHitPoint;
        bool found = SampleGround(down, out averagedHitPoint);

        if (!found)
        {
            if (logDetection)
                Debug.Log("[GravityWalker] Kein Boden in Reichweite - Position bleibt unverändert (kein Fallen).");
            return;
        }

        // Zielposition: Abstand "groundOffset" entlang der Zielrichtung über dem gemittelten Treffpunkt
        Vector3 desiredPosition = averagedHitPoint + targetUp * groundOffset;

        // Nur die Komponente ENTLANG der Zielrichtung korrigieren,
        // damit seitliche Bewegung aus der FPC unangetastet bleibt.
        Vector3 currentAlongUp = Vector3.Project(player.position, targetUp);
        Vector3 desiredAlongUp = Vector3.Project(desiredPosition, targetUp);
        Vector3 correction = desiredAlongUp - currentAlongUp;

        // SmoothDamp statt Lerp: federt kleine, ruckartige Höhensprünge
        // (Buckel/Dellen im Scan-Mesh) zusätzlich ab, statt sie 1:1 zu übernehmen.
        Vector3 smoothedCorrection = Vector3.SmoothDamp(
            Vector3.zero, correction, ref correctionVelocity, positionSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        playerRb.MovePosition(playerRb.position + smoothedCorrection);

        if (logDetection)
            Debug.Log($"[GravityWalker] Snap-Korrektur: {smoothedCorrection.magnitude:F3}");
    }

    /// <summary>
    /// Castet einen zentralen Punkt plus (optional) mehrere Punkte im Kreis
    /// um die Spielerposition, und mittelt die gefundenen Treffpunkte.
    /// Reduziert das Wackeln durch einzelne Unebenheiten im Mesh.
    /// </summary>
    bool SampleGround(Vector3 down, out Vector3 averagedHitPoint)
    {
        Vector3 sum = Vector3.zero;
        int hits = 0;

        // Ursprung entlang targetUp anheben (= entgegen "down"), Cast-Distanz
        // entsprechend verlängern. Deckt so sowohl Erhebungen als auch
        // Vertiefungen ab, ohne dass Wände/Decken in der Nähe faelschlich
        // getroffen werden (der zusaetzliche Bereich liegt ja weiterhin auf
        // derselben "down"-Achse, nicht seitlich).
        Vector3 raisedPlayerPos = player.position - down * castStartOffset;
        float castDistance = groundCheckDistance + castStartOffset;

        // Zentraler Cast
        if (TryCast(raisedPlayerPos, down, castDistance, out RaycastHit centerHit))
        {
            sum += centerHit.point;
            hits++;
        }

        // Zusätzliche Punkte im Kreis, senkrecht zu "down" ausgerichtet
        if (sampleCount > 0)
        {
            Vector3 circleRight = Vector3.Cross(down, Vector3.up);
            if (circleRight.sqrMagnitude < 0.001f)
                circleRight = Vector3.Cross(down, Vector3.forward);
            circleRight.Normalize();
            Vector3 circleForward = Vector3.Cross(circleRight, down).normalized;

            for (int i = 0; i < sampleCount; i++)
            {
                float angle = (360f / sampleCount) * i * Mathf.Deg2Rad;
                Vector3 offset = (circleRight * Mathf.Cos(angle) + circleForward * Mathf.Sin(angle)) * sampleRadius;
                Vector3 origin = raisedPlayerPos + offset;

                if (TryCast(origin, down, castDistance, out RaycastHit hit))
                {
                    sum += hit.point;
                    hits++;
                }
            }
        }

        if (hits == 0)
        {
            averagedHitPoint = Vector3.zero;
            return false;
        }

        averagedHitPoint = sum / hits;
        return true;
    }

    bool TryCast(Vector3 origin, Vector3 down, float distance, out RaycastHit hit)
    {
        return groundCheckRadius > 0f
            ? Physics.SphereCast(origin, groundCheckRadius, down, out hit, distance, groundMask)
            : Physics.Raycast(origin, down, out hit, distance, groundMask);
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