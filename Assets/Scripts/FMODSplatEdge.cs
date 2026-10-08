using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODSplatEdge : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Splat Edge Colliders")]
    public Collider[] edgeColliders;

    [Header("Proximity")]
    [Tooltip("Distance from a splat edge at which SplatEdge begins increasing.")]
    public float maxDistance = 6f;

    [Header("FMOD")]
    public EventReference glitchEvent;
    public EventReference snapshotEvent;

    private EventInstance glitchInstance;
    private EventInstance snapshotInstance;

    void Start()
    {
        // Start the glitch event and keep it running.
        glitchInstance = RuntimeManager.CreateInstance(glitchEvent);
        glitchInstance.start();

        // Start the snapshot and keep it running.
        snapshotInstance = RuntimeManager.CreateInstance(snapshotEvent);
        snapshotInstance.start();
    }

    void Update()
    {
        if (player == null || edgeColliders == null || edgeColliders.Length == 0)
            return;

        float nearestDistance = Mathf.Infinity;

        foreach (Collider edge in edgeColliders)
        {
            if (edge == null)
                continue;

            Vector3 closestPoint = edge.ClosestPoint(player.position);
            float distance = Vector3.Distance(player.position, closestPoint);

            if (distance < nearestDistance)
                nearestDistance = distance;
        }

       float splatEdge = 1f - Mathf.Clamp01(nearestDistance / maxDistance);
       
       Debug.Log("SplatEdge: " + splatEdge.ToString("F2") +
          " | Distance: " + nearestDistance.ToString("F2") + " m");

        // Drive the glitch composition.
        glitchInstance.setParameterByName("SplatEdge", splatEdge);

        // Use the same value as the snapshot intensity.
        snapshotInstance.setParameterByName("Intensity", splatEdge);
    }

    void OnDestroy()
    {
        glitchInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        glitchInstance.release();

        snapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        snapshotInstance.release();
    }
}