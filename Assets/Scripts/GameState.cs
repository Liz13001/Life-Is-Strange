using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [Header("NPC Status")]
    public bool npcWoken = false;

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
}