using UnityEngine;

public class ClaxonWakeTrigger : MonoBehaviour
{
    [SerializeField] private bool oneShot = true;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && triggered) return;

        triggered = true;
        GameState.Instance.npcWoken = true;

        Debug.Log("Claxon hit — NPC will be woken up.");

        // optional: honk sound / animation here
    }
}