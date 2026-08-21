using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPortalJump : MonoBehaviour
{
    [Header("Portal")]
    public Transform portalTransform;
    public string targetScene;
    public GameObject promptUI;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public float pullStrength = 15f;
    public float rotateSpeed = 5f;

    [Header("Fade")]
    public float fadeDuration = 0.8f;

    private bool isNearPortal = false;
    private bool isPortalJumping = false;
    private bool _transitioning = false;
    private float _fadeAlpha = 0f;
    private Texture2D _black;
    private FollowTarget followTarget;
    private FirstPersonController _fpc;

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
        followTarget = GetComponent<FollowTarget>();
        _fpc = GetComponent<FirstPersonController>();
        if (promptUI != null) promptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            isNearPortal = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            isNearPortal = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (_transitioning) return;

        if (!isPortalJumping && isNearPortal && GetJumpInput())
            StartPortalJump();

        if (isPortalJumping)
            HandlePortalJump();
    }

    void StartPortalJump()
    {
        isPortalJumping = true;
        if (followTarget != null) followTarget.enabled = false;
        if (_fpc != null) _fpc.playerCanMove = false;
    }

    void HandlePortalJump()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            portalTransform.position,
            pullStrength * Time.deltaTime
        );

        Vector3 toPortal = (portalTransform.position - transform.position).normalized;
        if (toPortal != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPortal, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, portalTransform.position) < 1.5f)
            StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        _transitioning = true;
        isPortalJumping = false;
        if (_fpc != null) _fpc.cameraCanMove = false;

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