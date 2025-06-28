using UnityEngine;

public class AIBoxer : MonoBehaviour
{
    public Animator animator;

    private float punchCooldown = 4f;
    private float timer;

    void Start()
    {
        animator = GetComponent<Animator>();
        timer = punchCooldown;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerRandomPunch();
            timer = punchCooldown; // reset the timer
        }
    }

    void TriggerRandomPunch()
    {
        int randomPunch = Random.Range(0, 2); // 0 = Left, 1 = Right

        if (randomPunch == 0)
        {
            animator.SetTrigger("PunchLeft");
        }
        else
        {
            animator.SetTrigger("PunchRight");
        }
    }
}
