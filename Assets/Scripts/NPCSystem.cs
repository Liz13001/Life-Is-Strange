using UnityEngine;
using TMPro;

public class NPCSystem : MonoBehaviour
{
    [Header("Dialog")]
    [TextArea(2, 5)]
    public string[] dialogLines;

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    private int currentLine = 0;

    void Start()
    {
        subtitleText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            subtitleText.text = dialogLines[currentLine];
            subtitleText.gameObject.SetActive(true);

            // Nächste Zeile vorbereiten, am Ende wieder von vorne
            currentLine = (currentLine + 1) % dialogLines.Length;
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