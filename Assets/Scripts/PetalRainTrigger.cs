using UnityEngine;

public class PetalRainTrigger : MonoBehaviour
{
    public ParticleSystem petalRainA;
    public ParticleSystem petalRainB;

    private bool isPlaying = false;

    void Start()
    {
        if (petalRainA != null) petalRainA.Stop();
        if (petalRainB != null) petalRainB.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPlaying)
        {
            if (petalRainA != null) petalRainA.Stop();
            if (petalRainB != null) petalRainB.Stop();
        }
        else
        {
            if (petalRainA != null) petalRainA.Play();
            if (petalRainB != null) petalRainB.Play();
        }

        isPlaying = !isPlaying;
    }
}