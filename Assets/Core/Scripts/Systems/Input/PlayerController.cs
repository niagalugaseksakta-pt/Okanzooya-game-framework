using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    public float speed = 3f;
    public float runMultiplier = 1.5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var input = InputManager.Instance.GetInput();
        Vector2 move = new Vector2(input.Horizontal, input.Vertical);

        if (move.magnitude > 0.1f)
        {
            float finalSpeed = speed * (move.magnitude > 0.8f ? runMultiplier : 1f);
            rb.linearVelocity = move.normalized * finalSpeed;

            animator.SetTrigger(move.magnitude > 0.8f ? "Run" : "Walk");
            transform.localScale = new Vector3(move.x > 0 ? 1 : -1, 1, 1);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetTrigger("Idle");
        }

        if (input.Attack)
        {
            animator.SetTrigger("Attack");
        }
    }
}
