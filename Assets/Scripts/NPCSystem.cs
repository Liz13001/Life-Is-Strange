
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

    [Header("Dialog - Nach Flower Event")]
    [TextArea(2, 5)]
    public string[] flowerEventDialogLines;

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    private int currentLine = 0;
    private string[] activeDialogLines;
    private bool playerInside = false;

    void Awake()
    {
        HideDialog();
    }

    void Start()
    {
        UpdateDialog();
        HideDialog();
    }

    void UpdateDialog()
    {
        bool woken = GameState.Instance != null &&
                     GameState.Instance.npcWoken;

        bool flowerEvent = GameState.Instance != null &&
                           GameState.Instance.flowerEventTriggered;

        if (flowerEvent)
        {
            activeDialogLines = flowerEventDialogLines;
        }
        else if (woken)
        {
            activeDialogLines = awakeDialogLines;
        }
        else
        {
            activeDialogLines = sleepingDialogLines;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Verhindert mehrfaches Auslösen
        if (playerInside) return;

        playerInside = true;

        // Aktuellen GameState prüfen
        UpdateDialog();

        if (subtitleText == null ||
            activeDialogLines == null ||
            activeDialogLines.Length == 0)
            return;

        // Nächste Dialogzeile anzeigen
        subtitleText.text = activeDialogLines[currentLine %
                                              activeDialogLines.Length];

        subtitleText.gameObject.SetActive(true);

        // Zur nächsten Zeile wechseln
        currentLine = (currentLine + 1) %
                      activeDialogLines.Length;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        HideDialog();
    }

    void HideDialog()
    {
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        playerInside = false;
        HideDialog();
    }
}
