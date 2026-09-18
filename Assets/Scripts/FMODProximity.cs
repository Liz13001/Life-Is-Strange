using UnityEngine;
using FMODUnity;

public class FMODProximity : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Collider proximityCollider;
    public StudioEventEmitter eventEmitter;

    [Header("Proximity")]
    [Tooltip("Distance from the collider surface at which Proximity reaches 0.")]
    public float maxDistance = 15f;

    void Update()
    {
        if (player == null || proximityCollider == null || eventEmitter == null)
            return;

        // Find the point on the collider closest to the player
        Vector3 closestPoint = proximityCollider.ClosestPoint(player.position);

        // Measure distance from player to the collider surface
        float distance = Vector3.Distance(player.position, closestPoint);

        // Convert distance into a 0–1 proximity value:
        // maxDistance or farther = 0
        // touching / inside collider = 1
        float proximity = 1f - Mathf.Clamp01(distance / maxDistance);

        // Send the value to the FMOD parameter
        eventEmitter.SetParameter("Proximity", proximity);
    }
}