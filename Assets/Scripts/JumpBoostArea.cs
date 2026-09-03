using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpBoostArea : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostForce = 8f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
    [SerializeField] private bool boostOnEveryJumpPress = true; // sonst nur einmal beim Eintritt

    [Header("One-Time Entry Boost (optional)")]
    [SerializeField] private bool boostOnEnter = false;
    [SerializeField] private float enterBoostForce = 5f;

    [Header("Debug")]
    [SerializeField] private bool logEvents = false;

    private bool playerInside = false;
    private Rigidbody playerRb;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerRb = other.attachedRigidbody;
        if (playerRb == null)
        {
            if (logEvents) Debug.LogWarning("[JumpBoostArea] Player hat keinen Rigidbody im Trigger-Collider.");
            return;
        }

        playerInside = true;
        if (logEvents) Debug.Log("[JumpBoostArea] Player betritt Boost-Zone.");

        if (boostOnEnter)
        {
            ApplyBoost(enterBoostForce);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        playerRb = null;
        if (logEvents) Debug.Log("[JumpBoostArea] Player verlässt Boost-Zone.");
    }

    private void Update()
    {
        if (!playerInside || playerRb == null || !boostOnEveryJumpPress) return;

        if (Input.GetButtonDown("Jump"))
        {
            ApplyBoost(boostForce);
        }
    }

    private void ApplyBoost(float force)
    {
        // Vertikale Geschwindigkeit vorher kappen, damit sich Boosts nicht unkontrolliert aufaddieren
        Vector3 vel = playerRb.linearVelocity;
        vel.y = 0f;
        playerRb.linearVelocity = vel;

        playerRb.AddForce(Vector3.up * force, forceMode);

        if (logEvents) Debug.Log($"[JumpBoostArea] Boost angewendet: {force} ({forceMode})");
    }
}