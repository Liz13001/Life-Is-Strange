using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip footstepLoop; // a seamless looping walking sound
    [SerializeField] private float volume = 0.7f;

    [Header("Movement Detection")]
    [Tooltip("Minimum speed (units/sec) before footsteps start playing")]
    [SerializeField] private float speedThreshold = 0.2f;

    [Tooltip("Optional: reference to CharacterController. Leave empty if using Rigidbody.")]
    [SerializeField] private CharacterController characterController;

    [Tooltip("Optional: reference to Rigidbody. Leave empty if using CharacterController.")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("Only play footsteps while grounded (needs one of the above assigned)")]
    [SerializeField] private bool requireGrounded = true;

    private AudioSource audioSource;
    private Vector3 lastPosition;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = footstepLoop;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;

        lastPosition = transform.position;
    }

    private void Update()
    {
        float currentSpeed = GetCurrentSpeed();
        bool grounded = IsGrounded();
        bool shouldPlay = currentSpeed >= speedThreshold && (!requireGrounded || grounded);

        if (shouldPlay && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!shouldPlay && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private float GetCurrentSpeed()
    {
        // Prefer a real velocity source if available
        if (rb != null)
        {
            Vector3 flatVel = rb.linearVelocity; // use rb.velocity if on an older Unity version
            flatVel.y = 0f;
            return flatVel.magnitude;
        }

        if (characterController != null)
        {
            Vector3 flatVel = characterController.velocity;
            flatVel.y = 0f;
            return flatVel.magnitude;
        }

        // Fallback: measure displacement manually (works with any movement setup)
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        float speed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;
        return speed;
    }

    private bool IsGrounded()
    {
        if (characterController != null) return characterController.isGrounded;
        if (rb != null) return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        return true; // no reference assigned, assume grounded
    }
}