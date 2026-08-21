using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLook : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName = "Bobby Car World";

    [Header("Facing")]
    public Transform portalTarget;
    [Range(-1f, 1f)]
    public float facingThreshold = 0.5f;

    [Header("Zoom Threshold")]
    public float triggerFOV = 25f;

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    // ── internals ─────────────────────────────────────────────
    bool _transitioning;
    float _fadeAlpha;
    Texture2D _black;

    FirstPersonController _fpc;

    void Start()
    {
        _fpc = FindFirstObjectByType<FirstPersonController>();
        if (_fpc == null)
            Debug.LogError("[PortalLook] FirstPersonController not found!");
        else
            Debug.Log("[PortalLook] Ready — target: " + targetSceneName);
    }

    void Update()
    {
        if (_transitioning) return;
        if (_fpc == null) return;

        Transform target = portalTarget != null ? portalTarget : transform;
        Vector3 toPortal = (target.position - _fpc.playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(_fpc.playerCamera.transform.forward, toPortal);
        float currentFOV = _fpc.playerCamera.fieldOfView;

        Debug.Log("[PortalLook] dot=" + dot.ToString("F2") + " FOV=" + currentFOV.ToString("F1") + " isFacing=" + (dot >= facingThreshold) + " isZoomed=" + (currentFOV <= triggerFOV));

        bool isFacing = dot >= facingThreshold;
        bool isZoomed = currentFOV <= triggerFOV;

        if (isFacing && isZoomed)
        {
            Debug.Log("[PortalLook] Conditions met — entering portal!");
            StartCoroutine(FadeAndLoad());
        }
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        _fpc.playerCanMove = false;
        _fpc.cameraCanMove = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        _fadeAlpha = 1f;

        Debug.Log("[PortalLook] Loading " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    void OnGUI()
    {
        if (_fadeAlpha <= 0f) return;
        if (_black == null)
        {
            _black = new Texture2D(1, 1);
            _black.SetPixel(0, 0, Color.black);
            _black.Apply();
        }
        GUI.color = new Color(0, 0, 0, _fadeAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _black);
        GUI.color = Color.white;
    }
}