using UnityEngine;

public class LookAtAssist : MonoBehaviour
{
    public Transform target;             // Enemy head
    public float rotationSpeed = 2f;     // How fast to rotate
    public bool isLockedOn = true;       // Toggle lock

    void Update()
    {
        if (isLockedOn && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Only rotate around Y and X (FPS style)
            Vector3 euler = lookRotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(euler.x, euler.y, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
