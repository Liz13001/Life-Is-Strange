using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    public bool hasVisitedLevel2 = false;
    public bool npcWoken = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}