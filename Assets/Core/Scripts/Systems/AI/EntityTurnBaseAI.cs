using Game.Model;
using System.Collections;
using UnityEngine;
using static Game.Config.Config;

[RequireComponent(typeof(EntityBase))]
public class EntityTurnBaseAI : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDelay = 0.6f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private LayerMask entityLayer;
    [SerializeField] private string attackState = "Attack";
    [SerializeField] private string idleState = "Idle";

    private EntityBase entity;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteBullet;
    private bool isActing = false;

    private void Awake()
    {
        entity = GetComponent<EntityBase>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteBullet = spriteRenderer.GetComponent<SpriteRenderer>();

        entity.OnDeathEvent += HandleDeath;
        BattleEventBus.Register(this);
    }

    private void OnDestroy()
    {
        BattleEventBus.Unregister(this);
        entity.OnDeathEvent -= HandleDeath;
    }

    private void OnEnable() => BattleEventBus.OnTurnStart += OnTurnStart;
    private void OnDisable() => BattleEventBus.OnTurnStart -= OnTurnStart;

    private void OnTurnStart(EntityTurnBaseAI current)
    {
        if (current != this || entity.IsDead) return;
        StartCoroutine(ActTurn());
    }

    private IEnumerator ActTurn()
    {
        isActing = true;

        // Choose random target from enemies
        EntityBase target = FindRandomEnemy();
        if (target == null)
        {
            Debug.Log($"{name} found no target.");
            yield return new WaitForSeconds(0.5f);
            EndTurn();
            yield break;
        }

        Debug.Log($"{name} Turn!");

        SetAnimation(EntityActionType.Attack);
        yield return new WaitForSeconds(attackDelay);

        // Apply damage
        int dmg = entity.Stats.AttackPower;
        target.TakeDamage(dmg);
        Debug.Log($"{name} attacks {target.DisplayName} for {dmg} damage!");

        yield return new WaitForSeconds(attackCooldown);

        SetAnimation(EntityActionType.Idle);
        isActing = false;
        EndTurn();
    }

    private void EndTurn()
    {
        BattleEventBus.EndTurn(this);
    }

    private void HandleDeath()
    {
        if (!entity.IsDead) return;
        Debug.Log($"{name} has died.");
    }

    private EntityBase FindRandomEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 3f, entityLayer);
        var candidates = new System.Collections.Generic.List<EntityBase>();

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            EntityBase other = hit.GetComponent<EntityBase>();
            if (other != null && !other.IsDead && other.TeamID != entity.TeamID)
                candidates.Add(other);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private void SetAnimation(EntityActionType profile)
    {
        Debug.Log("[AI] Play Animation");
        entity.PlayAction(profile);


    }

    public bool IsDead() => entity == null || entity.IsDead;
}
