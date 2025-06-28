using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIEnemyController : MonoBehaviour
{
    public Transform player;
    public float attackRange = 1.3f;

    [Header("Punch Colliders")]
    public Collider leftHandCollider;
    public Collider rightHandCollider;
    public float colliderActiveTime = 0.3f;

    private NavMeshAgent agent;
    private Animator animator;
    private float punchTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        SetRandomPunchCooldown();

        if (leftHandCollider) leftHandCollider.enabled = false;
        if (rightHandCollider) rightHandCollider.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);

            punchTimer -= Time.deltaTime;
            if (punchTimer <= 0f)
            {
                TriggerRandomPunch();
                SetRandomPunchCooldown();
            }
        }
    }

    void TriggerRandomPunch()
    {
        int punchType = Random.Range(0, 2);
        if (punchType == 0)
        {
            animator.SetTrigger("PunchLeft");
            StartCoroutine(EnableColliderTemporarily(leftHandCollider));
        }
        else
        {
            animator.SetTrigger("PunchRight");
            StartCoroutine(EnableColliderTemporarily(rightHandCollider));
        }
    }

IEnumerator EnableColliderTemporarily(Collider handCollider)
{
    if (handCollider == null) yield break;

    handCollider.enabled = true;
    yield return new WaitForSeconds(colliderActiveTime);
    handCollider.enabled = false;
}


    void SetRandomPunchCooldown()
    {
        punchTimer = Random.Range(0.5f, 2f);
    }
}
