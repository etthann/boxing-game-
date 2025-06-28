using UnityEngine;

public class BouncyWall : MonoBehaviour
{
    [Tooltip("How strong the bounce is when the player hits the wall.")]
    public float bounceForce = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object that hit this wall is the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the direction the player came from
            Vector3 bounceDirection = collision.contacts[0].normal;

            // Try to get the player's controller or Rigidbody
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                // Apply bounce force
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(bounceDirection * bounceForce, ForceMode.VelocityChange);

            }
        }
    }
}
