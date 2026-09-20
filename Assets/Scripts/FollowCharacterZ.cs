using UnityEngine;

public class FollowCharacterZ : MonoBehaviour
{
    [Header("Character to follow")]
    public Transform target;

    [Header("Player root to move")]
    public Transform playerRoot;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.O;

    private bool followActive = false;
    private float zOffset;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (!followActive)
            {
                if (target == null || playerRoot == null)
                    return;

                // Remember current distance between player and character
                zOffset = playerRoot.position.z - target.position.z;

                followActive = true;

                Debug.Log("Camera Z-Follow ON");
            }
            else
            {
                followActive = false;

                Debug.Log("Camera Z-Follow OFF");
            }
        }
    }

    void LateUpdate()
    {
        if (!followActive || target == null || playerRoot == null)
            return;

        Vector3 position = playerRoot.position;

        position.z = target.position.z + zOffset;

        playerRoot.position = position;
    }
}