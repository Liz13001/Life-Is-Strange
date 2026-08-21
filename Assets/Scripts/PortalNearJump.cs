using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalNearJump : MonoBehaviour
{
    public string targetScene;
    public float triggerRadius = 8f;
    public Transform carTransform;       // drag Small Bobby Car Pivot hier rein
    public GameObject promptUI;          // drag ein UI Text/Image "Press SPACE to enter" rein

    private bool isNear = false;

    private bool GetJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) return true;

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

    void Update()
    {
        if (carTransform == null) return;

        float dist = Vector3.Distance(carTransform.position, transform.position);
        isNear = dist <= triggerRadius;

        if (promptUI != null)
            promptUI.SetActive(isNear);

        if (isNear && GetJumpInput())
            SceneManager.LoadScene(targetScene);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}