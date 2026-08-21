using UnityEngine;

public class NPCSwapOnWake : MonoBehaviour
{
    [SerializeField] private GameObject defaultNpc;  // shown if NOT woken
    [SerializeField] private GameObject wokenNpc;     // shown if woken (sitting on car)

    private void Start()
    {
        bool woken = GameState.Instance.npcWoken;
        defaultNpc.SetActive(!woken);
        wokenNpc.SetActive(woken);
    }
}