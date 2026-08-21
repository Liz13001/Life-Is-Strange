using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class PetalStopRotationOnCollision : MonoBehaviour
{
    public float minRotationSpeed = 45f;
    public float maxRotationSpeed = 180f;
    public float speedThreshold = 0.05f; // unter diesem Wert gilt Partikel als "gelandet"

    private ParticleSystem ps;
    private HashSet<uint> landedSeeds = new();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (ps.particleCount == 0) return;
        var particles = new ParticleSystem.Particle[ps.particleCount];
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // wenn Partikel kaum noch Geschwindigkeit hat -> gelandet
            if (particles[i].velocity.magnitude < speedThreshold)
                landedSeeds.Add(particles[i].randomSeed);

            if (landedSeeds.Contains(particles[i].randomSeed))
                particles[i].angularVelocity3D = Vector3.zero;
            else
                particles[i].angularVelocity3D = new Vector3(0, 0,
                    Random.Range(minRotationSpeed, maxRotationSpeed));
        }

        ps.SetParticles(particles, count);
    }
}