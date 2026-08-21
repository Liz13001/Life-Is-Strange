using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a trigger collider (e.g. a Box Collider with "Is Trigger" enabled).
/// When the first-person player enters the trigger, the next scene in the
/// Build Settings list is loaded automatically.
/// </summary>
public class PortalNextLevel : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tag of the first-person player GameObject.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Transition Settings")]
    [Tooltip("Optional delay in seconds before loading the next level.")]
    [SerializeField] private float delay = 0f;

    [Tooltip("If enabled, wraps back to scene 0 when the last scene is reached.")]
    [SerializeField] private bool wrapAround = false;

    // Prevent the trigger from firing multiple times
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        if (!other.CompareTag(playerTag)) return;

        _triggered = true;

        if (delay > 0f)
            Invoke(nameof(LoadNextLevel), delay);
        else
            LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (nextIndex >= totalScenes)
        {
            if (wrapAround)
            {
                nextIndex = 0;
            }
            else
            {
                Debug.LogWarning($"[PortalNextLevel] No next scene after index {currentIndex}. " +
                                 "Enable 'Wrap Around' or add more scenes to Build Settings.");
                _triggered = false; // allow retry after fixing build settings
                return;
            }
        }

        Debug.Log($"[PortalNextLevel] Loading scene index {nextIndex}.");
        SceneManager.LoadScene(nextIndex);
    }
}