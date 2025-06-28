using UnityEngine;

public class PlayerHitResponse : MonoBehaviour
{
    public float pushForce = 0;
    public float recoveryTime = 0.3f;
    public float invincibilityTime = 0.5f; // Time during which player cannot be hit again

    private Rigidbody rb;
    private bool isHit = false;
    private float recoveryTimer = 0f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle invincibility timer first
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("Player is no longer invincible");
            }
        }

        if (isHit)
        {
            recoveryTimer -= Time.deltaTime;
            if (recoveryTimer <= 0f)
            {
                isHit = false;
                rb.linearVelocity = Vector3.zero; // stop movement
            }
        }
    }

}
