using UnityEngine;

/// <summary>
/// Kommt auf ein Trigger-Collider-Objekt (z.B. Box Collider), das du manuell
/// an einer Wand/Decke platzierst. Beim Betreten durch den Spieler wird die
/// Gravitationsrichtung am GravityWalker auf das eigene transform.up dieser
/// Zone gesetzt.
///
/// SETUP:
/// 1. Leeres GameObject an die Wand stellen, dort wo der Übergang beginnen soll
/// 2. Box Collider hinzufügen, "Is Trigger" aktivieren (passiert automatisch
///    via Reset(), falls du das Component frisch hinzufügst), Größe/Position
///    manuell an die Wandstelle anpassen
/// 3. Das Zonen-Objekt so ROTIEREN, dass seine grüne Y-Achse (Up, im Scene View
///    sichtbar) von der Wand weg ins Rauminnere zeigt - das wird die neue
///    "Boden"-Richtung für den Spieler
/// 4. Im Inspector: Gravity Walker-Feld auf dein GravityOrientation-Objekt ziehen
/// </summary>
[RequireComponent(typeof(Collider))]
public class GravityZone : MonoBehaviour
{
    [Tooltip("Das GravityWalker-Script auf GravityOrientation")]
    public GravityWalker gravityWalker;

    [Tooltip("Falls aktiv: beim Verlassen der Zone wird auf Revert Direction zurückgesetzt (z.B. normale Welt-Gravitation). Falls aus: Richtung bleibt bestehen, bis eine andere Zone betreten wird.")]
    public bool revertOnExit = false;
    public Vector3 revertDirection = Vector3.up;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (gravityWalker == null || gravityWalker.playerRb == null) return;
        if (other.attachedRigidbody != gravityWalker.playerRb) return;

        gravityWalker.SetGravityDirection(transform.up);
    }

    void OnTriggerExit(Collider other)
    {
        if (!revertOnExit) return;
        if (gravityWalker == null || gravityWalker.playerRb == null) return;
        if (other.attachedRigidbody != gravityWalker.playerRb) return;

        gravityWalker.SetGravityDirection(revertDirection);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1f);
        Gizmos.DrawSphere(transform.position + transform.up * 1f, 0.05f);
    }
}