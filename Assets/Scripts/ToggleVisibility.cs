using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.V;
    private Renderer[] renderers;
    private bool isVisible = true;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            foreach (Renderer r in renderers)
            {
                r.enabled = isVisible;
            }
        }
    }
}