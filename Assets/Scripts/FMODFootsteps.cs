using UnityEngine;
using FMODUnity;

public class FMODFootsteps : MonoBehaviour
{
    [Header("FMOD Events")]
    public EventReference footstepEvent;
    public EventReference jumpEvent;
    public EventReference landEvent;

    [Header("Footstep Tempo")]
    public float walkingBPM = 114f;
    public float sprintingBPM = 180f;

    [Header("Movement Detection")]
    public float speedThreshold = 0.2f;
    public float sprintSpeedThreshold = 4.5f;

    [Header("Surface")]
    public string surface = "Concrete";

    private Rigidbody rb;
    private FirstPersonController controller;

    private float stepTimer = 0f;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<FirstPersonController>();

        if (rb == null)
        {
            Debug.LogError("FMODFootsteps needs a Rigidbody on the same GameObject.");
        }

        if (controller == null)
        {
            Debug.LogError("FMODFootsteps needs FirstPersonController on the same GameObject.");
        }

        if (controller != null)
        {
            wasGrounded = controller.IsGrounded;
        }
    }

    void Update()
    {
        if (rb == null || controller == null)
            return;

        bool isGrounded = controller.IsGrounded;

        // JUMP
        // Grounded last frame, airborne now.
        if (wasGrounded && !isGrounded)
        {
            PlayJump();
            stepTimer = 0f;
        }

        // LAND
        // Airborne last frame, grounded now.
        if (!wasGrounded && isGrounded)
        {
            PlayLand();
            stepTimer = 0f;
        }

        // FOOTSTEPS
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        bool isWalking = speed > speedThreshold;
        bool isSprinting = speed > sprintSpeedThreshold;

        if (isGrounded && isWalking)
        {
            float currentBPM = isSprinting ? sprintingBPM : walkingBPM;
            float stepInterval = 60f / currentBPM;

            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer -= stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }

        wasGrounded = isGrounded;
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

    void PlayJump()
    {
        var instance = RuntimeManager.CreateInstance(jumpEvent);

        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform.position)
        );

        instance.start();
        instance.release();
    }

    void PlayLand()
    {
        var instance = RuntimeManager.CreateInstance(landEvent);

        instance.setParameterByNameWithLabel("Surface", surface);
        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform.position)
        );

        instance.start();
        instance.release();
    }
}