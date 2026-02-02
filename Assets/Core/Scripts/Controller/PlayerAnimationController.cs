using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform arrowSpawn;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.6f;

    private bool isAttacking = false;

    private void Reset()
    {
        // auto-assign in editor if possible
        animator = GetComponent<Animator>();
        var spawn = transform.Find("BowPivot/ArrowSpawn");
        if (spawn) arrowSpawn = spawn;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isAttacking)
        {
            StartCoroutine(DoAttack());
        }
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");        // triggers Attack animation
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // Called via animation event placed on Attack.anim at the fire frame
    public void FireArrow()
    {
        if (arrowPrefab == null || arrowSpawn == null) return;
        Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation);
    }
}
