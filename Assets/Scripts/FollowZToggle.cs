using UnityEngine;

public class FollowZToggle : MonoBehaviour
{
    [Header("Follow Z Script")]
    public MonoBehaviour followZScript;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.O;

    void Start()
    {
        // Follow Z is OFF when the game starts
        if (followZScript != null)
        {
            followZScript.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (followZScript != null)
            {
                followZScript.enabled = !followZScript.enabled;
            }
        }
    }
}