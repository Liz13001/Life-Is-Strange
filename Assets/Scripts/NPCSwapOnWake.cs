using UnityEngine;

public class NPCSwapOnWake : MonoBehaviour
{
    [Header("NPC Variants")]
    [SerializeField] private GameObject sleepingVariantNpc; // shown while golem is still asleep
    [SerializeField] private GameObject awakeVariantNpc;     // shown once golem has woken up

    private void Start()
    {
        bool woken = GameState.Instance != null && GameState.Instance.npcWoken;

        if (sleepingVariantNpc != null) sleepingVariantNpc.SetActive(!woken);
        if (awakeVariantNpc != null) awakeVariantNpc.SetActive(woken);
    }
}