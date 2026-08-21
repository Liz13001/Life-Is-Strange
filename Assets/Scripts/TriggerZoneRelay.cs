using UnityEngine;

public class TriggerZoneRelay : MonoBehaviour
{
    public enum ZoneType { ZoneA, ZoneB }

    [Tooltip("Which zone this trigger represents")]
    public ZoneType zoneType;

    [Tooltip("Reference to the gate that tracks entry counts")]
    public SplineStartGate gate;

    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (zoneType == ZoneType.ZoneA)
            gate.RegisterZoneAEntry();
        else
            gate.RegisterZoneBEntry();
    }
}