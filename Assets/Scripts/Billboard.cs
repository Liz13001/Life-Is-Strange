using UnityEngine;

/// <summary>
/// Attach to any world-space Canvas or GameObject.
/// Makes it always face the main camera.
/// </summary>
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.rotation = Camera.main.transform.rotation;
    }
}
