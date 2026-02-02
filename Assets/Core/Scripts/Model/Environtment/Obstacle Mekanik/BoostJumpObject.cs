using UnityEngine;

public class BoostJumpObject : EntityObstacleBase
{
    public enum KnockDirection8
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "BoostJump";

    [Header("Knockback Settings")]
    [SerializeField] private KnockDirection8 knockDirection = KnockDirection8.Up;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float upwardBoost = 0f;
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    protected override void Awake()
    {
        animator ??= GetComponent<Animator>();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayAnimation();
        ApplyKnockback(other);
    }

    private void PlayAnimation()
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    private void ApplyKnockback(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = GetDirectionVector(knockDirection);

        if (upwardBoost != 0)
            dir.y += upwardBoost;

        //dir = dir.normalized;

        rb.AddForce(dir * knockbackForce, forceMode);
    }

    private Vector2 GetDirectionVector(KnockDirection8 dir)
    {
        return dir switch
        {
            KnockDirection8.Up => new Vector2(0, 1),
            KnockDirection8.Down => new Vector2(0, -1),
            KnockDirection8.Left => new Vector2(-1, 0),
            KnockDirection8.Right => new Vector2(1, 0),

            KnockDirection8.UpLeft => new Vector2(-1, 1),
            KnockDirection8.UpRight => new Vector2(1, 1),
            KnockDirection8.DownLeft => new Vector2(-1, -1),
            KnockDirection8.DownRight => new Vector2(1, -1),

            _ => Vector2.up
        };
    }
}
