using Game.Model.Player;
using System.Collections;
using UnityEngine;

/// =======================================
///   🤖 ADVANCED BOSS AI CONTROLLER
///   Smooth Movement • Seeded Patterns • Air Logic
/// =======================================
public class BossAttackController : MonoBehaviour
{
    [Header("🔍 Target")]
    public PlayerEntity playerEntity;

    [Header("⚙ Core")]
    private Animator animator;
    private Rigidbody2D rb;
    public float thinkInterval = 0.18f;

    [Header("🏃 Movement Settings")]
    public float moveAcceleration = 30f;
    public float maxMoveSpeed = 4f;
    public float retreatAcceleration = 20f;
    public float dashForce = 12f;
    public float jumpForce = 14f;
    public float hoverSpeed = 3f;
    public float airControl = 0.5f;
    public bool canHover = false;

    [Header("📏 Distance Control")]
    public float idealDistance = 3f;
    public float meleeRange = 1.8f;
    public float rangedRange = 8f;

    [Header("⚔ Attack Patterns (Prefab-based)")]
    public BossAttackPatternObject meleeAttack;
    public BossAttackPatternObject rangedAttack;
    public BossAttackPatternObject heavyAttack;

    [Header("🌎 Environment")]
    public LayerMask groundMask;

    private bool isBusy;
    private bool grounded;
    private int aiSeed;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
    }

    void Start()
    {
        aiSeed = Random.Range(0, 99999);
        Random.InitState(aiSeed);

        StartCoroutine(BrainLoop());
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundMask);
    }

    private IEnumerator BrainLoop()
    {
        while (true)
        {
            if (!isBusy && playerEntity != null)
                DecideAction();

            yield return new WaitForSeconds(thinkInterval);
        }
    }

    private void DecideAction()
    {
        float distance = Vector2.Distance(transform.position, playerEntity.transform.position);

        float scoreAttack = Random.Range(8, 20);
        float scoreApproach = distance > idealDistance ? 15 : 0;
        float scoreRetreat = distance < meleeRange ? 10 : 0;

        float score = Mathf.Max(scoreAttack, scoreApproach, scoreRetreat);

        if (score == scoreAttack) ChooseAttack(distance);
        else if (score == scoreApproach) StartCoroutine(Approach());
        else if (score == scoreRetreat) StartCoroutine(Retreat());
    }

    private IEnumerator Approach()
    {
        isBusy = true;
        float t = 0.6f;

        while (t > 0)
        {
            Vector2 dir = (playerEntity.transform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * maxMoveSpeed, rb.linearVelocity.y);
            t -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity *= 0.1f;
        isBusy = false;
    }

    private IEnumerator Retreat()
    {
        isBusy = true;
        float t = 0.6f;

        while (t > 0)
        {
            Vector2 dir = (transform.position - playerEntity.transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * maxMoveSpeed, rb.linearVelocity.y);
            t -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity *= 0.1f;
        isBusy = false;
    }

    private void ChooseAttack(float dist)
    {
        if (dist <= meleeRange)
            StartCoroutine(PerformAttack(meleeAttack));
        else if (dist <= rangedRange)
            StartCoroutine(PerformAttack(rangedAttack));
        else
            StartCoroutine(PerformAttack(heavyAttack));
    }

    private IEnumerator PerformAttack(BossAttackPatternObject atk)
    {
        isBusy = true;

        animator.Play(atk.animationState);
        yield return new WaitForSeconds(atk.windup);

        atk.attackModule?.Execute(atk.attackPoint, playerEntity.transform);

        yield return new WaitForSeconds(atk.recovery);
        isBusy = false;
    }

}
