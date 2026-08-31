using UnityEngine;
using System.Collections;

/// <summary>
/// Kommt auf eine Trigger-Zone (Box/Sphere Collider, "Is Trigger" aktiv),
/// die um eine Ring-Form (z.B. auf einer Wandfläche liegend) platziert wird.
///
/// ZWEI STUFEN:
/// 1. Player läuft in die Zone, drückt Jump -> Bogensprung zum Ring-Zentrum
///    (landingPoint). hasLandedInRing wird true.
/// 2. Player steht im Ring, drückt Jump erneut -> größerer Katapult-Flug
///    zu catapultTarget (z.B. eine Stelle an einer anderen Wand). Am
///    Zielort sollte dort eine eigene GravityZone liegen, die beim
///    Eintreffen automatisch die neue Ausrichtung setzt.
///
/// Beide Sprünge sind NICHT der normale FirstPersonController-Jump (der
/// bleibt deaktiviert), sondern eigene, unabhängige Bogenbewegungen per
/// Rigidbody.MovePosition.
///
/// SETUP:
/// 1. Leeres GameObject um den Ring herum platzieren, Box/Sphere Collider
///    als Trigger hinzufügen (Erkennungs-Radius für "nah genug dran")
/// 2. Kind-Objekt "landingPoint" exakt im Ring-Zentrum platzieren
/// 3. Ein weiteres Transform (kann irgendwo in der Szene liegen, z.B. als
///    Kind der Zielwand) als "catapultTarget" anlegen - muss innerhalb
///    der GravityZone der Zielwand liegen, damit die Neuausrichtung dort
///    automatisch greift
/// 4. GravityWalker- und PlayerRb-Referenzen zuweisen
/// </summary>
[RequireComponent(typeof(Collider))]
public class RingJumpTrigger : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Das GravityWalker-Script auf GravityOrientation")]
    public GravityWalker gravityWalker;
    [Tooltip("Der Player-Rigidbody (gleiche Referenz wie in GravityWalker)")]
    public Rigidbody playerRb;
    [Tooltip("Zielpunkt im Zentrum des Rings, wohin der erste Sprung geht")]
    public Transform landingPoint;

    [Header("Sprung 1: Rein in den Ring")]
    public string jumpButton = "Jump";
    [Tooltip("Dauer des ersten Bogensprungs in Sekunden")]
    public float jumpDuration = 0.6f;
    [Tooltip("Höhe des ersten Bogens (entlang der aktuellen Up-Achse)")]
    public float jumpArcHeight = 1.5f;

    [Header("Sprung 2: Katapult zu anderer Wand")]
    [Tooltip("Zielposition an der anderen Wand (sollte innerhalb von deren GravityZone liegen)")]
    public Transform catapultTarget;
    [Tooltip("Dauer des Katapult-Flugs in Sekunden")]
    public float catapultDuration = 0.8f;
    [Tooltip("Höhe des Katapult-Bogens (entlang der aktuellen Up-Achse zum Zeitpunkt des Absprungs)")]
    public float catapultArcHeight = 2.5f;

    [Header("Debug")]
    public bool logEvents = false;

    private bool playerInZone = false;
    private bool isJumping = false;
    private bool hasLandedInRing = false;

    void OnTriggerEnter(Collider other)
    {
        if (playerRb == null || other.attachedRigidbody != playerRb) return;
        playerInZone = true;
        if (logEvents) Debug.Log("[RingJumpTrigger] Player in Zone.");
    }

    void OnTriggerExit(Collider other)
    {
        if (playerRb == null || other.attachedRigidbody != playerRb) return;
        playerInZone = false;
        hasLandedInRing = false;
    }

    void Update()
    {
        if (!playerInZone || isJumping) return;
        if (!Input.GetButtonDown(jumpButton)) return;

        if (!hasLandedInRing)
        {
            StartCoroutine(JumpIntoRing());
        }
        else
        {
            StartCoroutine(CatapultToTarget());
        }
    }

    IEnumerator JumpIntoRing()
    {
        isJumping = true;

        if (gravityWalker != null)
            gravityWalker.enabled = false;

        Vector3 startPos = playerRb.position;
        Vector3 endPos = landingPoint.position;
        Vector3 arcAxis = gravityWalker != null ? gravityWalker.transform.up : Vector3.up;

        yield return FlyArc(startPos, endPos, arcAxis, jumpArcHeight, jumpDuration);

        if (gravityWalker != null)
            gravityWalker.enabled = true;

        hasLandedInRing = true;
        isJumping = false;

        if (logEvents) Debug.Log("[RingJumpTrigger] Im Ring gelandet - naechster Jump katapultiert.");
    }

    IEnumerator CatapultToTarget()
    {
        if (catapultTarget == null)
        {
            if (logEvents) Debug.LogWarning("[RingJumpTrigger] Kein catapultTarget zugewiesen.");
            yield break;
        }

        isJumping = true;

        if (gravityWalker != null)
            gravityWalker.enabled = false;

        Vector3 startPos = playerRb.position;
        Vector3 endPos = catapultTarget.position;
        // Bogenachse zum Absprungzeitpunkt merken (aktuelle Wandnormale) -
        // waehrend des Flugs bleibt sie fix, damit der Bogen sauber aussieht,
        // auch wenn GravityWalker deaktiviert ist.
        Vector3 arcAxis = gravityWalker != null ? gravityWalker.transform.up : Vector3.up;

        yield return FlyArc(startPos, endPos, arcAxis, catapultArcHeight, catapultDuration);

        // GravityWalker wieder aktivieren - falls die Zielwand eine eigene
        // GravityZone hat, sollte deren OnTriggerEnter beim Eintreffen
        // bereits (oder kurz danach) die neue Richtung gesetzt haben.
        if (gravityWalker != null)
            gravityWalker.enabled = true;

        hasLandedInRing = false;
        isJumping = false;

        if (logEvents) Debug.Log("[RingJumpTrigger] Katapult-Flug abgeschlossen.");
    }

    IEnumerator FlyArc(Vector3 startPos, Vector3 endPos, Vector3 arcAxis, float arcHeight, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 flatPos = Vector3.Lerp(startPos, endPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            Vector3 targetPos = flatPos + arcAxis * arc;

            playerRb.MovePosition(targetPos);
            yield return new WaitForFixedUpdate();
        }

        playerRb.MovePosition(endPos);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        if (landingPoint != null)
        {
            Gizmos.DrawWireSphere(landingPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, landingPoint.position);
        }
        if (catapultTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(catapultTarget.position, 0.25f);
            if (landingPoint != null)
                Gizmos.DrawLine(landingPoint.position, catapultTarget.position);
        }
    }
}