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

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.O;

    private bool followActive = false;

    private float startPlayerZ;

    private float suna1StartZ;
    private float suna2StartZ;
    private float asaf1StartZ;
    private float asaf2StartZ;

    void Start()
    {
        // Follow starts OFF.
        // IMPORTANT: The component itself stays enabled!
        followActive = false;

        Debug.Log("FollowPlayerZ ready - press O to toggle");
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (followActive)
            {
                followActive = false;
                Debug.Log("Z-Follow OFF");
            }
            else
            {
                ActivateFollow();
            }
        }
    }

    void ActivateFollow()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("FollowPlayerZ: Player Camera is missing!");
            return;
        }

        startPlayerZ = playerCamera.position.z;

        if (suna1 != null)
            suna1StartZ = suna1.position.z;

        if (suna2 != null)
            suna2StartZ = suna2.position.z;

        if (asaf1 != null)
            asaf1StartZ = asaf1.position.z;

        if (asaf2 != null)
            asaf2StartZ = asaf2.position.z;

        followActive = true;

        Debug.Log("Z-Follow ON");
    }

    void LateUpdate()
    {
        if (!followActive)
            return;

        if (playerCamera == null)
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