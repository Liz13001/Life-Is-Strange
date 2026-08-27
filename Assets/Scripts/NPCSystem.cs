using UnityEngine;
using TMPro;

public class NPCSystem : MonoBehaviour
{
    [Header("Dialog - Schlafend")]
    [TextArea(2, 5)]
    public string[] sleepingDialogLines;

    [Header("Dialog - Wach")]
    [TextArea(2, 5)]
    public string[] awakeDialogLines;

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    private int currentLine = 0;
    private string[] activeDialogLines;

    void Start()
    {
        subtitleText.gameObject.SetActive(false);

        bool woken = GameState.Instance != null && GameState.Instance.npcWoken;
        activeDialogLines = woken ? awakeDialogLines : sleepingDialogLines;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeDialogLines == null || activeDialogLines.Length == 0) return;

            subtitleText.text = activeDialogLines[currentLine];
            subtitleText.gameObject.SetActive(true);

            currentLine = (currentLine + 1) % activeDialogLines.Length;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            subtitleText.gameObject.SetActive(false);
        }
    }
}