using UnityEngine;

// In der Zielszene auf den Player legen (oder ein leeres Manager-Objekt,
// dann playerTransform manuell zuweisen).
public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("Leer lassen, wenn das Script direkt auf dem Player sitzt")]
    public Transform playerTransform;

    [Tooltip("Falls kein Spawn-Point gefunden wird oder keine ID gesetzt ist, an aktueller Position bleiben")]
    public bool fallbackToCurrentPosition = true;

    private Rigidbody rb;

    void Start()
    {
        if (playerTransform == null) playerTransform = transform;
        rb = playerTransform.GetComponent<Rigidbody>();

        string targetId = SceneTransitionData.targetSpawnPointId;

        if (string.IsNullOrEmpty(targetId))
        {
            if (!fallbackToCurrentPosition)
                Debug.LogWarning("PlayerSpawner: keine targetSpawnPointId gesetzt.");
            return;
        }

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        SpawnPoint match = null;

        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.spawnId == targetId)
            {
                match = sp;
                break;
            }
        }

        if (match != null)
        {
            MovePlayerTo(match.transform.position, match.transform.rotation);
        }
        else
        {
            Debug.LogWarning("PlayerSpawner: kein SpawnPoint mit ID '" + targetId + "' in dieser Szene gefunden.");
        }

        // ID zurücksetzen, damit ein erneutes manuelles Laden dieser Szene
        // (z.B. per Menü) nicht versehentlich wieder denselben Spawn nutzt
        SceneTransitionData.targetSpawnPointId = null;
    }

    void MovePlayerTo(Vector3 position, Quaternion rotation)
    {
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
        }

        playerTransform.SetPositionAndRotation(position, rotation);
    }
}