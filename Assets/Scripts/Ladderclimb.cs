using UnityEngine;

// Läuft NACH der FirstPersonController FixedUpdate (Script Execution Order:
// Edit > Project Settings > Script Execution Order > LadderClimb weiter unten
// als FirstPersonController einordnen, oder einfach das Attribut hier nutzen).
[DefaultExecutionOrder(100)]
public class LadderClimb : MonoBehaviour
{
    [Header("Klettern")]
    public float climbSpeed = 3f;
    public float snapSpeed = 6f; // hält den Player mittig auf der Leiter

    [Header("Optional: seitliches Absteigen")]
    public float horizontalClimbSpeed = 1.5f;

    private Rigidbody rb;
    private Transform currentLadder;
    private bool isClimbing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            currentLadder = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder") && other.transform == currentLadder)
        {
            currentLadder = null;
            StopClimbing();
        }
    }

    void Update()
    {
        if (currentLadder == null)
        {
            if (isClimbing) StopClimbing();
            return;
        }

        float vertical = Input.GetAxis("Vertical");

        // Klettern startet automatisch, sobald man im Trigger vorwärts drückt
        if (!isClimbing && Mathf.Abs(vertical) > 0.1f)
        {
            StartClimbing();
        }

        // Mit Jump-Taste jederzeit loslassen (z.B. um runterzuspringen)
        if (isClimbing && Input.GetButtonDown("Jump"))
        {
            StopClimbing();
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing || currentLadder == null) return;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Vertikale Bewegung entlang der Leiter-Achse
        Vector3 climbVelocity = currentLadder.up * vertical * climbSpeed;

        // Leichtes seitliches Absteigen entlang der Leiter-Rechts-Achse
        climbVelocity += currentLadder.right * horizontal * horizontalClimbSpeed;

        // Einrasten auf die Leiter-Mitte (senkrecht zur Leiter-Achse),
        // damit man nicht seitlich runterrutscht
        Vector3 toLadder = currentLadder.position - transform.position;
        Vector3 correction = Vector3.ProjectOnPlane(toLadder, currentLadder.up) * snapSpeed;

        // Überschreibt die velocity, die die FirstPersonController FixedUpdate
        // in diesem Frame bereits gesetzt hat
        rb.linearVelocity = climbVelocity + correction;
    }

    void StartClimbing()
    {
        isClimbing = true;
        rb.useGravity = false;
    }

    void StopClimbing()
    {
        isClimbing = false;
        rb.useGravity = true;
    }
}