using UnityEngine;

public class SleepingNPC : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string sleepBoolParam = "IsAsleep"; // Bool param in Animator

    private void Start()
    {
        bool woken = GameState.Instance.npcWoken;
        animator.SetBool(sleepBoolParam, !woken);

        Debug.Log(woken ? "NPC is awake." : "NPC is still asleep.");
    }
}