using Game.Model;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Caster Settings")]
    public Transform casterContainer;
    public Transform casterContainerUltimate;
    public Transform freeCasterContainer;
    public GameObject spellPrefab;
    public GameObject spellPrefabUltimate;
    public float detectionRange = 100f;
    [SerializeField] private int targetTeamID = 2;

    // Search
    EntityBase FindTargetByTeam(int teamId)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(casterContainer.transform.position, detectionRange);
        float minDist = Mathf.Infinity;
        EntityBase closest = null;

        foreach (var hit in hits)
        {
            EntityBase e = hit.GetComponent<EntityBase>();
            if (e != null && e.TeamID == teamId && !e.IsDead)
            {
                float dist = Vector2.Distance(casterContainer.transform.position, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = e;
                }
            }
        }
        return closest;
    }

    // Prepare Init
    public void CastSpell()
    {
        if (spellPrefab == null || casterContainer == null)
            return;

        EntityBase target = FindTargetByTeam(targetTeamID);
        //if (target == null) return;
        // Create projectile
        GameObject obj = Instantiate(spellPrefab, casterContainer.transform.position, casterContainer.transform.rotation);
        // Initialize projectile data
        EntityProjectile spell = obj.GetComponent<EntityProjectile>();
        if (spell != null)
        {
            spell.Initialize(casterContainer, target, obj, freeCasterContainer);

        }
    }





    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(casterContainer.transform.position, detectionRange);

    }
}