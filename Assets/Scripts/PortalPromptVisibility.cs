using UnityEngine;

/// <summary>
/// Attach to the Bobby Car (or any portal object).
/// Shows/hides a child Canvas when the player enters/exits the trigger zone.
/// The Canvas must be a child of this object.
/// Requires a SphereCollider set as Trigger on this object.
/// </summary>
public class PortalPromptVisibility : MonoBehaviour
{
    [Tooltip("Drag your world-space Canvas child here")]
    public GameObject promptCanvas;

    void Start()
    {
        if (promptCanvas == null)
            Debug.LogError("[PortalPromptVisibility] promptCanvas not assigned!");
        else
            promptCanvas.SetActive(false); // hidden by default
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (promptCanvas != null) promptCanvas.SetActive(true);
        Debug.Log("[PortalPromptVisibility] Prompt shown");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (promptCanvas != null) promptCanvas.SetActive(false);
        Debug.Log("[PortalPromptVisibility] Prompt hidden");
    }
}
