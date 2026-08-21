using UnityEngine;

public class CarJump : MonoBehaviour
{
    [Header("Jump")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    public float gravity = 15f;

    private float verticalVelocity = 0f;
    private float yOffset = 0f;
    private bool isGrounded = true;
    private bool isInPortalZone = false;

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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal")) isInPortalZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Portal")) isInPortalZone = false;
    }

    void Start()
    {
        // nichts
    }

    void Update()
    {
        if (isGrounded && !isInPortalZone && GetJumpInput())
            StartJump();

        if (!isGrounded)
            HandleJump();
    }

    void StartJump()
    {
        verticalVelocity = jumpPower;
        yOffset = 0f;
        isGrounded = false;

        MoveAlongSpline splineScript = GetComponent<MoveAlongSpline>();
        if (splineScript != null) splineScript.enabled = false;
    }

    void HandleJump()
    {
        verticalVelocity -= gravity * Time.deltaTime;
        yOffset += verticalVelocity * Time.deltaTime;
        transform.position += new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);

        if (yOffset <= 0f)
        {
            yOffset = 0f;
            verticalVelocity = 0f;
            isGrounded = true;

            MoveAlongSpline splineScript = GetComponent<MoveAlongSpline>();
            if (splineScript != null) splineScript.enabled = true;
        }
    }
}