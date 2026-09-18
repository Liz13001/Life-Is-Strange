using UnityEngine;

public class FollowPlayerZ : MonoBehaviour
{
    [Header("Player")]
    public Transform playerCamera;

    [Header("Characters")]
    public Transform suna1;
    public Transform suna2;
    public Transform asaf1;
    public Transform asaf2;

    private bool followActive = false;

    private float startPlayerZ;

    private float suna1StartZ;
    private float suna2StartZ;
    private float asaf1StartZ;
    private float asaf2StartZ;

    void Update()
    {
        // Nur beim ERSTEN P aktivieren
        if (!followActive && Input.GetKeyDown(KeyCode.P))
        {
            startPlayerZ = playerCamera.position.z;

            suna1StartZ = suna1.position.z;
            suna2StartZ = suna2.position.z;
            asaf1StartZ = asaf1.position.z;
            asaf2StartZ = asaf2.position.z;

            followActive = true;

            Debug.Log("Suna/Asaf Z-Follow aktiviert");
        }
    }

    void LateUpdate()
    {
        if (!followActive)
            return;

        float offsetZ = playerCamera.position.z - startPlayerZ;

        SetZ(suna1, suna1StartZ + offsetZ);
        SetZ(suna2, suna2StartZ + offsetZ);
        SetZ(asaf1, asaf1StartZ + offsetZ);
        SetZ(asaf2, asaf2StartZ + offsetZ);
    }

    void SetZ(Transform target, float z)
    {
        if (target == null)
            return;

        Vector3 pos = target.position;
        pos.z = z;
        target.position = pos;
    }
}