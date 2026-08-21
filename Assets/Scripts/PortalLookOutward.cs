using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLookOutward : MonoBehaviour
{
    [Header("Ziel")]
    public string targetSceneName = "Zimmer";
    public Transform pullPoint; // wohin der Player springt (z.B. Punkt im Zielraum-Bereich)

    [Header("Circle Center")]
    public Transform circleCenter;

    [Header("Facing")]
    [Range(-1f, 1f)]
    public float facingThreshold = 0.5f;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpDuration = 1f;
    public float jumpArcHeight = 3f;
    public float rotateSpeed = 5f;

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    [Header("UI")]
    public GameObject promptUI;

    bool isPlayerNear = false;
    bool isPortalJumping = false;
    bool _transitioning;
    float _fadeAlpha;
    Texture2D _black;

    Transform playerTransform;
    FollowTarget followTarget;
    FirstPersonController fpc;

    Vector3 jumpStartPos;
    float jumpTimer;

    private bool GetJumpInput()
    {
        if (Input.GetKeyDown(jumpKey)) return true;

        string[] joysticks = Input.GetJoystickNames();
        foreach (string j in joysticks)
        {
            if (string.IsNullOrEmpty(j)) continue;
            string jLower = j.ToLower();
            if (jLower.Contains("xbox") || jLower.Contains("xinput"))
                return Input.GetKeyDown(KeyCode.JoystickButton0);
            if (jLower.Contains("ps4") || jLower.Contains("wireless"))
                return Input.GetKeyDown(KeyCode.JoystickButton1);
        }
        return false;
    }

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerTransform = other.transform;
        followTarget = other.GetComponent<FollowTarget>();
        fpc = other.GetComponent<FirstPersonController>();

        isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.transform != playerTransform) return;

        isPlayerNear = false;
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        if (_transitioning) return;
        if (isPortalJumping) { HandlePortalJump(); return; }

        if (!isPlayerNear || playerTransform == null || fpc == null || circleCenter == null)
        {
            if (promptUI != null) promptUI.SetActive(false);
            return;
        }

        Vector3 outwardDir = (fpc.playerCamera.transform.position - circleCenter.position).normalized;
        outwardDir.y = 0f;

        Vector3 camForward = fpc.playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        float dot = Vector3.Dot(camForward, outwardDir);
        bool isLookingOutward = dot >= facingThreshold;

        if (promptUI != null) promptUI.SetActive(isLookingOutward);

        if (isLookingOutward && GetJumpInput())
            StartPortalJump();
    }

    void StartPortalJump()
    {
        isPortalJumping = true;
        jumpTimer = 0f;
        jumpStartPos = playerTransform.position;

        if (promptUI != null) promptUI.SetActive(false);
        if (followTarget != null) followTarget.enabled = false;
        fpc.playerCanMove = false;
    }

    void HandlePortalJump()
    {
        Transform target = pullPoint != null ? pullPoint : transform;

        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        Vector3 horizontalPos = Vector3.Lerp(jumpStartPos, target.position, t);
        float arc = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
        horizontalPos.y += arc;

        playerTransform.position = horizontalPos;

        Vector3 toTarget = target.position - playerTransform.position;
        toTarget.y = 0f;
        if (toTarget != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (t >= 1f)
            StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        isPortalJumping = false;
        fpc.cameraCanMove = false;

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