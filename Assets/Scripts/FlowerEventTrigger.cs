
using UnityEngine;

public class FlowerEventTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameState.Instance != null)
        {
            GameState.Instance.flowerEventTriggered = true;
            Debug.Log("Flower Event triggered!");
        }
    }
}
