using UnityEngine;

public class ExtraNPCsActivator : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private int minLevelsVisited = 5;
    [SerializeField] private string requiredFinalScene = "Splat Installation";

    [Header("Extra NPCs")]
    [SerializeField] private GameObject[] extraNpcs;

    void Start()
    {
        if (GameState.Instance == null) return;

        bool unlocked = GameState.Instance.HasCompletedFullLoop(minLevelsVisited, requiredFinalScene);

        foreach (var npc in extraNpcs)
        {
            if (npc != null) npc.SetActive(unlocked);
        }
    }
}