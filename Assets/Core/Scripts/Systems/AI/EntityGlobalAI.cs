using Game.Model;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityBase))]
[RequireComponent(typeof(Collider2D))]
public class EntityGlobalAI : MonoBehaviour
{
    [Header("Detection & Combat")]
    [SerializeField] private bool canWalk = false;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private LayerMask entityLayer;

    [Header("Animation States")]
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string walkState = "Walk";
    [SerializeField] private string attackState = "Attack";
    [SerializeField] private string hurtState = "Hurt";
    [SerializeField] private string deathState = "Death";
    [SerializeField] private string collideState = "Collide";

    [Header("Alert Emote (once only)")]
    [SerializeField] private GameObject alertPrefab;
    [SerializeField] private Transform EmoticonPoint;
    private bool alertShown = false;

    private EntityBase entity;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private bool localIsDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        entity = GetComponent<EntityBase>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        entity.OnDamagedEvent += HandleHurt;
        entity.OnDeathEvent += HandleDeath;
    }

    private void Update()
    {
        if (localIsDead || entity.IsDead || isAttacking)
            return;

        EntityBase nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            SetAnimation(idleState);
            return;
        }

        float distance = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack(nearestEnemy));
        }
        else if (distance <= detectionRange && canWalk)
        {
            MoveToward(nearestEnemy.transform.position);
            SetAnimation(walkState);
        }

        spriteRenderer.flipX = nearestEnemy.transform.position.x < transform.position.x;
    }

    // -------------------------------------------------------
    // MOVEMENT
    // -------------------------------------------------------
    private void MoveToward(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );
    }

    // -------------------------------------------------------
    // ATTACK
    // -------------------------------------------------------
    private IEnumerator PerformAttack(EntityBase target)
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        SetAnimation(attackState);

        yield return new WaitForSeconds(0.3f); // windup

        if (target != null && !target.IsDead)
        {
            int dmg = entity.Stats.AttackPower;
            target.TakeDamage(dmg);
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // FIND TARGET + ALERT ONCE
    // -------------------------------------------------------
    private EntityBase FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, entityLayer);
        float closestDist = Mathf.Infinity;
        EntityBase nearest = null;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EntityBase other = hit.GetComponent<EntityBase>();
            if (other == null || other.IsDead || other.TeamID == entity.TeamID)
                continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = other;
            }
        }

        // ★ ALERT EMOTE ONCE ONLY
        if (nearest != null && !alertShown)
            ShowAlertEmote();

        return nearest;
    }

    private void ShowAlertEmote()
    {
        if (alertShown || alertPrefab == null)
            return;

        alertShown = true;

        GameObject fx = Instantiate(
            alertPrefab,
            EmoticonPoint.position,
            Quaternion.identity,
            transform
        );

        Destroy(fx, 1.2f);
    }

    // -------------------------------------------------------
    // DAMAGE & DEATH
    // -------------------------------------------------------
    private void HandleHurt(int amount)
    {
        if (localIsDead) return;
        SetAnimation(hurtState);
    }

    private void HandleDeath()
    {
        if (localIsDead) return;

        localIsDead = true;
        SetAnimation(deathState);

        col.enabled = false;
        StartCoroutine(DeathCleanup());
    }

    private IEnumerator DeathCleanup()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // -------------------------------------------------------
    // UTILITIES
    // -------------------------------------------------------
    private void SetAnimation(string stateName)
    {
        if (!animator) return;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(stateName))
            animator.Play(stateName);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject.GetComponent<EntityBase>();
        if (other != null && other.TeamID != entity.TeamID)
        {
            SetAnimation(collideState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

    }

    private void OnDestroy()
    {
        if (entity == null) return;
        entity.OnDamagedEvent -= HandleHurt;
        entity.OnDeathEvent -= HandleDeath;
    }
}
