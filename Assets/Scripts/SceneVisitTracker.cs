using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneVisitTracker : MonoBehaviour
{
    void Start()
    {
        if (GameState.Instance == null) return;

        string currentScene = SceneManager.GetActiveScene().name;
        GameState.Instance.RegisterSceneVisit(currentScene);
    }
}