using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneByName : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load (must be added to Build Settings)")]
    public string sceneName;

    [Tooltip("Only trigger for objects with this tag (leave empty to allow any collider)")]
    public string requiredTag = "Player";

    [Tooltip("ID of the SpawnPoint in the target scene the player should appear at (leave empty to skip)")]
    public string targetSpawnPointId;

    [Header("Disappear On Trigger")]
    [Tooltip("Optional: this object will be made invisible/disabled when the trigger is entered")]
    public GameObject objectToHide;

    [Tooltip("If true, disables the whole GameObject (SetActive false). If false, only disables its Renderer (stays active, just invisible).")]
    public bool disableWholeObject = true;

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LoadSceneByName: no sceneName set on " + gameObject.name);
            return;
        }

        if (objectToHide != null)
        {
            if (disableWholeObject)
            {
                objectToHide.SetActive(false);
            }
            else
            {
                Renderer rend = objectToHide.GetComponent<Renderer>();
                if (rend != null)
                    rend.enabled = false;
            }
        }

        SceneTransitionData.targetSpawnPointId = targetSpawnPointId;
        SceneManager.LoadScene(sceneName);
    }
}