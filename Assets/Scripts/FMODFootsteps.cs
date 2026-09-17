using UnityEngine;
using FMODUnity;

public class FMODFootsteps : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference footstepEvent;

    [Header("Footstep Settings")]
    public float stepDistance = 0.8f;
    
    [Header("Surface")]
    public string surface = "Concrete";

    private Vector3 lastPosition;
    private float distanceTravelled;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Measure how far the player moved horizontally.
        Vector3 currentPosition = transform.position;

        Vector3 horizontalMovement = new Vector3(
            currentPosition.x - lastPosition.x,
            0f,
            currentPosition.z - lastPosition.z
        );

        distanceTravelled += horizontalMovement.magnitude;
        lastPosition = currentPosition;

        // Trigger a footstep after travelling the chosen distance.
        if (distanceTravelled >= stepDistance)
        {
            PlayFootstep();
            distanceTravelled = 0f;
        }
    }

    void PlayFootstep()
    {
        var instance = RuntimeManager.CreateInstance(footstepEvent);

        instance.setParameterByNameWithLabel("Surface", surface);

        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform.position)
        );

        instance.start();
        instance.release();
    }
}