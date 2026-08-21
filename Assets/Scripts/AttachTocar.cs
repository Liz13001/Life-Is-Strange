using UnityEngine;

public class AttachToCar : MonoBehaviour
{
    public GameObject bobbyCar;

    void Start()
    {
        Debug.Log("AttachToCar Start called on: " + gameObject.name);

        if (bobbyCar == null)
        {
            Debug.LogError("Bobby Car is NOT assigned in the Inspector!");
            return;
        }

        Debug.Log("Bobby Car found: " + bobbyCar.name);

        transform.SetParent(bobbyCar.transform);
        Debug.Log("Parent set to: " + transform.parent.name);

        transform.localPosition = new Vector3(0f, 0.3f, 0f);
        transform.localRotation = Quaternion.identity;

        Debug.Log("NPC local position: " + transform.localPosition);
        Debug.Log("NPC world position: " + transform.position);
    }

    void Update()
    {
        // Track every frame where the NPC is
        Debug.Log("NPC world position: " + transform.position);
    }
}