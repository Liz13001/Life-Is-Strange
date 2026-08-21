using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLookOutward : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName = "Zimmer";

    [Header("Circle Center")]
    public Transform circleCenter;

    [Header("Facing")]
    [Range(-1f, 1f)]
    public float facingThreshold = 0.5f;

    [Header("Zoom Threshold")]
    public float triggerFOV = 25f;

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    bool _transitioning;
    float _fadeAlpha;
    Texture2D _black;

    FirstPersonController _fpc;
    FollowTarget _followTarget;

    void Start()
    {
        _fpc = FindFirstObjectByType<FirstPersonController>();
        if (_fpc == null)
            Debug.LogError("[PortalLookOutward] FirstPersonController not found!");

        _followTarget = _fpc != null ? _fpc.GetComponent<FollowTarget>() : null;
    }

    void Update()
    {
        if (_transitioning) return;
        if (_fpc == null) return;
        if (circleCenter == null) return;

        Vector3 outwardDir = (_fpc.playerCamera.transform.position - circleCenter.position).normalized;
        outwardDir.y = 0f;

        Vector3 camForward = _fpc.playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        float dot = Vector3.Dot(camForward, outwardDir);
        bool isLookingOutward = dot >= facingThreshold;
        bool isZoomed = _fpc.playerCamera.fieldOfView <= triggerFOV;

        if (isLookingOutward && isZoomed)
            StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        _fpc.playerCanMove = false;
        _fpc.cameraCanMove = false;

        if (_followTarget != null) _followTarget.enabled = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        _fadeAlpha = 1f;

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