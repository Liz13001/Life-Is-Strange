using UnityEngine;

// Manuell in der Szene platzieren (leeres GameObject an der gewünschten
// Spawn-Position/-Rotation), spawnId muss zur ID im LoadSceneByName-Script
// auf der auslösenden Seite passen.
public class SpawnPoint : MonoBehaviour
{
    public string spawnId;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward * 0.8f);
    }
}