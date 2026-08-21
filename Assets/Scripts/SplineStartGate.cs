using UnityEngine;

public class SplineStartGate : MonoBehaviour
{
    [Header("Zone Requirements")]
    [Tooltip("How many times Zone A must be entered")]
    public int zoneARequiredEntries = 2;

    [Tooltip("How many times Zone B must be entered")]
    public int zoneBRequiredEntries = 1;

    [Header("References")]
    [Tooltip("The spline mover to start once conditions are met")]
    public MoveAlongSpline splineMover;

    private int zoneACount = 0;
    private int zoneBCount = 0;
    private bool hasStarted = false;

    public void RegisterZoneAEntry()
    {
        if (hasStarted) return;

        zoneACount++;
        Debug.Log($"Zone A entered. Count: {zoneACount}/{zoneARequiredEntries}");
        CheckConditions();
    }

    public void RegisterZoneBEntry()
    {
        if (hasStarted) return;

        zoneBCount++;
        Debug.Log($"Zone B entered. Count: {zoneBCount}/{zoneBRequiredEntries}");
        CheckConditions();
    }

    private void CheckConditions()
    {
        if (zoneACount >= zoneARequiredEntries && zoneBCount >= zoneBRequiredEntries)
        {
            hasStarted = true;
            Debug.Log("Both zone conditions met — starting spline movement.");
            splineMover.StartMoving();
        }
    }
}