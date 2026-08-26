using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLookOutward : MonoBehaviour
{
    [Header("Ziel")]
    public string targetSceneName = "Zimmer";

    [Header("Circle Center")]
    public Transform circleCenter;

    [Header("Facing")]
    [Range(-1f, 1f)]
    public float facingThreshold = 0.5f;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float outwardSpeed = 4f;
    public float jumpPower = 6f;
    public float gravity = 15f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.2f;
    public LayerMask groundLayer;
    public float minAirTime = 0.2f; // verhindert sofortiges "landen" direkt beim Absprung

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    [Header("UI")]
    public GameObject promptUI;

    bool isPlayerNear = false;
    bool isFalling = false;
    bool _transitioning;
    float _fadeAlpha;
    Texture2D _black;

    Transform playerTransform;
    FollowTarget followTarget;
    FirstPersonController fpc;
    Rigidbody playerRb;

    Vector3 fallVelocity;
    float airTimer;

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
        playerRb = other.GetComponent<Rigidbody>();

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
        if (isFalling) { HandleFall(); return; }

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
            StartFallJump(outwardDir);
    }

    void StartFallJump(Vector3 outwardDir)
    {
        isFalling = true;
        airTimer = 0f;

        if (promptUI != null) promptUI.SetActive(false);
        if (followTarget != null) followTarget.enabled = false;

        // Rigidbody-basierte Bewegung deaktivieren, falls FPC per Rigidbody läuft,
        // damit wir die Fall-Bewegung hier manuell per Transform steuern
        fpc.playerCanMove = false;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        fallVelocity = outwardDir * outwardSpeed + Vector3.up * jumpPower;
    }

    void HandleFall()
    {
        airTimer += Time.deltaTime;

        fallVelocity.y -= gravity * Time.deltaTime;
        playerTransform.position += fallVelocity * Time.deltaTime;

        // horizontale Bewegung dämpfen, damit der Player nicht endlos weiterfliegt
        fallVelocity.x = Mathf.MoveTowards(fallVelocity.x, 0f, outwardSpeed * Time.deltaTime);
        fallVelocity.z = Mathf.MoveTowards(fallVelocity.z, 0f, outwardSpeed * Time.deltaTime);

        if (airTimer < minAirTime) return;

        bool grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (grounded)
            StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        isFalling = false;
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