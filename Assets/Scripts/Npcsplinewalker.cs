using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Bewegt dieses GameObject kontinuierlich entlang eines Splines im Loop.
/// Eigenständige, einfache Alternative zu SplineAnimate - volle Kontrolle, kein Editor-Only-Verhalten.
/// Einfach auf den NPC (das Root-Objekt mit der Laufanimation) legen und im Inspector
/// den SplineContainer zuweisen.
/// </summary>
public class NpcSplineWalker : MonoBehaviour
{
    [Header("Spline")]
    [Tooltip("Der Spline Container, dem der NPC folgen soll.")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Bewegung")]
    [Tooltip("Geschwindigkeit in Units pro Sekunde entlang des Splines.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Falls true, dreht sich der NPC in Bewegungsrichtung (entlang der Tangente).")]
    [SerializeField] private bool alignToSplineDirection = true;

    [Tooltip("Wie schnell der NPC seine Rotation der neuen Richtung anpasst (Grad/Sekunde). 0 = sofort.")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Loop")]
    [Tooltip("Falls true, läuft der NPC nach Erreichen des Endes wieder von vorne (kontinuierlicher Loop).")]
    [SerializeField] private bool loop = true;

    [Tooltip("Optionale Pause (in Sekunden) am Ende jeder Runde, bevor der Loop von vorne beginnt.")]
    [SerializeField] private float pauseAtLoopEnd = 0f;

    [Header("Start")]
    [Tooltip("Normalisierte Startposition auf dem Spline (0 = Anfang, 1 = Ende).")]
    [Range(0f, 1f)]
    [SerializeField] private float startOffset = 0f;

    [Tooltip("Ob die Bewegung automatisch beim Start beginnt.")]
    [SerializeField] private bool playOnStart = true;

    // interner State
    private float distanceTravelled; // aktuelle Distanz entlang des Splines in Units
    private float splineLength;
    private bool isPlaying;
    private bool isPaused;
    private float pauseTimer;

    private void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogError($"[NpcSplineWalker] Kein SplineContainer zugewiesen auf '{name}'.", this);
            enabled = false;
            return;
        }

        splineLength = splineContainer.CalculateLength();
        distanceTravelled = startOffset * splineLength;

        // NPC direkt an die Startposition setzen
        UpdateTransformAtDistance(distanceTravelled, snapRotation: true);

        isPlaying = playOnStart;
    }

    private void Update()
    {
        if (!isPlaying || splineContainer == null || splineLength <= 0f)
            return;

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
                isPaused = false;
            return;
        }

        distanceTravelled += speed * Time.deltaTime;

        if (distanceTravelled >= splineLength)
        {
            if (loop)
            {
                distanceTravelled -= splineLength; // Rest der Distanz in die neue Runde übernehmen

                if (pauseAtLoopEnd > 0f)
                {
                    isPaused = true;
                    pauseTimer = pauseAtLoopEnd;
                }
            }
            else
            {
                distanceTravelled = splineLength;
                isPlaying = false;
            }
        }

        UpdateTransformAtDistance(distanceTravelled, snapRotation: false);
    }

    private void UpdateTransformAtDistance(float distance, bool snapRotation)
    {
        // Distanz in normalisiertes t (0-1) umrechnen
        float t = splineLength > 0f ? distance / splineLength : 0f;
        t = Mathf.Clamp01(t);

        Vector3 position = splineContainer.EvaluatePosition(t);
        transform.position = position;

        if (alignToSplineDirection)
        {
            Vector3 tangent = splineContainer.EvaluateTangent(t);
            if (tangent.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);

                if (snapRotation || rotationSpeed <= 0f)
                    transform.rotation = targetRotation;
                else
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary> Bewegung starten/fortsetzen. </summary>
    public void Play() => isPlaying = true;

    /// <summary> Bewegung pausieren (Position bleibt erhalten). </summary>
    public void Pause() => isPlaying = false;

    /// <summary> Zurück an den Start und optional sofort wieder loslaufen. </summary>
    public void Restart(bool autoplay = true)
    {
        distanceTravelled = startOffset * splineLength;
        isPaused = false;
        UpdateTransformAtDistance(distanceTravelled, snapRotation: true);
        isPlaying = autoplay;
    }
}