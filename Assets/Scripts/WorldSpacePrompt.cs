using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Attach to any portal object alongside PortalZoom.
/// Creates a floating world-space prompt above the object automatically.
/// No prefab needed — everything is built in code.
///
/// Setup:
///   1. Add this script to the Bobby Car (or any portal object)
///   2. Set the text and offset in the Inspector
///   3. Assign the resulting CanvasGroup to PortalZoom → Prompt UI
/// </summary>
public class WorldSpacePrompt : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("The message shown to the player")]
    public string promptText = "Press [Z] to enter";

    [Tooltip("Secondary label below — e.g. the world name")]
    public string worldLabel = "Bobby Car World";

    [Header("Position")]
    [Tooltip("How far above the object's pivot the prompt floats")]
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);

    [Header("Appearance")]
    public float fontSize       = 0.12f;
    public float labelFontSize  = 0.08f;
    public Color textColor      = Color.white;
    public Color labelColor     = new Color(1f, 0.85f, 0.4f); // warm yellow

    [Header("Animation")]
    [Tooltip("How much the prompt bobs up and down")]
    public float bobAmplitude = 0.06f;
    [Tooltip("Speed of the bob")]
    public float bobSpeed     = 1.4f;
    [Tooltip("Fade-in / fade-out duration in seconds")]
    public float fadeDuration = 0.35f;

    // ── public reference so PortalZoom can grab it ─────────────
    [HideInInspector] public CanvasGroup canvasGroup;

    // ── internals ──────────────────────────────────────────────
    Transform  _billboard;
    Vector3    _basePosition;
    float      _bobOffset;
    Coroutine  _fadeRoutine;

    void Awake()
    {
        BuildUI();
    }

    void BuildUI()
    {
        // ── Canvas ──────────────────────────────────────────────
        var canvasGO = new GameObject("PortalPromptCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = offset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale    = Vector3.one;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2f, 0.6f);

        canvasGroup        = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha  = 0f;                 // start hidden
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        _billboard    = canvasGO.transform;
        _basePosition = canvasGO.transform.localPosition;

        // ── Background panel ────────────────────────────────────
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var panelRT        = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin  = Vector2.zero;
        panelRT.anchorMax  = Vector2.one;
        panelRT.offsetMin  = Vector2.zero;
        panelRT.offsetMax  = Vector2.zero;

        var img        = panelGO.AddComponent<UnityEngine.UI.Image>();
        img.color      = new Color(0f, 0f, 0f, 0.55f);
        img.raycastTarget = false;

        // Rounded feel via sprite — fallback to solid if no sprite
        // (works fine as a semi-transparent dark pill)

        // ── World label (top, small) ─────────────────────────────
        var labelGO = new GameObject("WorldLabel");
        labelGO.transform.SetParent(panelGO.transform, false);

        var labelRT           = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin     = new Vector2(0f, 0.52f);
        labelRT.anchorMax     = new Vector2(1f, 1f);
        labelRT.offsetMin     = new Vector2(10f, 0f);
        labelRT.offsetMax     = new Vector2(-10f, -4f);

        var labelTMP              = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text             = worldLabel.ToUpper();
        labelTMP.fontSize         = labelFontSize;
        labelTMP.color            = labelColor;
        labelTMP.alignment        = TextAlignmentOptions.Center;
        labelTMP.fontStyle        = FontStyles.Bold;
        labelTMP.enableWordWrapping = false;
        labelTMP.raycastTarget    = false;

        // ── Main prompt text (bottom) ────────────────────────────
        var promptGO = new GameObject("PromptText");
        promptGO.transform.SetParent(panelGO.transform, false);

        var promptRT          = promptGO.AddComponent<RectTransform>();
        promptRT.anchorMin    = new Vector2(0f, 0f);
        promptRT.anchorMax    = new Vector2(1f, 0.55f);
        promptRT.offsetMin    = new Vector2(10f, 4f);
        promptRT.offsetMax    = new Vector2(-10f, 0f);

        var promptTMP              = promptGO.AddComponent<TextMeshProUGUI>();
        promptTMP.text             = promptText;
        promptTMP.fontSize         = fontSize;
        promptTMP.color            = textColor;
        promptTMP.alignment        = TextAlignmentOptions.Center;
        promptTMP.enableWordWrapping = false;
        promptTMP.raycastTarget    = false;
    }

    void Update()
    {
        if (canvasGroup == null) return;

        // Always face the camera
        if (Camera.main != null)
            _billboard.rotation = Camera.main.transform.rotation;

        // Bob up and down
        _bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        _billboard.localPosition = _basePosition + new Vector3(0f, _bobOffset, 0f);
    }

    // ── called by PortalZoom via ShowPrompt ────────────────────
    // (or you can call directly from any script)
    public void Show() => FadeTo(1f);
    public void Hide() => FadeTo(0f);

    void FadeTo(float target)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float t     = 0f;
        while (t < fadeDuration)
        {
            t                  += Time.deltaTime;
            canvasGroup.alpha   = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }
}
