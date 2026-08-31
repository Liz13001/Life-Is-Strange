using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [Header("NPC Status")]
    public bool npcWoken = false;

    [Header("Level Progress")]
    public bool hasVisitedLevel2 = false;

    [Header("Visited Scenes Tracking")]
    public List<string> visitedScenes = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSceneVisit(string sceneName)
    {
        if (!visitedScenes.Contains(sceneName))
            visitedScenes.Add(sceneName);
    }

    public bool HasCompletedFullLoop(int minLevels, string requiredFinalScene)
    {
        return visitedScenes.Count >= minLevels && visitedScenes.Contains(requiredFinalScene);
    }
}