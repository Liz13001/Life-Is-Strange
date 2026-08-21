using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using StarterAssets;
#endif

public class PortalJump : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName = "Bobby Car World";
    [Header("Input")]
    [Tooltip("Automatically uses the Zoom Key from FirstPersonController. This shows the active key.")]
    public KeyCode portalKey = KeyCode.Z; // fallback, wird automatisch vom FPC überschrieben
    [Header("Facing")]
    public Transform portalTarget;
    [Range(-1f, 1f)]
    public float facingThreshold = 0.5f;
    [Header("Fade")]
    public float fadeDuration = 0.8f;

    bool _playerNearby;
    bool _transitioning;
    float _fadeAlpha;
    Texture2D _black;

    // old controller
    FirstPersonController _fpc;
    // new starter asset controller
#if ENABLE_INPUT_SYSTEM
    StarterAssets.FirstPersonController _fpcNew;
#endif

    Camera _playerCamera;

    void Start()
    {
        // try old controller first
        _fpc = FindFirstObjectByType<FirstPersonController>();

#if ENABLE_INPUT_SYSTEM
        // try new controller if old not found
        if (_fpc == null)
            _fpcNew = FindFirstObjectByType<StarterAssets.FirstPersonController>();
#endif

        // get camera and automatically sync zoom key
        if (_fpc != null)
        {
            _playerCamera = _fpc.playerCamera;
            portalKey = _fpc.zoomKey; // automatisch den Zoom Key vom FPC übernehmen
            Debug.Log("[PortalJump] Ready (old FPC) — target: " + targetSceneName + " — portalKey: " + portalKey);
        }
#if ENABLE_INPUT_SYSTEM
        else if (_fpcNew != null)
        {
            _playerCamera = Camera.main;
            Debug.Log("[PortalJump] Ready (Starter Assets FPC) — target: " + targetSceneName);
        }
#endif
        else
        {
            Debug.LogError("[PortalJump] No FirstPersonController found!");
        }
    }

    void Update()
    {
        if (_transitioning) return;
        if (_playerCamera == null) return;
        if (!_playerNearby) return;

        if (Input.GetKey(portalKey) || GetControllerPortal())
        {
            Transform target = portalTarget != null ? portalTarget : transform;
            Vector3 toPortal = (target.position - _playerCamera.transform.position).normalized;
            float dot = Vector3.Dot(_playerCamera.transform.forward, toPortal);

            Debug.Log("[PortalJump] dot=" + dot.ToString("F2") + " isFacing=" + (dot >= facingThreshold));

            if (dot >= facingThreshold)
            {
                Debug.Log("[PortalJump] Conditions met — entering portal!");
                StartCoroutine(FadeAndLoad());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = true;
        Debug.Log("[PortalJump] Player entered zone");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = false;
        Debug.Log("[PortalJump] Player left zone");
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;

        // disable movement on whichever controller is active
        if (_fpc != null)
        {
            _fpc.playerCanMove = false;
            _fpc.cameraCanMove = false;
        }
#if ENABLE_INPUT_SYSTEM
        else if (_fpcNew != null)
        {
            _fpcNew.MoveSpeed = 0f;
            _fpcNew.RotationSpeed = 0f;
        }
#endif

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        _fadeAlpha = 1f;
        Debug.Log("[PortalJump] Loading " + targetSceneName);
        if (GameState.Instance != null)
            GameState.Instance.hasVisitedLevel2 = true;
        SceneManager.LoadScene(targetSceneName);
    }

    private bool GetControllerPortal()
    {
        string[] joysticks = Input.GetJoystickNames();
        foreach (string j in joysticks)
        {
            if (string.IsNullOrEmpty(j)) continue;
            string jLower = j.ToLower();
            if (jLower.Contains("xbox") || jLower.Contains("xinput"))
                return Input.GetKey(KeyCode.JoystickButton1); // Xbox: B
            if (jLower.Contains("ps4") || jLower.Contains("wireless controller") || jLower.Contains("dualshock"))
                return Input.GetKey(KeyCode.JoystickButton2); // PS4: Kreis
        }
        return Input.GetKey(KeyCode.JoystickButton1); // Fallback
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