using UnityEngine;

public class PoppersObject : EntityObstacleBase
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "BloomPop";

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float upwardBoost = 1.5f;
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;

    [Header("Settings")]
    [SerializeField] private bool disableAfterPop = true;
    [SerializeField] private float disableDelay = 0.35f;

    private bool isPopped = false;

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isPopped) return;
        if (!other.CompareTag("Player")) return;

        Pop();
        ApplyKnockback(other);
    }

    private void Pop()
    {
        isPopped = true;

        if (animator != null)
            animator.SetTrigger(triggerName);

        if (disableAfterPop)
            Invoke(nameof(DisableObject), disableDelay);
    }

    private void ApplyKnockback(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Arah knockback (player menjauh dari poppers)
        Vector2 dir = (other.transform.position - transform.position).normalized;

        // Tambahkan sedikit upward
        dir.y += upwardBoost;

        rb.AddForce(dir * knockbackForce, forceMode);
    }

    private void DisableObject()
    {
        gameObject.SetActive(false);
    }
}
