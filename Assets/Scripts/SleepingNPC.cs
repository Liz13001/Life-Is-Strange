using UnityEngine;

public class SleepingNPC : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string sleepBoolParam = "IsAsleep";

    [Header("Position nach dem Aufwachen")]
    [SerializeField] private Transform awakePosition; // wohin die NPC sich bewegt/teleportiert, wenn wach

    private void Start()
    {
        bool woken = GameState.Instance.npcWoken;
        animator.SetBool(sleepBoolParam, !woken);

        if (woken && awakePosition != null)
        {
            transform.position = awakePosition.position;
            transform.rotation = awakePosition.rotation;
        }

        Debug.Log(woken ? "NPC ist wach und an neuer Position." : "NPC schläft noch.");
    }
}