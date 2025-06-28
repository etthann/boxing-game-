using UnityEngine;
using Unity.FPS.Game; // Required to access the Health component

public class EnemyPunchTrigger : MonoBehaviour
{
    public float pushForce = 10f;
    public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Punched");

            // Apply pushback
            PlayerHitResponse player = other.GetComponent<PlayerHitResponse>();
            if (player != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position);
            }

            // Apply health damage
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount, gameObject);
            }
        }
    }
}
