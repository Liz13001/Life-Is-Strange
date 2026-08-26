using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClaxonPortalJump : MonoBehaviour
{
    [Header("Ziel")]
    public string targetScene;
    public Transform pullPoint; // optional, sonst wird dieses Objekt selbst als Ziehpunkt genutzt

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpDuration = 1f;
    public float jumpArcHeight = 3f;
    public float rotateSpeed = 5f;

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    [Header("UI")]
    public GameObject promptUI;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip claxonSound;

    private bool isPlayerNear = false;
    private bool isPortalJumping = false;
    private bool _transitioning = false;
    private float _fadeAlpha = 0f;
    private Texture2D _black;

    private Transform playerTransform;
    private FollowTarget followTarget;
    private FirstPersonController fpc;

    private Vector3 jumpStartPos;
    private float jumpTimer;

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
        Debug.Log($"[ClaxonPortalJump] Trigger enter von: {other.name}, Tag: {other.tag}");

        if (!other.CompareTag("Player")) return;

        playerTransform = other.transform;
        followTarget = other.GetComponent<FollowTarget>();
        fpc = other.GetComponent<FirstPersonController>();

        isPlayerNear = true;
        if (promptUI != null) promptUI.SetActive(true);
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

        if (!isPortalJumping && isPlayerNear && playerTransform != null && GetJumpInput())
            StartPortalJump();

        if (isPortalJumping)
            HandlePortalJump();
    }

    void StartPortalJump()
    {
        isPortalJumping = true;
        jumpTimer = 0f;
        jumpStartPos = playerTransform.position;

        if (followTarget != null) followTarget.enabled = false;
        if (fpc != null) fpc.playerCanMove = false;

        if (audioSource != null && claxonSound != null)
            audioSource.PlayOneShot(claxonSound);

        GameState.Instance.npcWoken = true;
    }

    void HandlePortalJump()
    {
        Transform target = pullPoint != null ? pullPoint : transform;

        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        // horizontale Bewegung (lineare Interpolation zum Ziel)
        Vector3 horizontalPos = Vector3.Lerp(jumpStartPos, target.position, t);

        // vertikaler Bogen (Sinus-Kurve, 0 am Start/Ende, Maximum in der Mitte)
        float arc = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
        horizontalPos.y += arc;

        playerTransform.position = horizontalPos;

        // Rotation Richtung Ziel während des Sprungs
        Vector3 toPortal = (target.position - playerTransform.position);
        toPortal.y = 0f;
        if (toPortal != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPortal.normalized, Vector3.up);
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (t >= 1f)
            StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        isPortalJumping = false;
        if (fpc != null) fpc.cameraCanMove = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        _fadeAlpha = 1f;

        SceneManager.LoadScene(targetScene);
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