using UnityEngine;

public class CarPlayerRespawn : MonoBehaviour
{
    [Tooltip("Optional: assign the Player manually. If left empty, it will be found via tag on trigger.")]
    public Transform player;

    [Header("Hit Sound")]
    public AudioClip hitSound;
    [Range(0f, 1f)]
    public float hitVolume = 1f;
    private AudioSource audioSource;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private CharacterController playerController;

    void Start()
    {
        if (player != null)
        {
            CacheSpawn();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void CacheSpawn()
    {
        spawnPosition = player.position;
        spawnRotation = player.rotation;
        playerController = player.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // If player wasn't assigned in Inspector, grab it now and cache its position on first hit
        if (player == null)
        {
            player = other.transform;
            CacheSpawn();
        }

        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }

        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        // CharacterController blocks direct position changes unless disabled first
        if (playerController != null)
        {
            playerController.enabled = false;
            player.position = spawnPosition;
            player.rotation = spawnRotation;
            playerController.enabled = true;
        }
        else
        {
            player.position = spawnPosition;
            player.rotation = spawnRotation;
        }
    }
}