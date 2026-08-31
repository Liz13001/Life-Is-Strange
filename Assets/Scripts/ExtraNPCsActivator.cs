using UnityEngine;

public class ExtraNPCsActivator : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private int minLevelsVisited = 5;
    [SerializeField] private string requiredFinalScene = "Splat Installation";

    [Header("NPCs to Activate (once unlocked)")]
    [SerializeField] private GameObject[] npcsToActivate;

    [Header("NPCs to Deactivate (once unlocked)")]
    [SerializeField] private GameObject[] npcsToDeactivate;

    void Start()
    {
        if (GameState.Instance == null) return;

        bool unlocked = GameState.Instance.HasCompletedFullLoop(minLevelsVisited, requiredFinalScene);

        foreach (var npc in npcsToActivate)
        {
            if (npc != null) npc.SetActive(unlocked);
        }

        foreach (var npc in npcsToDeactivate)
        {
            if (npc != null) npc.SetActive(!unlocked);
        }
    }
}