
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODSplatEdge : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Safe Spheres")]
    [Tooltip("Areas where the glitch intensity is zero.")]
    public SphereCollider[] safeSpheres;

    [Tooltip("Only spheres on these layers are considered.")]
    public LayerMask audioZoneLayers;

    [Header("Glitch Distance")]
    [Tooltip("Distance outside a sphere where glitch reaches 1.")]
    public float maxDistance = 10f;

    [Tooltip("How quickly glitch intensity changes.")]
    public float smoothingSpeed = 3f;

    [Header("FMOD")]
    public EventReference glitchEvent;
    public EventReference snapshotEvent;

    private EventInstance glitchInstance;
    private EventInstance snapshotInstance;

    private float currentIntensity = 0f;

    void Start()
    {
        glitchInstance = RuntimeManager.CreateInstance(glitchEvent);

        if (glitchInstance.isValid())
        {
            glitchInstance.setParameterByName("SplatEdge", 0f);
            glitchInstance.start();
        }

        snapshotInstance = RuntimeManager.CreateInstance(snapshotEvent);

        // The snapshot starts here, but its intensity
        // is not yet controlled by this script.
        // We will connect that separately in FMOD.
        if (snapshotInstance.isValid())
        {
            snapshotInstance.start();
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float nearestDistance = Mathf.Infinity;
        bool foundSphere = false;

        if (safeSpheres != null)
        {
            foreach (SphereCollider sphere in safeSpheres)
            {
                if (sphere == null ||
                    !sphere.enabled ||
                    !sphere.gameObject.activeInHierarchy)
                    continue;

                // Ignore spheres on other layers.
                if ((audioZoneLayers.value &
                    (1 << sphere.gameObject.layer)) == 0)
                    continue;

                foundSphere = true;

                // Inside a sphere, distance is zero.
                Vector3 closestPoint =
                    sphere.ClosestPoint(player.position);

                float distance = Vector3.Distance(
                    player.position,
                    closestPoint
                );

                if (distance < nearestDistance)
                    nearestDistance = distance;
            }
        }

        float targetIntensity = 0f;

        if (foundSphere)
        {
            // Inside either sphere: 0
            // Outside: increases towards 1
            targetIntensity = Mathf.Clamp01(
                nearestDistance /
                Mathf.Max(0.01f, maxDistance)
            );
        }

        // Smooth changes in intensity.
        currentIntensity = Mathf.MoveTowards(
            currentIntensity,
            targetIntensity,
            Mathf.Max(0f, smoothingSpeed) * Time.deltaTime
        );

        // Control the glitch composition.
        if (glitchInstance.isValid())
        {
            glitchInstance.setParameterByName(
                "SplatEdge",
                currentIntensity
            );
        }
    }

    void OnDestroy()
    {
        if (glitchInstance.isValid())
        {
            glitchInstance.stop(
                FMOD.Studio.STOP_MODE.IMMEDIATE
            );
            glitchInstance.release();
        }

        if (snapshotInstance.isValid())
        {
            snapshotInstance.stop(
                FMOD.Studio.STOP_MODE.IMMEDIATE
            );
            snapshotInstance.release();
        }
    }
}
