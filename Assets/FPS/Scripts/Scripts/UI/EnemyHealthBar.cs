using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 lookPos = transform.position + cam.transform.rotation * Vector3.forward;
        Vector3 up = cam.transform.rotation * Vector3.up;
        transform.LookAt(lookPos, up);
    }
}
