using UnityEngine;

public class SkyboxTrigger : MonoBehaviour
{
    public Material skyboxB;

    private Material skyboxA;
    private bool toggled = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!toggled)
        {
            skyboxA = RenderSettings.skybox; // aktuelle Skybox merken
            RenderSettings.skybox = skyboxB;
        }
        else
        {
            RenderSettings.skybox = skyboxA; // zurück zur gemerkten
        }

        toggled = !toggled;
        DynamicGI.UpdateEnvironment();
    }
}