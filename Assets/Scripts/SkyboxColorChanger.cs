using UnityEngine;

public class ZimmerSkyController : MonoBehaviour
{
    public Material normalSkybox;
    public Material returnSkybox;

    void Start()
    {
        if (GameState.Instance == null) return;

        RenderSettings.skybox = GameState.Instance.hasVisitedLevel2
            ? returnSkybox
            : normalSkybox;
    }
}